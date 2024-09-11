using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SaveToolbox.Runtime.CallbackHandlers;
using SaveToolbox.Runtime.Core.MonoBehaviours;
using SaveToolbox.Runtime.Core.ScriptableObjects;
using SaveToolbox.Runtime.Interfaces;
using SaveToolbox.Runtime.Serialization;
using SaveToolbox.Runtime.Serialization.Binary;
using SaveToolbox.Runtime.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;
using Task = System.Threading.Tasks.Task;

namespace SaveToolbox.Runtime.Core
{
	[Serializable]
	public class SaveToolboxSystem
	{
		private const string META_DATA_NAME_SUFFIX = "_meta";

		private static SaveToolboxSystem instance;
		public static SaveToolboxSystem Instance
		{
			get
			{
				if (instance == null) instance = new SaveToolboxSystem();

				return instance;
			}
		}

		private SaveSettings SingletonSaveSettings => SaveToolboxPreferences.Instance.DefaultSaveSettings;
		private bool LoggingEnabled => SaveToolboxPreferences.Instance.LoggingEnabled;
		private int LowestAcceptableLoadingFrameRate => SaveToolboxPreferences.Instance.LowestAcceptableLoadingFrameRate;

		// TODO: Needs a better solution.
		// A way for entities to know what the current serialization settings are.
		public StbSerializationSettings CurrentSerializationSettings { get; private set; }

		// Static to allow non-monobehaviours access to attach selves to save system.
		private static List<ISaveDataEntity> nonMonoBehaviourSaveDataEntities = new List<ISaveDataEntity>();

		public static AbstractSaveDataEntityCollector saveDataEntityCollector = new StbSaveDataEntityCollector();
		public static StbJsonSerializer jsonSerializer;
		public static StbBinarySerializer binarySerializer;
		public static AbstractSaveMigrator saveMigrator = null;

		private float asyncCachedTimeScale;
		private float currentFrameTime;
		private float startFrameTime;
		private Task writeAsyncTask;
		private Task<byte[]> readAsyncTask;
		private bool isAsynchronouslySavingLoading;
		private bool isInitialized;

		public SaveData CurrentSaveData { get; private set; }
		public SaveMetaData CurrentMetaData { get; private set; }

		public StbCallbackDistributor StbCallbackDistributor { get; private set; } = new StbCallbackDistributor();
		public LoadingState LoadingState { get; private set; } = new LoadingState();
		public SavingState SavingState { get; private set; } = new SavingState();

		public event Action<LoadingState> OnLoadingStateChanged;
		public event Action<SavingState> OnSavingStateChanged;

		public SaveToolboxSystem()
		{
			jsonSerializer = new StbJsonSerializer
			{
				serializationSettings = SaveToolboxPreferences.Instance.SerializationSettings,
				UsesPrettyPrint = SaveToolboxPreferences.Instance.JsonPrettyPrint
			};

			binarySerializer = new StbBinarySerializer()
			{
				serializationSettings = SaveToolboxPreferences.Instance.SerializationSettings
			};
		}

		public bool Initialize()
		{
			if (isInitialized) return false;

			isInitialized = true;
			StbCallbackDistributor.CacheCallbackTypes();

			// Cache all types that are marked as serialed as former type.
			StbSerializationUtilities.CacheFormerTypes();
			return true;
		}

		/// <summary>
		/// Tries to load a save and if fails it will create a new save data. This will load the main STBSaveFile NOT any
		/// custom forms of save files that have been saved into a slot.
		/// </summary>
		/// <param name="slotIndex">Slot index to try load from.</param>
		/// <param name="saveMetaData">Meta data to create the new save from.</param>
		public async void LoadOrCreateNewSaveGame(int slotIndex, SaveMetaData saveMetaData = null)
		{
			Initialize();
			var request = TryLoadGameAsync(slotIndex);
			await Task.WhenAll(request);
#if STB_ASYNCHRONOUS_SAVING
			if (request.Result)
			{
				if (LoggingEnabled) Debug.Log("Couldn't load save. Creating new save.");
				if (saveMetaData == null)
				{
					NewSaveGame(slotIndex);
				}
				else
				{
					NewSaveGame(saveMetaData);
				}
			}
#else
			if (!request.Result)
			{
				if (LoggingEnabled) Debug.Log("Couldn't load save. Creating new save.");
				if (saveMetaData == null)
				{
					NewSaveGame(slotIndex);
				}
				else
				{
					NewSaveGame(saveMetaData);
				}
			}
#endif
		}

		// A region for non asynchronous methods with asynchronous counterparts.
#region NonAsynchronous

		/// <summary>
		/// Creates a new save game data taking a potential index for the slot it should save in.
		/// </summary>
		/// <param name="saveSlotIndex">The index of the slot this save should be stored in, defaults to 0. -1 would be no slot.</param>
		/// <returns>If it successfully saved or not.</returns>
		public bool TrySaveGame(int saveSlotIndex = 0)
		{
			return TrySaveGame(SingletonSaveSettings.Copy(), saveSlotIndex);
		}

		public bool TrySaveGame(SaveSettings saveSettings, int slotIndex = 0)
		{
			return TrySaveGame(new SaveMetaData(), saveSettings, slotIndex);
		}

		/// <summary>
		/// Tries to save all ISaveDataEntities. This includes any added to the save data objects list
		/// as well as any ISaveDataEntities that inherit from MonoBehaviours including Loadable Objects.
		/// </summary>
		/// <param name="saveMetaData">The meta data for this game save.</param>
		/// <param name="slotIndex">The slot the game data should be saved into.</param>
		/// <param name="saveSettings">The arguments the game should be saved with.</param>
		/// <returns>Whether it could successfully save.</returns>
		public bool TrySaveGame(SaveMetaData saveMetaData, SaveSettings saveSettings = null, int slotIndex = 0)
		{
			Initialize();
			if (saveMetaData == null) saveMetaData = new SaveMetaData();
			if (saveSettings == null) saveSettings = SingletonSaveSettings;

			var fileSaveSettings = new FileSaveSettings(saveSettings, slotIndex);
			var metaSaveDataSettings = new FileSaveSettings(fileSaveSettings);
			metaSaveDataSettings.SaveFileName += "_meta";

			CurrentSerializationSettings = saveSettings.SerializationSettings.Copy();
			CurrentMetaData = saveMetaData;

			try
			{
				if (!Directory.Exists(Application.persistentDataPath))
				{
					Directory.CreateDirectory(Application.persistentDataPath);
				}

				StbCallbackDistributor.HandleBeforeSaved();
				SetSavingState(SaveState.SavingLoadables, 0f);

				// Create the save file and the meta save file.
				var savedSuccessfully = TryGatherSlotData(saveMetaData, saveSettings) && TryCreateSaveData(CurrentSaveData, fileSaveSettings) && TryCreateSaveData(CurrentMetaData, metaSaveDataSettings);

				if (LoggingEnabled) Debug.Log($"Saved data in slot: {fileSaveSettings.SlotIndex} successfully? {savedSuccessfully}");

				if (!savedSuccessfully)
				{
					SetSavingState(SaveState.Failed, 1f);
				}

				var saveEntityLifecycles = StbUtilities.GetAllObjectsInAllScenes<ISaveEntityLifecycle>();
				foreach (var saveEntityLifecycle in saveEntityLifecycles)
				{
					saveEntityLifecycle.OnSaveCompleted();
				}

				if (savedSuccessfully)
				{
					StbCallbackDistributor.HandleSaved(CurrentSaveData);
				}

				SetSavingState(SaveState.None, 0f);

				return savedSuccessfully;
			}
			catch (Exception exception)
			{
				Debug.LogError(exception);
				return false;
			}
		}

