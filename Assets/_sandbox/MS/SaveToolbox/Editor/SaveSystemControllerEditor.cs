using SaveToolbox.Runtime.Core;
using SaveToolbox.Runtime.Core.MonoBehaviours;
using UnityEditor;
using UnityEngine;

namespace SaveToolbox.Editor
{
	[CustomEditor(typeof(SaveSystemController))]
	public class SaveSystemControllerEditor : UnityEditor.Editor
	{
		private int loadDataSlotIndex;
		private int saveDataSlotIndex;
		private int deleteSaveDataSlotIndex;

		private GUIStyle preferencesBoxStyle;

		private GUIStyle largeTextStyle;
		private GUIStyle mediumTextStyle;

		private GUIContent saveFileNameContent;
		private GUIContent relativeFolderPathContent;
		private GUIContent saveFileFormatContent;
		private GUIContent jsonPrettyPrintContent;
		private GUIContent targetFieldContent;
		private GUIContent targetPrivateFieldContent;
		private GUIContent targetPublicFieldContent;
		private GUIContent targetSerializeFieldFieldContent;
		private GUIContent targetStbSerializeFieldContent;
		private GUIContent targetPropertyContent;
		private GUIContent targetPrivatePropertyContent;
		private GUIContent targetPublicPropertyContent;
		private GUIContent targetSerializeFieldPropertyContent;
		private GUIContent targetStbSerializePropertyContent;
		private GUIContent encryptionTypeContent;
		private GUIContent encryptionKeywordContent;
		private GUIContent compressionTypeContent;
		private GUIContent initializationVectorContent;
		private GUIContent shouldSaveLoadScenesContent;
		private GUIContent sceneSaveLoadSolutionContent;
		private GUIContent rebuildLoadableObjectsContent;
		private GUIContent syncTransformsContent;
		private GUIContent asynchronousSavingContent;
		private GUIContent lowestFrameRateContent;
		private GUIContent loggingEnabledContent;
		private GUIContent freezeTimescaleContent;

