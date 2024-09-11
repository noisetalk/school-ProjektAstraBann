using System;
using SaveToolbox.Runtime.Utils;
using UnityEditor;
using UnityEngine;

namespace SaveToolbox.Runtime.Core.ScriptableObjects
{
	/// <summary>
	/// A singleton scriptable object that stores all the settings that define how game data should be saved.
	/// </summary>
	[CreateAssetMenu(fileName = "SaveToolboxPreferences", menuName = "SaveToolbox/SaveToolboxPreferences", order = 1)]
	public class SaveToolboxPreferences : StbSingletonScriptableObject<SaveToolboxPreferences>
	{
		private const string ASSET_PATH = "Assets/Resources";
		private const string ASYNCHRONOUS_SAVING_DEFINE = "STB_ASYNCHRONOUS_SAVING";

		/// <summary>
		/// The path at which the object should be saved by default.
		/// </summary>
		/// <exception cref="Exception">If it cannot find an asset path.</exception>
		public override string AssetPath
		{
			get
			{
				var scriptDirectoryParentName = ASSET_PATH;
				if (string.IsNullOrEmpty(scriptDirectoryParentName))
				{
					throw new Exception("Could not retrieve asset path.");
				}

				return scriptDirectoryParentName;
			}
		}

		[field: SerializeField]
		public SaveSettings DefaultSaveSettings { get; private set; }

		// Save settings getter properties.
		public string SaveFileName => DefaultSaveSettings.SaveFileName;
		public string RelativeFolderPath => DefaultSaveSettings.RelativeFolderPath;
		public StbFileFormat SaveFileFormat => DefaultSaveSettings.SaveFileFormat;
		public bool JsonPrettyPrint => DefaultSaveSettings.JsonPrettyPrint;
		public StbSerializationSettings SerializationSettings => DefaultSaveSettings.SerializationSettings;
		public StbEncryptionSettings StbEncryptionSettings => DefaultSaveSettings.StbEncryptionSettings;
		public StbCompressionType CompressionType => DefaultSaveSettings.CompressionType;
		public bool RebuildLoadableObjects => DefaultSaveSettings.RebuildLoadableObjects;
		public bool PhysicsSyncTransformsOnLoad => DefaultSaveSettings.PhysicsSyncTransformsOnLoad;
		public bool SaveScene => DefaultSaveSettings.SaveScene;
		public StbSceneSavingType StbSceneSavingType => DefaultSaveSettings.StbSceneSavingType;
		public bool FreezeTimeScaleOnSaveLoad => DefaultSaveSettings.FreezeTimeScaleOnAsynchronousSaveLoad;

		/// <summary>
		/// Should the data be saved asynchronously?
		/// </summary>
		[field: SerializeField]
		private bool asynchronousSaving;
		public bool AsynchronousSaving {
			get => asynchronousSaving;
			set
			{
				asynchronousSaving = value;
				UpdateScriptingDefines();
			}
		}

		/// <summary>
		/// If the data is saved asynchrnously, what is the lowest acceptable frame rate.
		/// </summary>
		[field: SerializeField]
		public int LowestAcceptableLoadingFrameRate { get; set; } = 30;

		/// <summary>
		/// When processes are completed or failed through the save system their are logs, would you like these to be enabled?
		/// </summary>
		[field: SerializeField]
		public bool LoggingEnabled { get; set; } = true;

		private bool previousAsynchronousSaving;

		private void Awake()
		{
			previousAsynchronousSaving = asynchronousSaving;
		}

		private void UpdateScriptingDefines()
		{
#if UNITY_EDITOR
			var scriptingDefines = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
			if (asynchronousSaving)
			{
				if (!scriptingDefines.Contains($";{ASYNCHRONOUS_SAVING_DEFINE}") && !scriptingDefines.Contains($"{ASYNCHRONOUS_SAVING_DEFINE}"))
				{
					scriptingDefines += $";{ASYNCHRONOUS_SAVING_DEFINE}";
				}
			}
			else
			{
				if (scriptingDefines.Contains($";{ASYNCHRONOUS_SAVING_DEFINE}"))
				{
					scriptingDefines = scriptingDefines.Replace($";{ASYNCHRONOUS_SAVING_DEFINE}", "");
				}

				if (scriptingDefines.Contains($"{ASYNCHRONOUS_SAVING_DEFINE}"))
				{
					scriptingDefines = scriptingDefines.Replace($"{ASYNCHRONOUS_SAVING_DEFINE}", "");
				}
			}

			previousAsynchronousSaving = asynchronousSaving;
			PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, scriptingDefines);
#endif
		}

		private void OnValidate()
		{
			if (previousAsynchronousSaving != asynchronousSaving)
			{
				UpdateScriptingDefines();
			}
		}
	}
}