		/// <summary>
		/// A function that gathers all the slot data from MonoBehaviours in the game. This is specifically for game saves
		/// and cannot be used for saving custom files as it would include a bunch of unnecessary data.
		/// </summary>
		/// <param name="saveMetaData">The meta data that is used when gathering data.</param>
		/// <param name="saveSettings"></param>
		/// <returns></returns>
		private bool TryGatherSlotData(SaveMetaData saveMetaData, SaveSettings saveSettings)
		{
			// Get all loadable objects in all active scenes.
			var foundLoadableObjects = saveDataEntityCollector.GetLoadableObjects();
			var serializedLoadableObjects = new List<LoadableObjectSaveData>();
			var loadableList = foundLoadableObjects.Where(loadableObject => loadableObject != null && loadableObject.IsInitialized && loadableObject.ShouldSpawnWhenLoaded).ToArray();

			// Handle loadable objects.
			for (var index = 0; index < loadableList.Length; index++)
			{
				var loadableObject = loadableList[index];
				serializedLoadableObjects.Add(loadableObject.Serialize(saveSettings));
				SetSavingState(SaveState.SavingLoadables, (float)index / loadableList.Length);
			}

			// Clear any lingering save objects.
			var serializedSaveEntityObjects = new List<SaveEntityObjectData>();

			SetSavingState(SaveState.SavingNonLoadables, 0f);
			// Find any save data MonoBehaviours that weren't referenced in a LoadableObject.
			var foundSavableMonoBehaviours = saveDataEntityCollector.GetMonoBehaviourISaveDataEntities();
			for (var index = 0; index < foundSavableMonoBehaviours.Count; index++)
			{
				var saveableMonoBehaviour = foundSavableMonoBehaviours[index];
				var newSaveData = new SaveEntityObjectData(saveableMonoBehaviour.SaveIdentifier, saveableMonoBehaviour.Serialize());
				serializedSaveEntityObjects.Add(newSaveData);
				SetSavingState(SaveState.SavingNonLoadables, (float)index / foundSavableMonoBehaviours.Count);
			}

			SetSavingState(SaveState.SavingNonMonoBehaviours, 0f);
			// Handle all the non-MonoBehaviour ISaveDataEntities that were added during runtime.
			for (var index = 0; index < nonMonoBehaviourSaveDataEntities.Count; index++)
			{
				var saveDataEntity = nonMonoBehaviourSaveDataEntities[index];
				// If the ISaveDataEntity is a MonoBehaviour (it shouldn't be as it will have been caught before
				if (saveDataEntity is MonoBehaviour _) continue;

				var newSaveData = new SaveEntityObjectData(saveDataEntity.SaveIdentifier, saveDataEntity.Serialize());
				serializedSaveEntityObjects.Add(newSaveData);
				SetSavingState(SaveState.SavingNonMonoBehaviours, (float)index / nonMonoBehaviourSaveDataEntities.Count);
			}

			// If we didn't find anything to save objects.
			if (serializedSaveEntityObjects.Count <= 0 && serializedLoadableObjects.Count <= 0)
			{
				if (LoggingEnabled) Debug.Log("Tried to save, but couldn't find any data to save.");
				return false;
			}

			var scenePaths = new List<string>();
			if (saveSettings.SaveScene)
			{
				scenePaths = GetScenePaths(saveSettings.StbSceneSavingType);
			}

			CurrentSaveData = new SaveData(serializedSaveEntityObjects, serializedLoadableObjects, scenePaths);
			CurrentMetaData = saveMetaData;
			return true;
		}

