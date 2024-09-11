using System;
using System.Collections.Generic;
using SaveToolbox.Runtime.Core;
using SaveToolbox.Runtime.Core.MonoBehaviours;
using SaveToolbox.Runtime.Core.ScriptableObjects;
using SaveToolbox.Runtime.Interfaces;
using SaveToolbox.Runtime.Utils;
using UnityEditor;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

namespace SaveToolbox.Editor
{
	public class SaveToolboxEditorWindow : EditorWindow
	{
		[field: SerializeField]
		public SaveToolBoxEditorTab CurrentTab { get; private set; }

		private bool isSettingsFoldOut;
		private Vector2 saveSettingsScrollValue;
		private Vector2 duplicatesScrollValue;
		private static SaveToolboxEditorWindow editorWindow;
		private GUIStyle databaseInfoBoxStyle;
		private GUIStyle settingsInfoBoxStyle;
		private GUIStyle databaseBoxStyle;
		private GUIStyle buttonStyle;
		private GUIStyle duplicateButtonsStyle;
		private GUIStyle duplicateText;

		private ListRequest listRequest;

		private Vector2 loadableObjectDatabaseScrollValue;
		private List<(string, MonoBehaviour)> duplicateMonoBehaviours = new List<(string, MonoBehaviour)>();

		[MenuItem("Tools/Save Toolbox")]
		public static void OpenWindow()
		{
			editorWindow = GetWindow<SaveToolboxEditorWindow>();
			editorWindow.minSize = new Vector2(300f, 500f);
		}

		private void InitializeStyles()
		{
			// Can sometimes be null so we null check before hand.
			if (EditorStyles.helpBox != null)
			{
				settingsInfoBoxStyle = new GUIStyle(EditorStyles.helpBox)
				{
					fontSize = 13,
					alignment = TextAnchor.UpperLeft,
					richText = true
				};
			}

			if (EditorStyles.textField != null)
			{
				new GUIStyle(EditorStyles.textField)
				{
					fontSize = 12,
					alignment = TextAnchor.MiddleLeft,
					richText = true
				};
			}

			if (buttonStyle == null)
			{
				buttonStyle = new GUIStyle(GUI.skin.button)
				{
					fontSize = 22,
					fixedHeight = 45,
					fixedWidth = 200,
					alignment = TextAnchor.MiddleCenter
				};
			}

			if (databaseInfoBoxStyle == null)
			{
				databaseInfoBoxStyle = new GUIStyle(EditorStyles.helpBox)
				{
					fontSize = 13,
					alignment = TextAnchor.UpperCenter,
					richText = true
				};
			}

			if (databaseBoxStyle == null)
			{
				databaseBoxStyle = new GUIStyle(GUI.skin.box)
				{
					padding = new RectOffset(15, 15, 15, 15),
					margin = new RectOffset(15, 15, 15, 15)
				};
			}

			if (buttonStyle == null)
			{
				buttonStyle = new GUIStyle(GUI.skin.button)
				{
					fontSize = 22,
					fixedHeight = 45,
					fixedWidth = 200,
					alignment = TextAnchor.MiddleCenter
				};
			}

			if (duplicateButtonsStyle == null)
			{
				duplicateButtonsStyle = new GUIStyle(GUI.skin.button)
				{
					fontSize = 14,
					fixedHeight = 45,
					fixedWidth = 450,
					alignment = TextAnchor.MiddleCenter
				};
			}

			if (duplicateText == null)
			{
				duplicateText = new GUIStyle(GUI.skin.label)
				{
					fontSize = 16,
					fixedHeight = 40f,
					fixedWidth = 350f,
					fontStyle = FontStyle.Bold,
					alignment = TextAnchor.MiddleCenter
				};
			}
		}