				private void TryCreateStyles()
		{
			if (preferencesBoxStyle == null)
			{
				preferencesBoxStyle = new GUIStyle(GUI.skin.box)
				{
					padding = new RectOffset(15, 15, 15, 15),
					margin = new RectOffset(15, 15, 15, 15)
				};
			}

			if (largeTextStyle == null)
			{
				largeTextStyle = new GUIStyle(GUI.skin.label)
				{
					fontSize = 16,
					fontStyle = FontStyle.Bold,
					padding = new RectOffset(-5, 5, 0, 10)
				};
			}

			if (mediumTextStyle == null)
			{
				mediumTextStyle = new GUIStyle(GUI.skin.label)
				{
					fontSize = 12,
					fontStyle = FontStyle.Bold,
					padding = new RectOffset(-5, 0, 10, 5)
				};
			}

			if (saveFileNameContent == null)
			{
				saveFileNameContent = new GUIContent("Save File Name:", "The name that the file will save with.");
			}
			if (relativeFolderPathContent == null)
			{
				relativeFolderPathContent = new GUIContent("Relative Folder Path:", "The folder path of the save file, this originates from the Application.PersistantDataPath.");
			}
			if (saveFileFormatContent == null)
			{
				saveFileFormatContent = new GUIContent("Save File Format:", "The file type the save data will be saved as.");
			}
			if (jsonPrettyPrintContent == null)
			{
				jsonPrettyPrintContent = new GUIContent("Json Pretty Print:", "Prints the json to file in a more readable format with new lines and whitespaces.");
			}
			if (targetFieldContent == null)
			{
				targetFieldContent = new GUIContent("Target Fields:", "Should custom object serialization to target fields.");
			}
			if (targetPrivateFieldContent == null)
			{
				targetPrivateFieldContent = new GUIContent("Target All Private Fields:", "Should serialization target all private fields.");
			}
			if (targetPublicFieldContent == null)
			{
				targetPublicFieldContent = new GUIContent("Target All Public Fields:", "Should serialization target all public fields.");
			}
			if (targetSerializeFieldFieldContent == null)
			{
				targetSerializeFieldFieldContent = new GUIContent("Target Fields With [SerializeField] Attribute:", "Should serialization target all fields tagged with the [SerializeField] attribute.");
			}
			if (targetStbSerializeFieldContent == null)
			{
				targetStbSerializeFieldContent = new GUIContent("Target Fields With [StbSerialize] Attribute:", "Should serialization target all fields tagged with the [StbSerialize] attribute. This is a custom SaveToolbox attribute.");
			}
			if (targetPropertyContent == null)
			{
				targetPropertyContent = new GUIContent("Target Properties:", "Should custom object serialization to target properties.");
			}
			if (targetPrivatePropertyContent == null)
			{
				targetPrivatePropertyContent = new GUIContent("Target All Private Properties:", "Should serialization target all private properties.");
			}
			if (targetPublicPropertyContent == null)
			{
				targetPublicPropertyContent = new GUIContent("Target All Public Properties:", "Should serialization target all public properties.");
			}
			if (targetSerializeFieldPropertyContent == null)
			{
				targetSerializeFieldPropertyContent = new GUIContent("Target Properties With [SerializeField] Attribute:", "Should serialization target all properties tagged with the [SerializeField] attribute.");
			}
			if (targetStbSerializePropertyContent == null)
			{
				targetStbSerializePropertyContent = new GUIContent("Target Properties With [StbSerialize] Attribute:", "Should serialization target all properties tagged with the [StbSerialize] attribute. This is a custom SaveToolbox attribute.");
			}
			if (encryptionTypeContent == null)
			{
				encryptionTypeContent = new GUIContent("Encryption Type:", "The type of encryption that will be used. Xor is a simpler form of encryption that uses a key. Aes is recommended, it also uses a key as well as an initialization vector for additional security (save toolbox can generate this for you)");
			}
			if (encryptionKeywordContent == null)
			{
				encryptionKeywordContent = new GUIContent("Encryption Keyword:", "The keyword that will be used for the encrypting of the save data. It is used to decrypt and encrypt the data, make sure to keep it secret.");
			}
			if (initializationVectorContent == null)
			{
				initializationVectorContent = new GUIContent("Initialization Vector:", "Another layer of security for Aes encryption. Save Toolbox generates this for you, you can regenerate it by pressing the regenerate button below.");
			}
			if (compressionTypeContent == null)
			{
				compressionTypeContent = new GUIContent("Compression Type:", "What type of compression should be used? Currently only GZip is supported.");
			}
			if (shouldSaveLoadScenesContent == null)
			{
				shouldSaveLoadScenesContent = new GUIContent("Should Save/Load Scenes:", "Should SaveToolbox handle the saving and loading of current scenes? If selected when a file is loaded it will find all necessary scenes and load them.");
			}
			if (sceneSaveLoadSolutionContent == null)
			{
				sceneSaveLoadSolutionContent = new GUIContent("Save/Load Scene Solution:", "If we are saving the scenes, we need a way to store reference to the scene. Build index means we find the scene by build index, changing this in build settings after a save has been create can lead to losing reference to the scene. Path means we find the scene by the asset path, moving the scene asset can lead to the toolbox not being able to find the scene.");
			}
			if (rebuildLoadableObjectsContent == null)
			{
				rebuildLoadableObjectsContent = new GUIContent("Destroy & Rebuild Loadable Objects:", "When we load loadable objects into a scene and they already exist in the scene from a prior game state, do we want to destroy them and rebuild them from scratch?. Recommended for to ensure no discrepancies exist between save states.");
			}
			if (syncTransformsContent == null)
			{
				syncTransformsContent = new GUIContent("Physics Sync Transform On Load:", "Do we want to perform a physics sync transform on load, this ensures all newly loaded loadable objects are in the correct position on the physics update.");
			}
			if (asynchronousSavingContent == null)
			{
				asynchronousSavingContent = new GUIContent("Use Asynchronous Saving:", "Does saving and loading want to be done over multiple frames to avoid large lag spikes.");
			}
			if (lowestFrameRateContent == null)
			{
				lowestFrameRateContent = new GUIContent("Target Asynchronous Frame Rate:", "If asynchronous saving is happening, what is the lowest acceptable frame rate.");
			}
			if (loggingEnabledContent == null)
			{
				loggingEnabledContent = new GUIContent("Logging Enabled?:", "Save Toolbox has built in logs for when save or load is executed etc, if this is enabled is will display those logs in the console.");
			}
			if (freezeTimescaleContent == null)
			{
				freezeTimescaleContent = new GUIContent("Freeze Timescale On Asynchronous Save/Load:", "Should the timescale be set to 0 when asynchronously saving/loading game states. true is recommended.");
			}
		}

