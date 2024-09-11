using System;
using System.Collections.Generic;
using System.Linq;
using SaveToolbox.Runtime.Attributes;
using SaveToolbox.Runtime.Core.ScriptableObjects;
using SaveToolbox.Runtime.Interfaces;
using SaveToolbox.Runtime.Utils;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SaveToolbox.Runtime.Core.MonoBehaviours
{
	/// <summary>
	/// A MonoBehaviour component to allow for spawning in a scene from an ID that is saved on this object.
	/// The ID is dictated by the database.
	/// </summary>

	[ExecuteAlways, DisallowMultipleComponent, AddComponentMenu("SaveToolbox/Core/LoadableObject")]
	public class LoadableObject : MonoBehaviour
	{
		private const string PASTED_STACK_TRACE_VALUE = "UnityEditor.SceneHierarchy.PasteGO";
		private const string DUPLICATED_STACK_TRACE_VALUE = "UnityEditor.SceneHierarchy.DuplicateGO";
		private const string DRAGGED_FROM_ASSETS_VALUE = "UnityEditorInternal.InternalEditorUtility.HierarchyWindowDragByID";
		private const string INSTANTIATED_STACK_TRACE_VALUE = "UnityEngine.Object.Instantiate";
		private const string INSTANTIATED_PREFAB_STACK_TRACE_VALUE = "UnityEditor.PrefabUtility.InstantiatedPrefab";

		[field: SerializeField, StbSerialize, ReadOnly]
		public string Identifier { get; private set; } = Guid.NewGuid().ToString();

		[field: SerializeField, StbSerialize, ReadOnly]
		public int LoadableObjectId { get; set; } = -1;

		[field: SerializeField, StbSerialize, ReadOnly]
		public bool WasInstantiatedAtRuntime { get; private set; }

		[field: SerializeField, StbSerialize, ReadOnly]
		public bool IsInitialized { get; private set; }

		[SerializeField, StbSerialize, Tooltip("Saves any added ISaveDataEntity monobehaviours.")]
		private bool saveAddedComponents = true;
		public bool SaveAddedComponents => saveAddedComponents;


#if STB_ABOVE_2021_3 // Non reorderable not available on older editors.
		[SerializeField, StbSerialize, ReadOnly, NonReorderable]
#else
		[SerializeField, StbSerialize, ReadOnly]
#endif
		public List<MonoBehaviour> saveDataEntityBehaviours = new List<MonoBehaviour>();

		/// <summary>
		/// This should be set to false through scripts if you do not want to spawn the object when you save and load.
		/// </summary>
		public bool ShouldSpawnWhenLoaded { get; set; } = true;

		private void Awake()
		{
			//If it wasn't initialized, initialize it and regenerateIds.
			if (!IsInitialized)
			{
				UpdateReferencedBehaviours();
				WasInstantiatedAtRuntime = Application.isPlaying;
				ShouldSpawnWhenLoaded = WasInstantiatedAtRuntime;
				IsInitialized = true;

				RegenerateIdentifiers();
			}
			else
			{
				var environmentStackTrace = Environment.StackTrace;
				// Check the stack trace to figure out how the loadable object was created.
				if (environmentStackTrace.Contains(PASTED_STACK_TRACE_VALUE) ||
				    environmentStackTrace.Contains(DUPLICATED_STACK_TRACE_VALUE) ||
				    environmentStackTrace.Contains(INSTANTIATED_STACK_TRACE_VALUE) ||
				    environmentStackTrace.Contains(INSTANTIATED_PREFAB_STACK_TRACE_VALUE) ||
				    environmentStackTrace.Contains(DRAGGED_FROM_ASSETS_VALUE))
				{
					RegenerateIdentifiers();
				}
			}

			if (Application.isPlaying)
			{
				foreach (var saveDataEntityBehaviour in saveDataEntityBehaviours)
				{
					if (saveDataEntityBehaviour is ILoadableObjectTarget loadableObjectTarget)
					{
						loadableObjectTarget.LoadableObjectId = Identifier;
					}
				}
			}
		}

		private void OnEnable()
		{
#if UNITY_EDITOR
			UnityEditor.SceneManagement.EditorSceneManager.sceneSaving += HandleSceneSaved;
#if STB_ABOVE_2021_3
			UnityEditor.SceneManagement.PrefabStage.prefabSaved += HandlePrefabSaved;
#else
			AssetsSavedListener.OnAssetsSaved += HandleAssetsSaved;
#endif
#endif
			UpdateReferencedBehaviours();
		}

		private void HandleAssetsSaved()
		{
			UpdateReferencedBehaviours();
		}

		private void HandlePrefabSaved(GameObject _)
		{
			UpdateReferencedBehaviours();
		}

		private void HandleSceneSaved(Scene _, string _2)
		{
			UpdateReferencedBehaviours();
		}

		/// <summary>
		/// This is used to catch any duplication since it is on ExecuteAlways. If We are duplicated, we want to regenerate new IDs for each ISaveDataEntity.
		/// </summary>
		private void Reset()
		{
			UpdateReferencedBehaviours();
			foreach (var saveDataEntityBehaviour in saveDataEntityBehaviours)
			{
				if (saveDataEntityBehaviour is ISaveDataEntity saveDataEntity)
				{
#if STB_ABOVE_2021_3
					saveDataEntity.GenerateNewIdentifier();
#else
					saveDataEntity.SaveIdentifier = Guid.NewGuid().ToString();
#endif
				}
			}
		}

		private void OnValidate()
		{
			UpdateReferencedBehaviours();
		}

		/// <summary>
		/// Updates which Behaviours are referenced by this LoadableObject. Clears all current references and
		/// assigns all new ones to the stored list.
		/// </summary>
		[ContextMenu("Update Referenced Behaviours")]
		public void UpdateReferencedBehaviours()
		{
			// Remove any old null behaviours.
			for (var index = saveDataEntityBehaviours.Count - 1; index >= 0; index--)
			{
				if (saveDataEntityBehaviours[index] == null)
				{
					saveDataEntityBehaviours.RemoveAt(index);
				}
			}

			// Add any new behaviours.
			var saveDataEntities = GetComponentsInChildren<ISaveDataEntity>(true);
			foreach (var saveDataEntity in saveDataEntities)
			{
				if (saveDataEntity is MonoBehaviour monoBehaviour)
				{
					if (saveDataEntityBehaviours.Contains(monoBehaviour)) continue;

					saveDataEntityBehaviours.Add(monoBehaviour);
				}
			}
		}

		/// <summary>
		/// Regenerates the identifier of this LoadableObject.
		/// </summary>
		[ContextMenu("Regenerate Own SaveIdentifier")]
		public void RegenerateOwnIdentifier()
		{
			Identifier = Guid.NewGuid().ToString();
		}

		/// <summary>
		/// Regenerates the identifiers of self and also any referenced ISaveDataEntity's in the SaveDataEntityBehaviours List.
		/// </summary>
		[ContextMenu("Regenerate All Behaviour Identifiers")]
		public void RegenerateIdentifiers()
		{
			RegenerateOwnIdentifier();
			foreach (var saveDataEntityBehaviour in saveDataEntityBehaviours)
			{
				var saveDataEntity = saveDataEntityBehaviour as ISaveDataEntity;
				if (saveDataEntity == null) continue;

				saveDataEntity.SaveIdentifier = Guid.NewGuid().ToString();
			}
		}

		/// <summary>
		/// Serialize the loadable object into a LoadableObjectSaveData. This is a serializable class and can be saved
		/// using the save systems.
		/// </summary>
		/// <returns></returns>
		public LoadableObjectSaveData Serialize(SaveSettings saveSettings)
		{
			var saveDataEntityObjects = new List<LoadableObjectEntitySaveData>();
			for (var i = 0; i < saveDataEntityBehaviours.Count; i++)
			{
				var saveDataEntity = saveDataEntityBehaviours[i] as ISaveDataEntity;
				if (saveDataEntityBehaviours[i] == null)
				{
					if (SaveToolboxPreferences.Instance.LoggingEnabled) Debug.LogWarning("Tried to serialize a non-ISaveDataEntity. This should not be in the loadable objects saveDataEntityBehaviours.");
					continue;
				}

				var saveDataEntityObject = new LoadableObjectEntitySaveData(saveDataEntity.SaveIdentifier, saveDataEntity.GetType().AssemblyQualifiedName, i);
				saveDataEntityObjects.Add(saveDataEntityObject);
			}

			saveDataEntityObjects = new List<LoadableObjectEntitySaveData>(saveDataEntityObjects.OrderByDescending(saveDataEntity => saveDataEntity.LoadableObjectEntityIndex));

			// We save the scene path even if they didn't select it, so at least the data is there if they change their mind.
			string scenePath;
			switch (saveSettings.StbSceneSavingType)
			{
				case StbSceneSavingType.BuildIndex:
					scenePath = gameObject.scene.buildIndex.ToString();
					break;
				case StbSceneSavingType.Path:
					scenePath = gameObject.scene.path;
					break;
				default:
					scenePath = string.Empty;
					break;
			}

			var serializedData = new LoadableObjectSaveData(name, LoadableObjectId, Identifier, saveDataEntityObjects, scenePath);
			return serializedData;
		}

		/// <summary>
		/// Deserialize the loadable object save data and applies it to the loadable object.
		/// </summary>
		/// <param name="loadableObjectSaveData">The loadable object save data.</param>
		public void Deserialize(LoadableObjectSaveData loadableObjectSaveData)
		{
			Identifier = loadableObjectSaveData.Identifier;
			name = loadableObjectSaveData.GameObjectName;

			// Loop through all the saved monobehaviours that loadableobject referenced.
			foreach (var saveEntityObjectData in loadableObjectSaveData.SaveDataObjects)
			{
				// If it should save added components load them too.
				if (saveDataEntityBehaviours.Count <= saveEntityObjectData.LoadableObjectEntityIndex && saveAddedComponents)
				{
					var saveDataEntityType = Type.GetType(saveEntityObjectData.TypeAssemblyName);
					gameObject.AddComponent(saveDataEntityType);
					UpdateReferencedBehaviours();
				}

				// If it's not a save data entity throw an error and continue.
				var saveDataEntity = saveDataEntityBehaviours[saveEntityObjectData.LoadableObjectEntityIndex] as ISaveDataEntity;
				if (saveDataEntity == null)
				{
					if (SaveToolboxPreferences.Instance.LoggingEnabled) Debug.LogWarning("Tried to deserialize a non-ISaveDataEntity. This should not be in the loadable objects saveDataEntityBehaviours.");
					continue;
				}

				// Update the save data entity identifier so it is the same when we load it just for consistencies sake.
				saveDataEntity.SaveIdentifier = saveEntityObjectData.Identifier;

				if (saveDataEntityBehaviours[saveEntityObjectData.LoadableObjectEntityIndex] is ILoadableObjectTarget loadableObjectTarget)
				{
					loadableObjectTarget.LoadableObjectId = Identifier;
				}
			}
		}

		/// <summary>
		/// A function to determine whether or not the LoadableObject references a behaviour of ISaveEntity with
		/// a specific identifier.
		/// </summary>
		/// <param name="identifier">The ISaveDataEntity identifier of the object you are checking for.</param>
		/// <returns>If it contains a behaviour with this identifier.</returns>
		public bool ContainsBehaviour(string identifier)
		{
			foreach (var saveDataEntityBehaviour in saveDataEntityBehaviours)
			{
				var saveDataEntity = saveDataEntityBehaviour as ISaveDataEntity;
				if (saveDataEntity == null) continue;

				if (saveDataEntity.SaveIdentifier == identifier) return true;
			}
			return false;
		}

		private void OnDisable()
		{
#if UNITY_EDITOR
			UnityEditor.SceneManagement.EditorSceneManager.sceneSaving -= HandleSceneSaved;
#if STB_ABOVE_2021_3
			UnityEditor.SceneManagement.PrefabStage.prefabSaved -= HandlePrefabSaved;
#else
			AssetsSavedListener.OnAssetsSaved -= HandleAssetsSaved;
#endif
#endif
		}
	}

	[Serializable]
	public class LoadableObjectEntitySaveData
	{
		[SerializeField, StbSerialize]
		private string identifier;
		public string Identifier => identifier;

		[SerializeField, StbSerialize]
		private string typeAssemblyName = string.Empty;
		public string TypeAssemblyName => typeAssemblyName;

		[SerializeField, StbSerialize]
		private int loadableObjectEntityIndex = -1;
		public int LoadableObjectEntityIndex => loadableObjectEntityIndex;

		public LoadableObjectEntitySaveData(string identifier, string typeAssemblyName, int loadableObjectEntityIndex)
		{
			this.identifier = identifier;
			this.typeAssemblyName = typeAssemblyName;
			this.loadableObjectEntityIndex = loadableObjectEntityIndex;
		}

		public LoadableObjectEntitySaveData() {}
	}

	[Serializable]
	public class LoadableObjectSaveData
	{
		[SerializeField, StbSerialize]
		private string gameObjectName;
		public string GameObjectName => gameObjectName;

		[SerializeField, StbSerialize]
		private string identifier;
		public string Identifier => identifier;

		[SerializeField, StbSerialize]
		private int loadableObjectId;
		public int LoadableObjectId => loadableObjectId;

		[SerializeField, StbSerialize]
		private string scenePath;
		public string ScenePath => scenePath;

		[SerializeField, StbSerialize]
		private List<LoadableObjectEntitySaveData> saveDataObjects = new List<LoadableObjectEntitySaveData>();
		public List<LoadableObjectEntitySaveData> SaveDataObjects => saveDataObjects;

		public LoadableObjectSaveData(string gameObjectName, int loadableObjectId, string identifier, List<LoadableObjectEntitySaveData> saveDataObjects, string scenePath = "")
		{
			this.gameObjectName = gameObjectName;
			this.loadableObjectId = loadableObjectId;
			this.identifier = identifier;
			this.saveDataObjects = saveDataObjects;
			this.scenePath = scenePath;
		}

		public LoadableObjectSaveData()
		{
			gameObjectName = string.Empty;
			identifier = string.Empty;
			loadableObjectId = -1;
			scenePath = string.Empty;
			saveDataObjects = new List<LoadableObjectEntitySaveData>();
		}
	}
}