		private void OnGUI()
		{
			CurrentTab = (SaveToolBoxEditorTab)GUILayout.Toolbar((int)CurrentTab, Enum.GetNames(typeof(SaveToolBoxEditorTab)));
			switch (CurrentTab)
			{
				case SaveToolBoxEditorTab.SaveSettings:
					DrawSettingsTab();
					break;
				case SaveToolBoxEditorTab.LoadableObjectDatabase:
					DrawLoadableObjectDatabaseTab();
					break;
				case SaveToolBoxEditorTab.EditorTools:
					DrawEditorToolsTab();
					break;
				case SaveToolBoxEditorTab.ScriptableObjectDatabase:
					DrawScriptableObjectDatabaseTab();
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		private void DrawEditorToolsTab()
		{
			InitializeStyles();
			EditorGUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();

			if (GUILayout.Button("Generate New SaveIdentifier For All Scene Save Behaviours", duplicateButtonsStyle))
			{
				duplicateMonoBehaviours = StbUtilities.GetAllMonoBehavioursWithDuplicateIds();

				StbUtilities.GenerateNewIdsForAllSceneSaveDataEntities();

				foreach (var duplicateMonoBehaviour in duplicateMonoBehaviours)
				{
					EditorUtility.SetDirty(duplicateMonoBehaviour.Item2);
					PrefabUtility.RecordPrefabInstancePropertyModifications(duplicateMonoBehaviour.Item2);
				}

				duplicateMonoBehaviours = StbUtilities.GetAllMonoBehavioursWithDuplicateIds();
			}

			GUILayout.Space(100f);
			if (GUILayout.Button("Get All Objects With Duplicate IDs.", duplicateButtonsStyle))
			{
				duplicateMonoBehaviours = StbUtilities.GetAllMonoBehavioursWithDuplicateIds();
			}

			GUILayout.FlexibleSpace();
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginVertical();

			if (duplicateMonoBehaviours.Count == 0)
			{
				EditorGUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				EditorGUILayout.LabelField("Could not find any objects with duplicate ids.", duplicateText);
				GUILayout.FlexibleSpace();
				EditorGUILayout.EndHorizontal();
			}
			else
			{
				duplicatesScrollValue = EditorGUILayout.BeginScrollView(duplicatesScrollValue);
				for (var index = duplicateMonoBehaviours.Count - 1; index >= 0; index--)
				{
					var duplicateObject = duplicateMonoBehaviours[index];
					EditorGUILayout.BeginHorizontal();

					if (GUILayout.Button("Regenerate"))
					{
						var iSaveDataEntity = (ISaveDataEntity)duplicateObject.Item2;
#if STB_ABOVE_2021_3
						iSaveDataEntity.GenerateNewIdentifier();
#else
						iSaveDataEntity.SaveIdentifier = Guid.NewGuid().ToString();
#endif
						duplicateMonoBehaviours.RemoveAt(index);
						EditorUtility.SetDirty(duplicateObject.Item2);
						PrefabUtility.RecordPrefabInstancePropertyModifications(duplicateObject.Item2);
					}

					EditorGUI.BeginDisabledGroup(true);
					EditorGUILayout.ObjectField(duplicateObject.Item2.name, duplicateObject.Item2, typeof(MonoBehaviour), true);
					EditorGUI.EndDisabledGroup();

					EditorGUILayout.LabelField($"Save Identifier: {duplicateObject.Item1}");
					EditorGUILayout.LabelField($"Component Name: {duplicateObject.Item2.GetType().Name}");

					EditorGUILayout.EndHorizontal();
				}

				EditorGUILayout.EndScrollView();
			}
			EditorGUILayout.EndVertical();

		}

		private void DrawSettingsTab()
		{
			saveSettingsScrollValue = EditorGUILayout.BeginScrollView(saveSettingsScrollValue);
			EditorGUIUtility.labelWidth = 360f;
			var saveSettings = SaveToolboxPreferences.Instance;
			if (saveSettings == null)
			{
				GUILayout.Label("<color=red>Could not find a save settings file scriptable object!</color>", settingsInfoBoxStyle);
				return;
			}

			GUILayout.Label("Hover the label of each field for a description on how they work.");

			var customEditor = UnityEditor.Editor.CreateEditor(saveSettings);
			customEditor.OnInspectorGUI();

			EditorGUILayout.EndScrollView();
		}

		private void DrawLoadableObjectDatabaseTab()
		{
			InitializeStyles();
			var loadableObjectDatabase = LoadableObjectDatabase.Instance;
			if (loadableObjectDatabase == null)
			{
				GUILayout.Label("<color=red>Could not find a loadable object database scriptable object!</color>", settingsInfoBoxStyle);
				return;
			}

			EditorGUILayout.LabelField("This is the loadable object database. This references a scriptable object in the SaveToolbox package." +
			                           " \n It is used to be able to spawn loadable objects at runtime." +
			                           "\n If the system needs to spawn a loadable object when loading a save it must first be referenced in the database." +
			                           "\n For the database to reference the object you can choose to <b><color=red>Rebuild</color></b> or <b><color=red>Refresh</color></b> the database." +
			                           "\n <b><color=red>Rebuild</color> = Clear the database. Find all loadable objects, it will also break any references that exist on the current objects.</b>" +
			                           "\n <b><color=red>Refresh</color> = Keep all current references but add any new ones it can find.</b>", databaseInfoBoxStyle);

			EditorGUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			if (GUILayout.Button("Refresh", buttonStyle))
			{
				loadableObjectDatabase.RefreshDatabase();
				EditorUtility.SetDirty(this);
			}

			GUILayout.Space(100f);

			if (GUILayout.Button("Full Rebuild", buttonStyle))
			{
				loadableObjectDatabase.RebuildDatabase();
				EditorUtility.SetDirty(this);
			}

			GUILayout.FlexibleSpace();
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.Space();

			GUILayout.Label($"Total database loadable objects: {loadableObjectDatabase.loadableObjects.Count}", databaseInfoBoxStyle);

			EditorGUILayout.Space();

			EditorGUILayout.BeginVertical();

			loadableObjectDatabaseScrollValue = EditorGUILayout.BeginScrollView(loadableObjectDatabaseScrollValue);
			for (var index = 0; index < loadableObjectDatabase.loadableObjects.Count; index++)
			{
				var loadableObject = loadableObjectDatabase.loadableObjects[index];
				EditorGUILayout.BeginVertical();
				if (loadableObject != null)
				{
					EditorGUILayout.ObjectField(loadableObject.name, loadableObject, typeof(LoadableObject), false);
				}
				else
				{
					EditorGUILayout.LabelField($"Loadable Object {index}: Null loadable object found! Recommended: Rebuild database!");
				}

				EditorGUILayout.EndVertical();
			}

			EditorGUILayout.EndScrollView();
			EditorGUILayout.EndVertical();
		}

		private void DrawScriptableObjectDatabaseTab()
		{
			InitializeStyles();
			var scriptableObjectDatabase = ScriptableObjectDatabase.Instance;
			if (scriptableObjectDatabase == null)
			{
				GUILayout.Label("<color=red>Could not find a scriptable object database scriptable object!</color>", settingsInfoBoxStyle);
				return;
			}

			var customEditor = UnityEditor.Editor.CreateEditor(scriptableObjectDatabase);
			customEditor.OnInspectorGUI();
		}

		public enum SaveToolBoxEditorTab
		{
			SaveSettings,
			LoadableObjectDatabase,
			EditorTools,
			ScriptableObjectDatabase
		}
	}
}
