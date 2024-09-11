using SaveToolbox.Runtime.Core.MonoBehaviours;
using SaveToolbox.Runtime.Core.ScriptableObjects;
using UnityEditor;
using UnityEngine;

namespace SaveToolbox.Editor
{
	[CustomEditor(typeof(LoadableObjectDatabase))]
	public class LoadableObjectDatabaseEditor : UnityEditor.Editor
	{
		private Vector2 loadableObjectDatabaseScrollValue;
		private GUIStyle databaseInfoBoxStyle;
		private GUIStyle databaseBoxStyle;
		private GUIStyle buttonStyle;

		private void CreateStyles()
		{
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
		}

		public override void OnInspectorGUI()
		{
			CreateStyles();

			var loadableObjectDatabase = (LoadableObjectDatabase)target;
			if (loadableObjectDatabase == null) return;

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
				serializedObject.ApplyModifiedProperties();
			}

			GUILayout.Space(100f);

			if (GUILayout.Button("Full Rebuild", buttonStyle))
			{
				loadableObjectDatabase.RebuildDatabase();
				EditorUtility.SetDirty(this);
				serializedObject.ApplyModifiedProperties();
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
	}
}
