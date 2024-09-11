using SaveToolbox.Runtime.Utils;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SaveToolbox.Editor
{
	[InitializeOnLoad]
	public class SceneIdChecker : MonoBehaviour
	{
#if UNITY_EDITOR
		static SceneIdChecker()
		{
			EditorSceneManager.sceneOpened += HandleSceneOpened;
		}

		private static void HandleSceneOpened(Scene scene, OpenSceneMode mode)
		{
			if (StbUtilities.CheckAllScenesForDuplicateIDs(out var duplicateIds))
			{
				Debug.LogError($"2 or more ISaveDataEntity behaviours with duplicate IDs were found. For safety all identifiers should be unique. List of IDs {string.Join(" | ", duplicateIds)}.");
				Debug.LogError($"Generate new identifiers from inside Tools/SaveToolbox and select the EditorTools tab.");
			}
		}
#endif
	}
}