		public override void OnInspectorGUI()
		{
			TryCreateStyles();
			var saveSystemController = (SaveSystemController)target;

			saveSystemController.HasCustomSaveSettings = GUILayout.Toggle(saveSystemController.HasCustomSaveSettings, "Custom Serialization Settings?");
			var customSaveSettings = saveSystemController.CustomSaveSettings;

			if (saveSystemController.HasCustomSaveSettings)
			{
				EditorGUI.BeginChangeCheck();

				EditorGUILayout.BeginVertical(preferencesBoxStyle);

				GUILayout.Label("File Settings", mediumTextStyle);

				// Save file name.
				customSaveSettings.SaveFileName = EditorGUILayout.TextField(saveFileNameContent, customSaveSettings.SaveFileName);

				// Relative folder path to Application.PersistentDataPath.
				customSaveSettings.RelativeFolderPath = EditorGUILayout.TextField(relativeFolderPathContent, customSaveSettings.RelativeFolderPath);

				// Save File Type.
				customSaveSettings.SaveFileFormat = (StbFileFormat)EditorGUILayout.EnumPopup(saveFileFormatContent, customSaveSettings.SaveFileFormat);

				// Json Pretty Print.
				if (customSaveSettings.SaveFileFormat == StbFileFormat.Json)
				{
					customSaveSettings.JsonPrettyPrint = EditorGUILayout.Toggle(jsonPrettyPrintContent, customSaveSettings.JsonPrettyPrint);
				}

				EditorGUILayout.Space();

				GUILayout.Label("Serialization Settings", mediumTextStyle);

				var fieldSettings = customSaveSettings.SerializationSettings.FieldSerializationSettings;
				fieldSettings.ValidTarget = EditorGUILayout.Toggle(targetFieldContent, fieldSettings.ValidTarget);
				if (fieldSettings.ValidTarget)
				{
					fieldSettings.TargetAllPrivate = EditorGUILayout.Toggle(targetPrivateFieldContent, fieldSettings.TargetAllPrivate);
					fieldSettings.TargetAllPublic = EditorGUILayout.Toggle(targetPublicFieldContent, fieldSettings.TargetAllPublic);
					fieldSettings.TargetSerializeFieldAttribute = EditorGUILayout.Toggle(targetSerializeFieldFieldContent, fieldSettings.TargetSerializeFieldAttribute);
					fieldSettings.TargetStbSerializeAttribute = EditorGUILayout.Toggle(targetStbSerializeFieldContent, fieldSettings.TargetStbSerializeAttribute);
				}

				EditorGUILayout.Space();

				var propertySettings = customSaveSettings.SerializationSettings.PropertySerializationSettings;
				propertySettings.ValidTarget = EditorGUILayout.Toggle(targetPropertyContent, propertySettings.ValidTarget);
				if (propertySettings.ValidTarget)
				{
					EditorGUILayout.LabelField("Please Note: The property access modifier (public/Private) is dictated by the access modifier of the get method on the property.");
					propertySettings.TargetAllPrivate = EditorGUILayout.Toggle(targetPrivatePropertyContent, propertySettings.TargetAllPrivate);
					propertySettings.TargetAllPublic = EditorGUILayout.Toggle(targetPublicPropertyContent, propertySettings.TargetAllPublic);
					propertySettings.TargetSerializeFieldAttribute = EditorGUILayout.Toggle(targetSerializeFieldPropertyContent, propertySettings.TargetSerializeFieldAttribute);
					propertySettings.TargetStbSerializeAttribute = EditorGUILayout.Toggle(targetStbSerializePropertyContent, propertySettings.TargetStbSerializeAttribute);
				}

				EditorGUILayout.Space();

				// Encryption SettingsScriptableObject.
				GUILayout.Label("Encryption Settings", mediumTextStyle);

				// Encryption Type.
				customSaveSettings.StbEncryptionSettings.EncryptionType = (StbEncryptionType)EditorGUILayout.EnumPopup(encryptionTypeContent, customSaveSettings.StbEncryptionSettings.EncryptionType);
				if (customSaveSettings.StbEncryptionSettings.EncryptionType != StbEncryptionType.None)
				{
					// Encryption Keyword.
					customSaveSettings.StbEncryptionSettings.EncryptionKeyword = EditorGUILayout.TextField(encryptionKeywordContent, customSaveSettings.StbEncryptionSettings.EncryptionKeyword);
				}

				// Encryption Type Aes.
				if (customSaveSettings.StbEncryptionSettings.EncryptionType == StbEncryptionType.Aes)
				{
					EditorGUI.BeginDisabledGroup(true);
					// Encryption Initialization Vector for Aes.
					customSaveSettings.StbEncryptionSettings.EncryptionInitializationVector = EditorGUILayout.TextField(initializationVectorContent, customSaveSettings.StbEncryptionSettings.EncryptionInitializationVector);
					EditorGUI.EndDisabledGroup();
					if (GUILayout.Button("Regenerate Initialization Vector"))
					{
						customSaveSettings.RegenerateInitializationVector();
					}
				}

				EditorGUILayout.Space();

				// Compression SettingsScriptableObject.
				GUILayout.Label("Compression Settings", mediumTextStyle);

				// Compression Type.
				customSaveSettings.CompressionType = (StbCompressionType)EditorGUILayout.EnumPopup(compressionTypeContent, customSaveSettings.CompressionType);

				EditorGUILayout.Space();

				GUILayout.Label("Object Loading Settings", mediumTextStyle);

				// Should Save/Load Scenes.
				customSaveSettings.SaveScene = EditorGUILayout.Toggle(shouldSaveLoadScenesContent, customSaveSettings.SaveScene);

				// Scene Load Type
				if (customSaveSettings.SaveScene)
				{
					customSaveSettings.StbSceneSavingType = (StbSceneSavingType)EditorGUILayout.EnumPopup(sceneSaveLoadSolutionContent, customSaveSettings.StbSceneSavingType);
				}

				// Rebuild loadable objects.
				customSaveSettings.RebuildLoadableObjects = EditorGUILayout.Toggle(rebuildLoadableObjectsContent, customSaveSettings.RebuildLoadableObjects);

				// Physics sync transform on load.
				customSaveSettings.PhysicsSyncTransformsOnLoad = EditorGUILayout.Toggle(syncTransformsContent, customSaveSettings.PhysicsSyncTransformsOnLoad);

				// Freeze time scale when asynchronous saving/loading
				customSaveSettings.FreezeTimeScaleOnAsynchronousSaveLoad = EditorGUILayout.Toggle(freezeTimescaleContent, customSaveSettings.FreezeTimeScaleOnAsynchronousSaveLoad);

				EditorGUILayout.EndVertical();
				EditorGUILayout.BeginHorizontal();
				saveDataSlotIndex = EditorGUILayout.IntField(saveDataSlotIndex);
				if (GUILayout.Button("Try Save"))
				{
					saveSystemController.SaveGame(customSaveSettings, saveDataSlotIndex);
				}

				EditorGUILayout.EndHorizontal();

				EditorGUILayout.LabelField("Load data in slot.");

				EditorGUILayout.BeginHorizontal();
				loadDataSlotIndex = EditorGUILayout.IntField(loadDataSlotIndex);
				if (GUILayout.Button("Try Load"))
				{
					saveSystemController.Load(customSaveSettings, loadDataSlotIndex);
				}

				EditorGUILayout.EndHorizontal();

				EditorGUILayout.LabelField("Delete data in slot.");

				EditorGUILayout.BeginHorizontal();
				deleteSaveDataSlotIndex = EditorGUILayout.IntField(deleteSaveDataSlotIndex);
				if (GUILayout.Button("Try Delete"))
				{
					saveSystemController.TryDelete(deleteSaveDataSlotIndex);
				}

				EditorGUILayout.EndHorizontal();

				var hasChanged = EditorGUI.EndChangeCheck();

				if (hasChanged)
				{
					serializedObject.ApplyModifiedProperties();
					EditorUtility.SetDirty(saveSystemController);
				}
			}
			else
			{
				EditorGUILayout.BeginHorizontal();
				saveDataSlotIndex = EditorGUILayout.IntField(saveDataSlotIndex);
				if (GUILayout.Button("Try Save"))
				{
					saveSystemController.SaveGame(saveDataSlotIndex);
				}

				EditorGUILayout.EndHorizontal();

				EditorGUILayout.LabelField("Load data in slot.");

				EditorGUILayout.BeginHorizontal();
				loadDataSlotIndex = EditorGUILayout.IntField(loadDataSlotIndex);
				if (GUILayout.Button("Try Load"))
				{
					saveSystemController.Load(loadDataSlotIndex);
				}

				EditorGUILayout.EndHorizontal();

				EditorGUILayout.LabelField("Delete data in slot.");

				EditorGUILayout.BeginHorizontal();
				deleteSaveDataSlotIndex = EditorGUILayout.IntField(deleteSaveDataSlotIndex);
				if (GUILayout.Button("Try Delete"))
				{
					saveSystemController.TryDelete(deleteSaveDataSlotIndex);
				}

				EditorGUILayout.EndHorizontal();
			}
		}
	}
}