		/// <summary>
		/// Returns a list of strings that are the paths or the build index of the scenes that are currently loaded
		/// depending on the scene saving type.
		/// </summary>
		/// <param name="stbSceneSavingType">The type of way to save the scene either by build index or string.</param>
		/// <returns></returns>
		private List<string> GetScenePaths(StbSceneSavingType stbSceneSavingType)
		{
			var scenePaths = new List<string>();

#if STB_ABOVE_2022_2
			var sceneCount = SceneManager.loadedSceneCount;
#else
			var sceneCount = SceneManager.sceneCount;
#endif

			switch (stbSceneSavingType)
			{
				case StbSceneSavingType.Path:
					// First, we want to add the active scene.
					scenePaths.Add(SceneManager.GetActiveScene().path);

					for (var i = 0; i < sceneCount; i++)
					{
						var scenePath = SceneManager.GetSceneAt(i).path;
						if (scenePaths.Contains(scenePath)) continue;

						scenePaths.Add(scenePath);
					}
					break;
				case StbSceneSavingType.BuildIndex:
					// First, we want to add the active scene.
					scenePaths.Add(SceneManager.GetActiveScene().buildIndex.ToString());
					for (var i = 0; i < sceneCount; i++)
					{
						var scenePath = SceneManager.GetSceneAt(i).buildIndex.ToString();
						if (scenePaths.Contains(scenePath)) continue;

						scenePaths.Add(scenePath);
					}
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			return scenePaths;
		}

		/// <summary>
		/// Try to load the data in the slot.
		/// </summary>
		/// <param name="slotIndex">The index of the slot. Defaults to 0.</param>
		/// <returns>Whether or not the data in slot index was able to be loaded correctly.</returns>
		public bool TryLoadGame(int slotIndex = 0)
		{
			return TryLoadGame(SingletonSaveSettings, slotIndex);
		}

		/// <summary>
		/// Try load the data using the specified args.
		/// </summary>
		/// <param name="saveSettings">The save settings to be used.</param>
		/// <param name="slotIndex">The slot index in which the data should be loaded from.</param>
		/// <returns>Whether or not the data in slot index was able to be loaded correctly.</returns>
		public bool TryLoadGame(SaveSettings saveSettings, int slotIndex = 0)
		{
			Initialize();
			var loadedSuccessfully = false;
			if (saveSettings == null) saveSettings = SingletonSaveSettings;

			var fileSaveSettings = new FileSaveSettings(saveSettings, slotIndex);

			try
			{
				var loadPath = GetSlotSaveDefaultFilePath(fileSaveSettings);
				if (File.Exists(loadPath))
				{
					StbCallbackDistributor.HandleBeforeLoaded();
					SetLoadingState(LoadState.LoadingObjects, 0f);
					CurrentSaveData = LoadData<SaveData>(fileSaveSettings);
					TryMigrateGameSave();
					ApplySaveDataToGame(saveSettings);
					SetLoadingState(LoadState.None, 0f);
					loadedSuccessfully = true;
				}
				else
				{
					SetLoadingState(LoadState.Failed, 1f);
					if (LoggingEnabled) Debug.LogError($"Could not load save in path {loadPath}. As it could not be found.");
				}
			}
			catch (Exception exception)
			{
				Debug.LogError(exception);
			}

			if (LoggingEnabled) Debug.Log($"Loaded Data in slot {fileSaveSettings.SlotIndex} successfully? {loadedSuccessfully}");
			if (loadedSuccessfully)
			{
				StbCallbackDistributor.HandleLoaded(CurrentSaveData);
			}
			return loadedSuccessfully;
		}

		/// <summary>
		/// Load the data in the slot index into the current slot save data. Also applies the loaded data to all MonoBehaviour ISaveDataEntities.
		/// </summary>
		/// <param name="fileSaveSettings">Save data arguments.</param>
		/// <exception cref="ArgumentOutOfRangeException">Unknown file type.</exception>
		public T LoadData<T>(FileSaveSettings fileSaveSettings)
		{
			Initialize();
			var loadedBytes = File.ReadAllBytes(GetSlotSaveDefaultFilePath(fileSaveSettings));
			var encryptionSettings = fileSaveSettings.StbEncryptionSettings;
			var decryptedData = StbSaveEncryption.Decrypt(loadedBytes, encryptionSettings.EncryptionType, encryptionSettings.EncryptionKeyword, encryptionSettings.EncryptionInitializationVector);
			var uncompressedData = StbSaveCompression.Decompress(decryptedData, fileSaveSettings.CompressionType);

			switch (fileSaveSettings.SaveFileFormat)
			{
				case StbFileFormat.Binary:
					var selectedBinarySerializer = fileSaveSettings.CustomBinarySerializer ?? binarySerializer;
					CurrentSerializationSettings = selectedBinarySerializer.serializationSettings.Copy();
					return selectedBinarySerializer.FromBinary<T>(uncompressedData);
				case StbFileFormat.Json:
					var loadedJsonString = Encoding.UTF8.GetString(uncompressedData);
					var selectedJsonSerializer = fileSaveSettings.CustomJsonSerializer ?? jsonSerializer;
					CurrentSerializationSettings = selectedJsonSerializer.serializationSettings.Copy();
					return selectedJsonSerializer.FromJson<T>(loadedJsonString);
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		/// <summary>
		/// Apply Current SaveData to all MonoBehaviour ISaveDataEntities including loadable objects.
		/// 1. Cache all current objects in the scene.
		/// 2. Spawn all loadable objects that don't exist in the scene.
		/// 3. Deserialize any data for the respective loadable objects.
		/// 4. Read the loadable object in the correct scene.
		/// 5. Deserialize all data from SaveableMonoBehaviours that aren't inside loadable objects.
		/// </summary>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		private async void ApplySaveDataToGame(SaveSettings saveSettings)
		{
			// Find all current loadable objects.
			SetLoadingState(LoadState.LoadingObjects, 0f);
			var currentLoadableObjects = saveDataEntityCollector.GetLoadableObjects();
			var currentLoadableObjectDictionary = new Dictionary<string, LoadableObject>();

			// Destroy all the loadable objects if they should be rebuilt.
			if (saveSettings.RebuildLoadableObjects)
			{
				for (var index = currentLoadableObjects.Count - 1; index >= 0; index--)
				{
					Object.DestroyImmediate(currentLoadableObjects[index].gameObject);
					currentLoadableObjects.RemoveAt(index);
				}
			}
			else
			{
				// Create a dictionary for lookup.
				currentLoadableObjectDictionary = currentLoadableObjects.ToDictionary(currentLoadableObject => currentLoadableObject.Identifier);
			}

			// Spawn any loadable objects and setup any behaviours on them..
			for (var index = 0; index < CurrentSaveData.LoadableObjectSaveEntityDatas.Count(); index++)
			{
				var loadableObjectSaveData = CurrentSaveData.LoadableObjectSaveEntityDatas[index];

				// Create the new instance and have the loadable object deserialize the data.
				if (!LoadableObjectDatabase.Instance.TryGetLoadableObjectById(loadableObjectSaveData.LoadableObjectId, out var loadableObject))
				{
					Debug.LogError($"Could not find loadable object of Id {loadableObjectSaveData.LoadableObjectId}, Ensure it is in the loadable object database. Either refresh or rebuild.");
					continue;
				}

				LoadableObject newLoadableObject;
				if (!saveSettings.RebuildLoadableObjects && currentLoadableObjectDictionary.ContainsKey(loadableObjectSaveData.Identifier))
				{
					newLoadableObject = currentLoadableObjectDictionary[loadableObjectSaveData.Identifier];
					currentLoadableObjectDictionary.Remove(loadableObjectSaveData.Identifier);
				}
				else
				{
					// If it doesn't exist yet, create it.
					newLoadableObject = Object.Instantiate(loadableObject);
				}

				var iSaveEntityLifecycle = newLoadableObject.GetComponentsInChildren<ISaveEntityLifecycle>();
				foreach (var saveEntityLifecycle in iSaveEntityLifecycle)
				{
					saveEntityLifecycle.OnLoadingSpawned();
				}

				newLoadableObject.Deserialize(loadableObjectSaveData);
				currentLoadableObjectDictionary.Remove(loadableObjectSaveData.Identifier);

				// If we save the scene put it in the correct scene.
				if (saveSettings.SaveScene) await LoadObjectsScene(loadableObjectSaveData, newLoadableObject.gameObject, saveSettings.StbSceneSavingType);

				SetLoadingState(LoadState.LoadingObjects, (float)index / CurrentSaveData.LoadableObjectSaveEntityDatas.Count);
			}

			// Destroy any objects that pre-existed & weren't loaded to.
			foreach (var loadableObject in currentLoadableObjectDictionary.Values)
			{
				Object.DestroyImmediate(loadableObject.gameObject);
			}
			currentLoadableObjectDictionary.Clear();

			// Load all scenes that have not already been loaded when loading the loadable objects and set the active scene.
			if (saveSettings.SaveScene) LoadScenes(CurrentSaveData.ScenePaths, saveSettings.StbSceneSavingType);

			//Get all objects implementing ISaveDataEntity and deserialize their data.
			SetLoadingState(LoadState.ApplyingData, 0f);

			StbUtilities.CacheSaveDataEntities();

			var saveDataEntityObjects = saveDataEntityCollector.GetAllISaveDataEntities();
			for (var index = 0; index < saveDataEntityObjects.Count; index++)
			{
				var saveDataEntity = saveDataEntityObjects[index];
				foreach (var saveDataEntityObject in CurrentSaveData.SaveDataEntityObjects)
				{
					if (saveDataEntityObject.Identifier == saveDataEntity.SaveIdentifier)
					{
						saveDataEntity.Deserialize(saveDataEntityObject.ObjectValue);
					}
					SetLoadingState(LoadState.ApplyingData, (float)index / saveDataEntityObjects.Count);
				}
			}

			if (saveSettings.PhysicsSyncTransformsOnLoad)
			{
				Physics.SyncTransforms();
				Physics2D.SyncTransforms();
			}

			// Let all the save entities know the data was loaded.
			var saveLifecycleHandlers = StbUtilities.GetAllObjectsInAllScenes<ISaveEntityLifecycle>();
			foreach (var saveLifecycleHandler in saveLifecycleHandlers)
			{
				saveLifecycleHandler.OnLoadCompleted();
			}
		}

		/// <summary>
		/// Loads the list of scenes from the scene paths. This first entry in the list will be treated as the active scene.
		/// Will also unload any scenes that are not in this list.
		/// </summary>
		/// <param name="scenePaths">The scene paths which are either the path of the build index.</param>
		/// <param name="stbSceneSavingType">The scene saving type which denotes whether it was saved by build index or path.</param>
		private async void LoadScenes(List<string> scenePaths, StbSceneSavingType stbSceneSavingType)
		{
			for (var index = 0; index < scenePaths.Count; index++)
			{
				var scenePath = scenePaths[index];
				// Load any scenes that were saved but are currently not loaded.
				if (!string.IsNullOrEmpty(scenePath))
				{
					Scene scene;
					switch (stbSceneSavingType)
					{
						case StbSceneSavingType.Path:
							scene = SceneManager.GetSceneByPath(scenePath);
							break;
						case StbSceneSavingType.BuildIndex:
							var buildIndex = int.Parse(scenePath);
							scene = SceneManager.GetSceneByBuildIndex(buildIndex);
							break;
						default:
							throw new ArgumentOutOfRangeException();
					}

					// If it is not valid it is either not loaded or doesn't exist.
					if (!scene.IsValid())
					{
						if (LoggingEnabled) Debug.LogError($"Attempting to load scene at path. {CurrentSaveData.ScenePaths}");
						await LoadSceneTask(scenePath, stbSceneSavingType);
					}

					// The first scene in the paths, becomes the active scene.
					if (index == 0)
					{
						SceneManager.SetActiveScene(scene);
					}
				}
			}

			// Collect all scenes to unload.
			var scenesToUnloadList = new List<Scene>();

#if STB_ABOVE_2022_2
			var sceneCount = SceneManager.loadedSceneCount;
#else
			var sceneCount = SceneManager.sceneCount;
#endif

			for (var i = 0; i < sceneCount; i++)
			{
				var loadedScene = SceneManager.GetSceneAt(i);

				switch (stbSceneSavingType)
				{
					case StbSceneSavingType.Path:
						if (!scenePaths.Contains(loadedScene.path))
						{
							scenesToUnloadList.Add(loadedScene);
						}
						break;
					case StbSceneSavingType.BuildIndex:
						if (!scenePaths.Contains(loadedScene.buildIndex.ToString()))
						{
							scenesToUnloadList.Add(loadedScene);
						}
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}

			// Unload scenes that should be unloaded.
			foreach (var sceneToUnload in scenesToUnloadList)
			{
				var asyncOperation = SceneManager.UnloadSceneAsync(sceneToUnload);
				if (asyncOperation == null)
				{
					Debug.LogError("Tried to unload a scene that doesn't exist or isn't valid.");
					continue;
				}
				await asyncOperation.ConvertToTask();
			}
		}

		private async Task LoadSceneTask(string scenePath, StbSceneSavingType stbSceneSavingType, bool isAdditive = true)
		{
			Task sceneLoadTask = default;
			switch (stbSceneSavingType)
			{
				case StbSceneSavingType.Path:
					var pathAsyncOperation = SceneManager.LoadSceneAsync(scenePath, isAdditive ? LoadSceneMode.Additive : LoadSceneMode.Single);
					if (pathAsyncOperation == null)
					{
						Debug.LogError($"Could not load scene as it was either not in build settings or it's path didn't exist. Path: {scenePath}");
						break;
					}
					sceneLoadTask = pathAsyncOperation.ConvertToTask();
					break;
				case StbSceneSavingType.BuildIndex:
					var buildIndex = int.Parse(scenePath);
					var buildIndexAsyncOperation = SceneManager.LoadSceneAsync(buildIndex, isAdditive ? LoadSceneMode.Additive : LoadSceneMode.Single);
					if (buildIndexAsyncOperation == null)
					{
						Debug.LogError($"Could not load scene as it was either not in build settings or it's path didn't exist. Path: {scenePath}");
						break;
					}
					sceneLoadTask = buildIndexAsyncOperation.ConvertToTask();
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			if (sceneLoadTask != null) await sceneLoadTask;
		}

#endregion

		// A region for asynchronous methods with non-asynchronous counterparts.
#region Asynchronous

		/// <summary>
		/// Saves the game data asynchronously. Takes the save data slot index as a parameter.
		/// </summary>
		/// <param name="slotIndex">The index of the slot this save should be stored.</param>
		/// <returns>A task of type generic bool that dictates if it saved successfully or not.</returns>
		public async Task<bool> TrySaveGameAsync(int slotIndex = 0)
		{
			return await TrySaveGameAsync(SingletonSaveSettings.Copy(), slotIndex);
		}

		public async Task<bool> TrySaveGameAsync(SaveSettings saveSettings, int slotIndex = 0)
		{
			return await TrySaveGameAsync(new SaveMetaData(), saveSettings, slotIndex);
		}

		/// <summary>
		/// Saves the game data asynchronously. Takes save meta data as the parameter.
		/// </summary>
		/// <param name="saveMetaData">The meta that should be stored in the save.</param>
		/// <param name="saveSettings">The args to save the data with</param>
		/// <param name="slotIndex">The slot the file should be saved under.</param>
		/// <returns>A task of type generic bool that dictates if it saved successfully or not.</returns>
		public async Task<bool> TrySaveGameAsync(SaveMetaData saveMetaData, SaveSettings saveSettings = null, int slotIndex = 0)
		{
			Initialize();
			if (saveMetaData == null) saveMetaData = new SaveMetaData(0);
			if (saveSettings == null) saveSettings = SingletonSaveSettings;

			var fileSaveSettings = new FileSaveSettings(saveSettings, slotIndex);
			var metaSaveDataArgs = new FileSaveSettings(fileSaveSettings);
			metaSaveDataArgs.SaveFileName += "_meta";

			CurrentSerializationSettings = saveSettings.SerializationSettings.Copy();
			CurrentMetaData = saveMetaData;

			try
			{
				if (isAsynchronouslySavingLoading) return false;

				isAsynchronouslySavingLoading = true;
				SetTimeScaleFrozenIfPossible(saveSettings, true);

				if (!Directory.Exists(Application.persistentDataPath))
				{
					Directory.CreateDirectory(Application.persistentDataPath);
				}

				startFrameTime = Time.realtimeSinceStartup;
				currentFrameTime = Time.realtimeSinceStartup;

				StbCallbackDistributor.HandleBeforeSaved();
				SetSavingState(SaveState.SavingLoadables, 0f);

				var savedSuccessfully = await TryGatherSlotDataAsync(saveMetaData, saveSettings) && await TryCreateSaveDataAsync(CurrentSaveData, fileSaveSettings) && await TryCreateSaveDataAsync(CurrentMetaData, metaSaveDataArgs);

				if (LoggingEnabled) Debug.Log($"Saved data in slot: {fileSaveSettings.SlotIndex} successfully? {savedSuccessfully}");

				if (!savedSuccessfully)
				{
					SetSavingState(SaveState.Failed, 1f);
				}

				var saveEntityLifecycles = StbUtilities.GetAllObjectsInAllScenes<ISaveEntityLifecycle>();
				foreach (var saveEntityLifecycle in saveEntityLifecycles)
				{
					saveEntityLifecycle.OnSaveCompleted();
				}

				if (savedSuccessfully)
				{
					StbCallbackDistributor.HandleSaved(CurrentSaveData);
				}

				SetSavingState(SaveState.None, 0f);

				isAsynchronouslySavingLoading = false;
				SetTimeScaleFrozenIfPossible(saveSettings, false);

				return savedSuccessfully;
			}
			catch (Exception exception)
			{
				// Unfreeze if it fails.
				SetTimeScaleFrozenIfPossible(saveSettings, false);
				isAsynchronouslySavingLoading = false;
				Debug.LogError(exception);
				return false;
			}
		}

		private async Task<bool> TryGatherSlotDataAsync(SaveMetaData saveMetaData, SaveSettings saveSettings)
		{
			// Get all loadable objects in all active scenes.
			var foundLoadableObjects = saveDataEntityCollector.GetLoadableObjects();
			var serializedLoadableObjects = new List<LoadableObjectSaveData>();
			var loadableList = foundLoadableObjects.Where(loadableObject => loadableObject != null && loadableObject.IsInitialized && loadableObject.ShouldSpawnWhenLoaded).ToArray();

			// Handle loadable objects.
			for (var index = 0; index < loadableList.Length; index++)
			{
				var loadableObject = loadableList[index];
				serializedLoadableObjects.Add(loadableObject.Serialize(saveSettings));
				SetSavingState(SaveState.SavingLoadables, (float)index / loadableList.Length);
				await CheckFrameTime();
			}

			// Clear any lingering save objects.
			var serializedSaveEntityObjects = new List<SaveEntityObjectData>();

			SetSavingState(SaveState.SavingNonLoadables, 0f);
			// Find any save data MonoBehaviours that weren't referenced in a LoadableObject.
			var foundSavableMonoBehaviours = saveDataEntityCollector.GetMonoBehaviourISaveDataEntities();
			for (var index = 0; index < foundSavableMonoBehaviours.Count; index++)
			{
				var saveableMonoBehaviour = foundSavableMonoBehaviours[index];
				var newSaveData = new SaveEntityObjectData(saveableMonoBehaviour.SaveIdentifier, saveableMonoBehaviour.Serialize());
				serializedSaveEntityObjects.Add(newSaveData);
				SetSavingState(SaveState.SavingNonLoadables, (float)index / foundSavableMonoBehaviours.Count);
				await CheckFrameTime();
			}

			SetSavingState(SaveState.SavingNonMonoBehaviours, 0f);
			// Handle all the non-MonoBehaviour ISaveDataEntities that were added during runtime.
			for (var index = 0; index < nonMonoBehaviourSaveDataEntities.Count; index++)
			{
				var saveDataEntity = nonMonoBehaviourSaveDataEntities[index];
				// If the ISaveDataEntity is a monobehaviour (it shouldn't be as it will have been caught before
				if (saveDataEntity is MonoBehaviour _) continue;

				var newSaveData = new SaveEntityObjectData(saveDataEntity.SaveIdentifier, saveDataEntity.Serialize());
				serializedSaveEntityObjects.Add(newSaveData);
				SetSavingState(SaveState.SavingNonMonoBehaviours, (float)index / nonMonoBehaviourSaveDataEntities.Count);
				await CheckFrameTime();
			}

			// If we didn't find anything to save objects.
			if (serializedSaveEntityObjects.Count <= 0 && serializedLoadableObjects.Count <= 0)
			{
				if (LoggingEnabled) Debug.Log("Tried to save, but couldn't find any data to save.");
				return false;
			}

			var scenePaths = new List<string>();
			if (saveSettings.SaveScene)
			{
				scenePaths = GetScenePaths(saveSettings.StbSceneSavingType);
			}
			CurrentSaveData = new SaveData(serializedSaveEntityObjects, serializedLoadableObjects, scenePaths);
			CurrentMetaData = saveMetaData;
			return true;
		}

		public async Task<bool> TryCreateSaveDataAsync(object saveObject, FileSaveSettings fileSaveSettings)
		{
			Initialize();
			var saveFilePath = GetSlotSaveDefaultFilePath(fileSaveSettings);
			var saveDirectory = GetSaveDefaultFileDirectory(fileSaveSettings);

			if (!Directory.Exists(saveDirectory))
			{
				Directory.CreateDirectory(saveDirectory);
			}

			// If it already exists, overwrite.
			var existingFilePaths = Directory.GetFiles(saveDirectory);
			foreach (var existingFilePath in existingFilePaths)
			{
				if (Path.GetFileNameWithoutExtension(existingFilePath) == fileSaveSettings.SaveFileName)
				{
					File.Delete(existingFilePath);
					if (LoggingEnabled) Debug.Log($"File at path: {saveDirectory}/{fileSaveSettings.SaveFileName} already Exists, overwriting.");
				}
			}

			switch (fileSaveSettings.SaveFileFormat)
			{
				// Save file works as type, then data.
				case StbFileFormat.Json:
				{
					var serializer = fileSaveSettings.CustomJsonSerializer ?? jsonSerializer;
					CurrentSerializationSettings = serializer.serializationSettings.Copy();
					var jsonSlotSaveData = await serializer.ToJsonAsync(saveObject);
					var byteData = Encoding.UTF8.GetBytes(jsonSlotSaveData);
					var compressedData = StbSaveCompression.Compress(byteData, fileSaveSettings.CompressionType);
					var encryptedData = StbSaveEncryption.Encrypt(compressedData, fileSaveSettings.StbEncryptionSettings.EncryptionType, fileSaveSettings.StbEncryptionSettings.EncryptionKeyword, fileSaveSettings.StbEncryptionSettings.EncryptionInitializationVector);

					if (encryptedData == null)
					{
						if (LoggingEnabled) Debug.Log("Tried to save but the serialized data was null.");
						return false;
					}

					if (writeAsyncTask != null && !writeAsyncTask.IsCompleted) writeAsyncTask.Dispose();
					writeAsyncTask = WriteAllBytesAsync(saveFilePath, encryptedData);
					while (!writeAsyncTask.IsCompleted) await CheckFrameTime();
					return true;
				}

				case StbFileFormat.Binary:
				{
					var selectedBinarySerializer = fileSaveSettings.CustomBinarySerializer ?? binarySerializer;
					CurrentSerializationSettings = selectedBinarySerializer.serializationSettings.Copy();
					var serializedData = await selectedBinarySerializer.ToBinaryAsync(saveObject);
					var compressedData = StbSaveCompression.Compress(serializedData, fileSaveSettings.CompressionType);
					var encryptedData = StbSaveEncryption.Encrypt(compressedData, fileSaveSettings.StbEncryptionSettings.EncryptionType, fileSaveSettings.StbEncryptionSettings.EncryptionKeyword, fileSaveSettings.StbEncryptionSettings.EncryptionInitializationVector);

					if (encryptedData == null)
					{
						if (LoggingEnabled) Debug.Log("Tried to save but the serialized data was null.");
						return false;
					}

					if (writeAsyncTask != null && !writeAsyncTask.IsCompleted) writeAsyncTask.Dispose();
					writeAsyncTask = WriteAllBytesAsync(saveFilePath, encryptedData);
					while (!writeAsyncTask.IsCompleted) await CheckFrameTime();
					return true;
				}
			}

			return false;
		}

		public async Task<bool> TryLoadGameAsync(int slotIndex = 0)
		{
			return await TryLoadGameAsync(SingletonSaveSettings, slotIndex);
		}

		public async Task<bool> TryLoadGameAsync(SaveSettings saveSettings, int slotIndex = 0)
		{
			Initialize();
			// If we're already loading/saving game data return false.
			if (isAsynchronouslySavingLoading) return false;

			var loadedSuccessfully = false;
			if (saveSettings == null) saveSettings = SingletonSaveSettings;

			var fileSaveSettings = new FileSaveSettings(saveSettings, slotIndex);

			try
			{
				var loadPath = GetSlotSaveDefaultFilePath(fileSaveSettings);
				if (File.Exists(loadPath))
				{
					isAsynchronouslySavingLoading = true;
					SetTimeScaleFrozenIfPossible(saveSettings, true);

					StbCallbackDistributor.HandleBeforeLoaded();
					SetLoadingState(LoadState.LoadingObjects, 0f);
					CurrentSaveData = await LoadDataAsync<SaveData>(fileSaveSettings);
					TryMigrateGameSave();
					await ApplySaveDataToGameAsync(saveSettings);
					SetLoadingState(LoadState.None, 0f);

					SetTimeScaleFrozenIfPossible(saveSettings, false);
					isAsynchronouslySavingLoading = false;
					loadedSuccessfully = true;
				}
				else
				{
					SetLoadingState(LoadState.Failed, 1f);
					isAsynchronouslySavingLoading = false;
					if (LoggingEnabled) Debug.LogError($"Could not load save in path {loadPath}. As it could not be found.");
				}
			}
			catch (Exception exception)
			{
				// Reset timescale even if it fails.
				SetTimeScaleFrozenIfPossible(saveSettings, false);
				Debug.LogError(exception);
			}

			if (LoggingEnabled) Debug.Log($"Loaded Data in slot {fileSaveSettings.SlotIndex} successfully? {loadedSuccessfully}");
			if (loadedSuccessfully)
			{
				StbCallbackDistributor.HandleLoaded(CurrentSaveData);
			}
			return loadedSuccessfully;
		}

		private void SetTimeScaleFrozenIfPossible(SaveSettings saveSettings, bool shouldFreeze)
		{
			if (!saveSettings.FreezeTimeScaleOnAsynchronousSaveLoad) return;

			if (shouldFreeze)
			{
				asyncCachedTimeScale = Time.timeScale;
				Time.timeScale = 0f;
			}
			else
			{
				Time.timeScale = asyncCachedTimeScale;
			}
		}

		private async Task<byte[]> ReadAllBytesAsync(string filePath)
		{
#if STB_ABOVE_2021_3
			var loadedBytes = await File.ReadAllBytesAsync(filePath);
#else
			var loadedBytes = File.ReadAllBytes(filePath);
#endif
			return loadedBytes;
		}

		private async Task WriteAllBytesAsync(string saveFilePath, byte[] bytes)
		{
#if STB_ABOVE_2021_3
			await File.WriteAllBytesAsync(saveFilePath, bytes);
#else
			File.WriteAllBytes(saveFilePath, bytes);
#endif
		}

		public async Task<T> LoadDataAsync<T>(FileSaveSettings fileSaveSettings)
		{
			Initialize();
			// Not awaiting it causes the synchronous version to be called for whatever reason.
			readAsyncTask = ReadAllBytesAsync(GetSlotSaveDefaultFilePath(fileSaveSettings));
			while (!readAsyncTask.IsCompleted) await CheckFrameTime();

			var loadedBytes = readAsyncTask.Result;
			var encryptionSettings = fileSaveSettings.StbEncryptionSettings;
			var decryptedData = StbSaveEncryption.Decrypt(loadedBytes, encryptionSettings.EncryptionType, encryptionSettings.EncryptionKeyword, encryptionSettings.EncryptionInitializationVector);
			var uncompressedData = StbSaveCompression.Decompress(decryptedData, fileSaveSettings.CompressionType);

			switch (fileSaveSettings.SaveFileFormat)
			{
				case StbFileFormat.Binary:
					var selectedBinarySerializer = fileSaveSettings.CustomBinarySerializer ?? binarySerializer;
					CurrentSerializationSettings = selectedBinarySerializer.serializationSettings.Copy();
					var deserializeAsyncTask = await selectedBinarySerializer.FromBinaryAsync<T>(uncompressedData);
					return deserializeAsyncTask;

				case StbFileFormat.Json:
					var loadedJsonString = Encoding.UTF8.GetString(uncompressedData);
					var selectedJsonSerializer = fileSaveSettings.CustomJsonSerializer ?? jsonSerializer;
					CurrentSerializationSettings = selectedJsonSerializer.serializationSettings.Copy();
					var returnValue = await selectedJsonSerializer.FromJsonAsync<T>(loadedJsonString);
					return returnValue;

				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		private async Task ApplySaveDataToGameAsync(SaveSettings saveSettings)
		{
			// Find all current loadable objects.
			SetLoadingState(LoadState.LoadingObjects, 0f);
			var currentLoadableObjects = saveDataEntityCollector.GetLoadableObjects();
			var currentLoadableObjectDictionary = new Dictionary<string, LoadableObject>();

			startFrameTime = Time.realtimeSinceStartup;
			currentFrameTime = startFrameTime;

			// Destroy all the loadable objects if they should be rebuilt.
			if (saveSettings.RebuildLoadableObjects)
			{
				for (var index = currentLoadableObjects.Count - 1; index >= 0; index--)
				{
					Object.DestroyImmediate(currentLoadableObjects[index].gameObject);
					currentLoadableObjects.RemoveAt(index);
					await CheckFrameTime();
				}
			}
			else
			{
				// Create a dictionary for lookup.
				currentLoadableObjectDictionary = currentLoadableObjects.ToDictionary(currentLoadableObject => currentLoadableObject.Identifier);
			}

			// Spawn any loadable objects and setup any behaviours on them..
			for (var index = 0; index < CurrentSaveData.LoadableObjectSaveEntityDatas.Count; index++)
			{
				var loadableObjectSaveData = CurrentSaveData.LoadableObjectSaveEntityDatas[index];

				// Create the new instance and have the loadable object deserialize the data.
				if (!LoadableObjectDatabase.Instance.TryGetLoadableObjectById(loadableObjectSaveData.LoadableObjectId, out var loadableObject))
				{
					Debug.LogError($"Could not find loadable object of Id {loadableObjectSaveData.LoadableObjectId}, Ensure it is in the loadable object database. Either refresh or rebuild.");
					continue;
				}

				LoadableObject newLoadableObject;
				if (!saveSettings.RebuildLoadableObjects && currentLoadableObjectDictionary.ContainsKey(loadableObjectSaveData.Identifier))
				{
					// Destroy old version of object to protect against any small changes.
					newLoadableObject = currentLoadableObjectDictionary[loadableObjectSaveData.Identifier];
					currentLoadableObjectDictionary.Remove(loadableObjectSaveData.Identifier);
				}
				else
				{
					newLoadableObject = Object.Instantiate(loadableObject);
				}

				var iSaveEntityLifecycle = newLoadableObject.GetComponentsInChildren<ISaveEntityLifecycle>();
				foreach (var saveEntityLifecycle in iSaveEntityLifecycle)
				{
					saveEntityLifecycle.OnLoadingSpawned();
				}

				newLoadableObject.Deserialize(loadableObjectSaveData);
				currentLoadableObjectDictionary.Remove(loadableObjectSaveData.Identifier);

				// If we save the scene put it in the correct scene.
				if (saveSettings.SaveScene) await LoadObjectsScene(loadableObjectSaveData, newLoadableObject.gameObject, saveSettings.StbSceneSavingType);

				SetLoadingState(LoadState.LoadingObjects, (float)index / CurrentSaveData.LoadableObjectSaveEntityDatas.Count);
				await CheckFrameTime();
			}

			// Destroy any objects that pre-existed & weren't loaded to.
			foreach (var loadableObject in currentLoadableObjectDictionary.Values)
			{
				Object.DestroyImmediate(loadableObject.gameObject);
				await CheckFrameTime();
			}
			currentLoadableObjectDictionary.Clear();

			// Load all scenes that have not already been loaded when loading the loadable objects and set the active scene.
			if (saveSettings.SaveScene) LoadScenes(CurrentSaveData.ScenePaths, saveSettings.StbSceneSavingType);

			//Get all objects implementing ISaveDataEntity and deserialize their data.
			SetLoadingState(LoadState.ApplyingData, 0f);

			StbUtilities.CacheSaveDataEntities();

			var saveDataEntityObjects = saveDataEntityCollector.GetAllISaveDataEntities();
			for (var index = 0; index < saveDataEntityObjects.Count; index++)
			{
				var saveDataEntity = saveDataEntityObjects[index];
				foreach (var saveDataEntityObject in CurrentSaveData.SaveDataEntityObjects)
				{
					if (saveDataEntityObject.Identifier == saveDataEntity.SaveIdentifier)
					{
						saveDataEntity.Deserialize(saveDataEntityObject.ObjectValue);
					}
					SetLoadingState(LoadState.ApplyingData, (float)index / saveDataEntityObjects.Count);
				}
				await CheckFrameTime();
			}

			if (saveSettings.PhysicsSyncTransformsOnLoad)
			{
				Physics.SyncTransforms();
				Physics2D.SyncTransforms();
			}

			// Let all the save entities know the data was loaded.
			var saveLifecycleHandlers = StbUtilities.GetAllObjectsInAllScenes<ISaveEntityLifecycle>();
			foreach (var saveLifecycleHandler in saveLifecycleHandlers)
			{
				saveLifecycleHandler.OnLoadCompleted();
			}
		}

		private async Task LoadObjectsScene(LoadableObjectSaveData loadableObjectSaveData, GameObject newLoadableObject, StbSceneSavingType stbSceneSavingType)
		{
			var scenePath = loadableObjectSaveData.ScenePath;
			if (!string.IsNullOrEmpty(scenePath))
			{
				switch (stbSceneSavingType)
				{
					case StbSceneSavingType.Path:
						var pathScene = SceneManager.GetSceneByPath(scenePath);
						// If it is not valid it is either not loaded or doesn't exist.
						if (!pathScene.IsValid())
						{
							if (LoggingEnabled) Debug.Log($"Attempting to load scene at path. {pathScene}");
							await LoadSceneTask(scenePath, stbSceneSavingType);
						}
						break;
					case StbSceneSavingType.BuildIndex:
						if (!int.TryParse(scenePath, out var buildIndex)) break;
						var buildIndexScene = SceneManager.GetSceneByBuildIndex(buildIndex);
						// If it is not valid it is either not loaded or doesn't exist.
						if (!buildIndexScene.IsValid())
						{
							if (LoggingEnabled) Debug.Log($"Attempting to load scene at path. {buildIndexScene}");
							await LoadSceneTask(scenePath, stbSceneSavingType);
						}
						break;
					default:
						throw new ArgumentOutOfRangeException(nameof(stbSceneSavingType), stbSceneSavingType, null);
				}

				switch (stbSceneSavingType)
				{
					case StbSceneSavingType.Path:
						var pathScene = SceneManager.GetSceneByPath(scenePath);
						if (pathScene.IsValid() && pathScene.isLoaded)
						{
							SceneManager.MoveGameObjectToScene(newLoadableObject, pathScene);
						}
						break;
					case StbSceneSavingType.BuildIndex:
						if (!int.TryParse(scenePath, out var buildIndex)) break;

						var buildIndexScene = SceneManager.GetSceneByBuildIndex(buildIndex);
						if (buildIndexScene.IsValid() && buildIndexScene.isLoaded)
						{
							SceneManager.MoveGameObjectToScene(newLoadableObject, buildIndexScene);
						}
						break;
					default:
						throw new ArgumentOutOfRangeException(nameof(stbSceneSavingType), stbSceneSavingType, null);
				}
			}
		}

		private async Task CheckFrameTime()
		{
			currentFrameTime = Time.realtimeSinceStartup;
			var difference = currentFrameTime - startFrameTime;
			if (difference > 1f / LowestAcceptableLoadingFrameRate)
			{
				await Task.Yield();
				startFrameTime = Time.realtimeSinceStartup;
			}
		}
#endregion

		private void TryMigrateGameSave()
		{
			if (saveMigrator != null)
			{
				CurrentSaveData = saveMigrator.Migrate(CurrentSaveData, CurrentMetaData);
			}
		}

		/// <summary>
		/// Returns the save file path for a specific slot.
		/// </summary>
		/// <param name="fileSaveSettings">Save data arguments.</param>
		/// <returns>The save file path for the index.</returns>
		public string GetSlotSaveDefaultFilePath(FileSaveSettings fileSaveSettings)
		{
			if (fileSaveSettings.SlotIndex < 0)
			{
				if (!string.IsNullOrEmpty(fileSaveSettings.CustomSavePath))
				{
					return $"{fileSaveSettings.CustomSavePath}/{fileSaveSettings.SaveFileName}{StbUtilities.GetAssetTypeFileExtension(fileSaveSettings.SaveFileFormat)}";
				}

				return $"{Application.persistentDataPath}/{fileSaveSettings.SaveFileName}{StbUtilities.GetAssetTypeFileExtension(fileSaveSettings.SaveFileFormat)}";

			}

			if (!string.IsNullOrEmpty(fileSaveSettings.CustomSavePath))
			{
				return $"{fileSaveSettings.CustomSavePath}/Slot_{fileSaveSettings.SlotIndex.ToString()}/{fileSaveSettings.SaveFileName}{StbUtilities.GetAssetTypeFileExtension(fileSaveSettings.SaveFileFormat)}";
			}

			return $"{Application.persistentDataPath}/Slot_{fileSaveSettings.SlotIndex.ToString()}/{fileSaveSettings.SaveFileName}{StbUtilities.GetAssetTypeFileExtension(fileSaveSettings.SaveFileFormat)}";
		}

		public string GetSaveDefaultFileDirectory(FileSaveSettings fileSaveSettings)
		{
			if (fileSaveSettings.SlotIndex < 0)
			{
				if (!string.IsNullOrEmpty(fileSaveSettings.CustomSavePath))
				{
					return $"{fileSaveSettings.CustomSavePath}";
				}

				return $"{Application.persistentDataPath}";
			}

			if (!string.IsNullOrEmpty(fileSaveSettings.CustomSavePath))
			{
				return $"{fileSaveSettings.CustomSavePath}/Slot_{fileSaveSettings.SlotIndex.ToString()}";
			}

			return $"{Application.persistentDataPath}/Slot_{fileSaveSettings.SlotIndex.ToString()}";
		}

		/// <summary>
		/// Sets the CurrentSlotSaveData to a new save data.
		/// </summary>
		/// <param name="saveMetaData">Meta data to create the new save with.</param>
		private void NewSaveGame(SaveMetaData saveMetaData)
		{
			CurrentMetaData = saveMetaData;
			CurrentSaveData = null;
		}

		/// <summary>
		/// Sets the CurrentSlotSaveData to a new save data.
		/// </summary>
		/// <param name="slotIndex">The slot index for the new save.</param>
		private void NewSaveGame(int slotIndex = 0)
		{
			NewSaveGame(new SaveMetaData(slotIndex));
		}

		/// <summary>
		/// Returns the meta data for a save in a slot index.
		/// </summary>
		/// <param name="slotIndex">The slot index to get the meta data from.</param>
		/// <returns>The save meta data in the slot index.</returns>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		public SaveMetaData GetSaveMetaDataInSlot(int slotIndex, SaveSettings saveSettings = null)
		{
			Initialize();
			if (saveSettings == null) saveSettings = SingletonSaveSettings;
			var saveDataArgs = new FileSaveSettings(saveSettings, slotIndex);
			var loadedData = File.ReadAllBytes(GetSlotSaveDefaultFilePath(saveDataArgs));
			var encryptionSettings = saveSettings.StbEncryptionSettings;
			var decryptedData = StbSaveEncryption.Decrypt(loadedData, encryptionSettings.EncryptionType, encryptionSettings.EncryptionKeyword, encryptionSettings.EncryptionInitializationVector);
			var uncompressedData = StbSaveCompression.Decompress(decryptedData, saveSettings.CompressionType);
			switch (saveSettings.SaveFileFormat)
			{
				case StbFileFormat.Binary:
					return binarySerializer.FromBinary<SaveMetaData>(uncompressedData);
				case StbFileFormat.Json:
					var uncompressedString = Encoding.UTF8.GetString(uncompressedData);
					return jsonSerializer.FromJson<SaveMetaData>(uncompressedString);
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		public void SaveData(object saveObject, FileSaveSettings fileSaveSettings)
		{
			TryCreateSaveData(saveObject, fileSaveSettings);
		}

		/// <summary>
		/// Creates serialized save data. Also applies any encryption and compression.
		/// </summary>
		/// <param name="saveObject">The object which you would like to save.</param>
		/// <param name="fileSaveSettings">Defines the settings in which the save data will be created with.</param>
		/// <returns>Whether or not it successfully created the save data.</returns>
		public bool TryCreateSaveData(object saveObject, FileSaveSettings fileSaveSettings)
		{
			Initialize();
			var saveFilePath = GetSlotSaveDefaultFilePath(fileSaveSettings);
			var saveDirectory = GetSaveDefaultFileDirectory(fileSaveSettings);

			if (!Directory.Exists(saveDirectory))
			{
				Directory.CreateDirectory(saveDirectory);
			}

			// If it already exists, overwrite.
			var existingFilePaths = Directory.GetFiles(saveDirectory);
			foreach (var existingFilePath in existingFilePaths)
			{
				if (Path.GetFileNameWithoutExtension(existingFilePath) == fileSaveSettings.SaveFileName)
				{
					File.Delete(existingFilePath);
					if (LoggingEnabled) Debug.Log($"File at path: {saveDirectory}/{fileSaveSettings.SaveFileName} already Exists, overwriting.");
				}
			}

			switch (fileSaveSettings.SaveFileFormat)
			{
				// Save file works as type, then data.
				case StbFileFormat.Json:
				{
					var serializer = fileSaveSettings.CustomJsonSerializer ?? jsonSerializer;
					CurrentSerializationSettings = serializer.serializationSettings.Copy();
					var jsonSlotSaveData = serializer.ToJson(saveObject);
					var byteData = Encoding.UTF8.GetBytes(jsonSlotSaveData);
					var compressedData = StbSaveCompression.Compress(byteData, fileSaveSettings.CompressionType);
					var encryptedData = StbSaveEncryption.Encrypt(compressedData, fileSaveSettings.StbEncryptionSettings.EncryptionType, fileSaveSettings.StbEncryptionSettings.EncryptionKeyword, fileSaveSettings.StbEncryptionSettings.EncryptionInitializationVector);

					if (encryptedData == null)
					{
						if (LoggingEnabled) Debug.Log("Tried to save but the serialized data was null.");
						return false;
					}

					File.WriteAllBytes(saveFilePath, encryptedData);
					return true;
				}

				case StbFileFormat.Binary:
				{
					var selectedBinarySerializer = fileSaveSettings.CustomBinarySerializer ?? binarySerializer;
					CurrentSerializationSettings = selectedBinarySerializer.serializationSettings.Copy();
					var serializedData = selectedBinarySerializer.ToBinary(saveObject);
					var compressedData = StbSaveCompression.Compress(serializedData, fileSaveSettings.CompressionType);
					var encryptedData = StbSaveEncryption.Encrypt(compressedData, fileSaveSettings.StbEncryptionSettings.EncryptionType, fileSaveSettings.StbEncryptionSettings.EncryptionKeyword, fileSaveSettings.StbEncryptionSettings.EncryptionInitializationVector);

					if (encryptedData == null)
					{
						if (LoggingEnabled) Debug.Log("Tried to save but the serialized data was null.");
						return false;
					}

					File.WriteAllBytes(saveFilePath, encryptedData);
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Try to delete the data in a save slot.
		/// </summary>
		/// <param name="slotIndex">The index of the slot you want to try delete data from.</param>
		/// <param name="fileSaveSettings">The args to define which save slot should be deleted.</param>
		/// <returns>Whether or not it was successful in deleting data from the slot index.</returns>
		public bool TryDeleteSaveInSlot(int slotIndex = 1, FileSaveSettings fileSaveSettings = null)
		{
			Initialize();
			if (fileSaveSettings == null) fileSaveSettings = new FileSaveSettings(SingletonSaveSettings, slotIndex);

			var saveFilePath = GetSlotSaveDefaultFilePath(fileSaveSettings);
			if (!File.Exists(saveFilePath))
			{
				if (LoggingEnabled) Debug.LogError($"Could not delete save data in slot {slotIndex}, as it could not be found in path, {saveFilePath}");
				return false;
			}
			File.Delete(saveFilePath);
			if (LoggingEnabled) Debug.Log($"Deleted save in slot {slotIndex} successfully.");
			return true;
		}

		/// <summary>
		/// Load all meta datas from all save slots. Save slots are directories.
		/// </summary>
		/// <returns>An array of Meta Datas for all save slot datas. This can be used when showing simple data for loading slot datas.</returns>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		public T[] LoadAllSaveGameMetaDatas<T>(FileSaveSettings fileSaveSettings = null) where T : SaveMetaData
		{
			Initialize();
			if (fileSaveSettings == null) fileSaveSettings = new FileSaveSettings(SingletonSaveSettings);
			if (!fileSaveSettings.SaveFileName.Contains(META_DATA_NAME_SUFFIX))
			{
				fileSaveSettings.SaveFileName += META_DATA_NAME_SUFFIX;
			}

			return LoadAllSaveData<T>(fileSaveSettings);
		}

		/// <summary>
		/// Loads all data in every slot.
		/// </summary>
		/// <param name="fileSaveSettings">Save args to define what kind of data should be loaded and how. Defaults to
		/// the save settings if none is provided which will just load all save datas.</param>
		/// <typeparam name="T">The type of object to be loaded.</typeparam>
		/// <returns>An array of all loaded datas.</returns>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		public T[] LoadAllSaveData<T>(FileSaveSettings fileSaveSettings = null)
		{
			Initialize();
			var foundFileDirectoryPaths = new List<string>();
			if (fileSaveSettings == null) fileSaveSettings = new FileSaveSettings(SingletonSaveSettings);
			var path = GetSaveDefaultFileDirectory(fileSaveSettings);
			if (Directory.Exists($"{path}/"))
			{
				var extension = StbUtilities.GetAssetTypeFileExtension(fileSaveSettings.SaveFileFormat);
				var rootDirectoryFiles = Directory.GetFiles($"{path}/").Where(p => Path.GetFileName(p) == fileSaveSettings.SaveFileName + extension);
				if (rootDirectoryFiles.Any())
				{
					foundFileDirectoryPaths.Add($"{path}/");
				}

				var directories = Directory.GetDirectories($"{path}/").ToList();
				// Get all files from all directories.
				for (var index = 0; index < directories.Count; index++)
				{
					var directory = directories[index];
					var foundFiles = Directory.GetFiles($"{directory}/");
					foreach (var foundFile in foundFiles)
					{
						if (Path.GetFileName(foundFile) == fileSaveSettings.SaveFileName + extension)
						{
							foundFileDirectoryPaths.Add(directory);
						}
					}

					// Add any nested directories to look in there too.
					directories.AddRange(Directory.GetDirectories(directory));
				}
			}

			var loadedDatas = new List<T>();

			foreach (var foundFileDirectoryPath in foundFileDirectoryPaths)
			{
				fileSaveSettings.CustomSavePath = foundFileDirectoryPath;
				loadedDatas.Add(LoadData<T>(fileSaveSettings));
			}
			return loadedDatas.ToArray();
		}

		/// <summary>
		/// A SaveToolbox api to load all bytes at a file location. Will take into account encryption and compression but
		/// will not take into consideration custom serializers.
		/// </summary>
		/// <param name="fileSaveSettings">The file save settings to use. Will not take into consideration the custom serializers.</param>
		/// <returns>An array of bytes that was at the destination.</returns>
		public byte[] ReadAllBytes(FileSaveSettings fileSaveSettings)
		{
			Initialize();
			var loadedBytes = File.ReadAllBytes(GetSlotSaveDefaultFilePath(fileSaveSettings));
			var encryptionSettings = fileSaveSettings.StbEncryptionSettings;
			var decryptedData = StbSaveEncryption.Decrypt(loadedBytes, encryptionSettings.EncryptionType, encryptionSettings.EncryptionKeyword, encryptionSettings.EncryptionInitializationVector);
			var uncompressedData = StbSaveCompression.Decompress(decryptedData, fileSaveSettings.CompressionType);
			return uncompressedData;
		}

		/// <summary>
		/// A SaveToolbox api to load all text at a file location. Will take into account encryption and compression but
		/// will not take into consideration custom serializers.
		/// </summary>
		/// <param name="fileSaveSettings">The file save settings to use. Will not take into consideration the custom serializers.</param>
		/// <returns>A string containing text of the file that was at the destination.</returns>
		public string ReadAllText(FileSaveSettings fileSaveSettings)
		{
			Initialize();
			var loadedBytes = File.ReadAllBytes(GetSlotSaveDefaultFilePath(fileSaveSettings));
			var encryptionSettings = fileSaveSettings.StbEncryptionSettings;
			var decryptedData = StbSaveEncryption.Decrypt(loadedBytes, encryptionSettings.EncryptionType, encryptionSettings.EncryptionKeyword, encryptionSettings.EncryptionInitializationVector);
			var uncompressedData = StbSaveCompression.Decompress(decryptedData, fileSaveSettings.CompressionType);
			return Encoding.UTF8.GetString(uncompressedData);
		}

		/// <summary>
		/// A SaveToolbox api to write bytes at a file location. Will take into account encryption and compression but
		/// will not take into consideration custom serializers.
		/// </summary>
		/// <param name="bytes">The bytes to write to the file.</param>
		/// <param name="fileSaveSettings">The settings to use for writing, will not take into account custom serializers.</param>
		public void WriteAllBytes(byte[] bytes, FileSaveSettings fileSaveSettings)
		{
			Initialize();
			var saveFilePath = GetSlotSaveDefaultFilePath(fileSaveSettings);
			var saveDirectory = GetSaveDefaultFileDirectory(fileSaveSettings);

			if (!Directory.Exists(saveDirectory))
			{
				Directory.CreateDirectory(saveDirectory);
			}

			// If it already exists, overwrite.
			var existingFilePaths = Directory.GetFiles(saveDirectory);
			foreach (var existingFilePath in existingFilePaths)
			{
				if (Path.GetFileNameWithoutExtension(existingFilePath) == fileSaveSettings.SaveFileName)
				{
					File.Delete(existingFilePath);
					if (LoggingEnabled) Debug.Log($"File at path: {saveDirectory}/{fileSaveSettings.SaveFileName} already Exists, overwriting.");
				}
			}

			// Save file works as type, then data.
			var compressedData = StbSaveCompression.Compress(bytes, fileSaveSettings.CompressionType);
			var encryptedData = StbSaveEncryption.Encrypt(compressedData, fileSaveSettings.StbEncryptionSettings.EncryptionType, fileSaveSettings.StbEncryptionSettings.EncryptionKeyword, fileSaveSettings.StbEncryptionSettings.EncryptionInitializationVector);

			if (encryptedData == null || encryptedData.Length == 0)
			{
				Debug.LogError("Tried to save but the serialized data was null.");
				return;
			}

			File.WriteAllBytes(saveFilePath, encryptedData);
		}

		/// <summary>
		/// A SaveToolbox api to write text at a file location. Will take into account encryption and compression but
		/// will not take into consideration custom serializers.
		/// </summary>
		/// <param name="text">The bytes to write to the file.</param>
		/// <param name="fileSaveSettings">The settings to use for writing, will not take into account custom serializers.</param>
		public void WriteAllText(string text, FileSaveSettings fileSaveSettings)
		{
			Initialize();
			var saveFilePath = GetSlotSaveDefaultFilePath(fileSaveSettings);
			var saveDirectory = GetSaveDefaultFileDirectory(fileSaveSettings);

			if (!Directory.Exists(saveDirectory))
			{
				Directory.CreateDirectory(saveDirectory);
			}

			// If it already exists, overwrite.
			var existingFilePaths = Directory.GetFiles(saveDirectory);
			foreach (var existingFilePath in existingFilePaths)
			{
				if (Path.GetFileNameWithoutExtension(existingFilePath) == fileSaveSettings.SaveFileName)
				{
					File.Delete(existingFilePath);
					if (LoggingEnabled) Debug.Log($"File at path: {saveDirectory}/{fileSaveSettings.SaveFileName} already Exists, overwriting.");
				}
			}

			// Save file works as type, then data.
			var compressedData = StbSaveCompression.Compress(Encoding.UTF8.GetBytes(text), fileSaveSettings.CompressionType);
			var encryptedData = StbSaveEncryption.Encrypt(compressedData, fileSaveSettings.StbEncryptionSettings.EncryptionType, fileSaveSettings.StbEncryptionSettings.EncryptionKeyword, fileSaveSettings.StbEncryptionSettings.EncryptionInitializationVector);

			if (encryptedData == null || encryptedData.Length == 0)
			{
				Debug.LogError("Tried to save but the serialized data was null.");
				return;
			}

			File.WriteAllBytes(saveFilePath, encryptedData);
		}

		~SaveToolboxSystem()
		{
			writeAsyncTask?.Dispose();
			readAsyncTask?.Dispose();
		}

		/// <summary>
		///  A helper function to set the current loading state. Only useful during asynchronous loading.
		/// </summary>
		/// <param name="loadState">Current load state.</param>
		/// <param name="progression">Current progression through the current state 0 - 1.</param>
		private void SetLoadingState(LoadState loadState, float progression)
		{
			LoadingState.LoadState = loadState;
			LoadingState.CurrentStateProgression = progression;
			OnLoadingStateChanged?.Invoke(LoadingState);
		}

		/// <summary>
		///  A helper function to set the current saving state. Only useful during asynchronous saving.
		/// </summary>
		/// <param name="saveState">Current save state.</param>
		/// <param name="progression">Current progression through the current state 0 - 1.</param>
		private void SetSavingState(SaveState saveState, float progression)
		{
			SavingState.SaveState = saveState;
			SavingState.CurrentStateProgression = progression;
			OnSavingStateChanged?.Invoke(SavingState);
		}

		public static List<ISaveDataEntity> GetAllNonMonoBehaviourISaveDataEntities()
		{
			return nonMonoBehaviourSaveDataEntities.ToList();
		}

		/// <summary>
		/// Add an entity to the objects that will be saved list. This is for ISaveDataEntities that do not inherit
		/// from MonoBehaviours as they cannot be found by the FindObjectsOfType.
		/// </summary>
		/// <param name="saveDataEntity">The entity that will be added to the list to be saved.</param>
		public static void AddSaveGameDataEntity(ISaveDataEntity saveDataEntity)
		{
			if (nonMonoBehaviourSaveDataEntities.Any(nonMonoBehaviourSaveDataEntity => nonMonoBehaviourSaveDataEntity.SaveIdentifier == saveDataEntity.SaveIdentifier))
			{
				Debug.LogError("You are trying to add a saveable with an identifier that is already in the saveable list.");
				return;
			}

			if (saveDataEntity is MonoBehaviour _)
			{
				Debug.LogError("You are trying to add a saveable that is a MonoBehaviour. This will be picked up automatically in the save process. Use save collectors to achieve different collection of objects.");
				return;
			}

			nonMonoBehaviourSaveDataEntities.Add(saveDataEntity);
		}

		/// <summary>
		/// Add array of entities to the objects that will be saved list. This is for ISaveDataEntities that do not inherit
		/// from MonoBehaviours are they cannot be found by the FindObjectsOfType.
		/// </summary>
		/// <param name="saveDataEntities">The entities that will be added to the list to be saved.</param>
		public static void AddSaveGameDataEntities(ISaveDataEntity[] saveDataEntities)
		{
			foreach (var saveDataEntity in saveDataEntities)
			{
				AddSaveGameDataEntity(saveDataEntity);
			}
		}

		/// <summary>
		/// Remove a ISaveDataEntity from the list of objects that will be saved. This is for ISaveDataEntities that do not inherit
		/// from MonoBehaviours are they cannot be found by the FindObjectsOfType.
		/// </summary>
		/// <param name="saveDataEntity">The ISaveDataEntity that will be removed from the object to save list.</param>
		public static void RemoveSaveGameDataEntity(ISaveDataEntity saveDataEntity)
		{
			if (!nonMonoBehaviourSaveDataEntities.Contains(saveDataEntity))
			{
				Debug.LogError("You are trying to remove a saveable that is not in the saveable list.");
				return;
			}

			nonMonoBehaviourSaveDataEntities.Remove(saveDataEntity);
		}

		/// <summary>
		/// Remove a collection of ISaveDataEntities from the list of objects that will be saved. This is for ISaveDataEntities that do not inherit
		/// from MonoBehaviours are they cannot be found by the FindObjectsOfType.
		/// </summary>
		/// <param name="saveDataEntities">The ISaveDataEntity collection that will be removed from the object to save list.</param>
		public static void RemoveSaveGameDataEntities(IEnumerable<ISaveDataEntity> saveDataEntities)
		{
			foreach (var saveDataEntity in saveDataEntities)
			{
				RemoveSaveGameDataEntity(saveDataEntity);
			}
		}
	}
}