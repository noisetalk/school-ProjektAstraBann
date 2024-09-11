using SaveToolbox.Runtime.Core.MonoBehaviours;
using UnityEditor;
#if !STB_ABOVE_2021_3
using UnityEditor.Experimental.SceneManagement;
#endif
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SaveToolbox.Editor
{
	[CustomEditor(typeof(LoadableObject))]
	public class LoadableObjectEditor : UnityEditor.Editor
	{
		private GUIStyle errorTextStyle;
		private GUIStyle boxStyle;

		private void CreateStyles()
		{
			if (errorTextStyle == null)
			{
				errorTextStyle = new GUIStyle(GUI.skin.label)
				{
					fontSize = 12,
					fontStyle = FontStyle.Bold,
					wordWrap = true,
					alignment = TextAnchor.UpperCenter,
					richText = true
				};
			}

			if (boxStyle == null)
			{
				boxStyle = new GUIStyle(GUI.skin.box)
				{
					padding = new RectOffset(15, 15, 15, 15),
					margin = new RectOffset(15, 15, 15, 15)
				};
			}
		}

		public override void OnInspectorGUI()
		{
			CreateStyles();
			var loadableObject = (LoadableObject)target;
			if (loadableObject == null) return;

			var notInDatabase = loadableObject.LoadableObjectId < 0;
			var notAPrefab = PrefabUtility.GetPrefabInstanceHandle(loadableObject) == null && !PrefabUtility.IsPartOfAnyPrefab(loadableObject);
			var notInPrefabEditMode = PrefabStageUtility.GetCurrentPrefabStage() == null;

			if ((notInDatabase || notAPrefab) && !Application.isPlaying && notInPrefabEditMode)
			{
				EditorGUILayout.BeginVertical(boxStyle);
				if (notInDatabase)
				{
					EditorGUILayout.LabelField("<color=red>ATTENTION!</color> This object is not in the loadable object database. Refresh/Rebuild the database. Go to Tools/SaveToolbox -> LoadableObjectDatabase.", errorTextStyle);
				}

				if (notAPrefab)
				{
					EditorGUILayout.LabelField("<color=red>ATTENTION!</color> This object is not a prefab. It must be a prefab in order to be loaded/spawned into a scene from save data.", errorTextStyle);
				}
				EditorGUILayout.EndVertical();
			}

			DrawDefaultInspector();

			if (GUILayout.Button("Regenerate Own SaveIdentifier"))
			{
				loadableObject.RegenerateOwnIdentifier();
				EditorUtility.SetDirty(loadableObject);
			}

			if (GUILayout.Button("Regenerate All Behaviour Identifiers"))
			{
				loadableObject.RegenerateIdentifiers();
				EditorUtility.SetDirty(loadableObject);
			}

			if (GUILayout.Button("Update Referenced Behaviours"))
			{
				loadableObject.UpdateReferencedBehaviours();
				EditorUtility.SetDirty(loadableObject);
			}
		}
	}
}