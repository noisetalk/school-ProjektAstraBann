using SaveToolbox.Runtime.Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SaveToolbox.Example.Scripts.SceneLoading
{
	[AddComponentMenu("SaveToolbox/Example/ExampleSceneLoadingUI")]
	public class ExampleSceneLoadingUI : MonoBehaviour
	{
		private const string RELATIVE_FOLDER_PATH = "SceneSavingExample";
		private const string TARGET_SCENE_NAME = "ExampleManyObjectsWithSceneSaving";

		[SerializeField]
		private Button enterSceneButton;

		[SerializeField]
		private Button loadSceneButton;

		private void OnEnable()
		{
			enterSceneButton.onClick.AddListener(HandleEnterScene);
			loadSceneButton.onClick.AddListener(HandleSceneLoaded);
		}

		private void HandleSceneLoaded()
		{
			var saveSettings = new SaveSettings
			{
				SaveScene = true,
				RelativeFolderPath = RELATIVE_FOLDER_PATH
			};
			SaveToolboxSystem.Instance.TryLoadGame(saveSettings);
		}

		private void HandleEnterScene()
		{
			SceneManager.LoadScene(TARGET_SCENE_NAME);
		}

		private void OnDisable()
		{
			enterSceneButton.onClick.RemoveListener(HandleEnterScene);
			loadSceneButton.onClick.RemoveListener(HandleSceneLoaded);
		}
	}
}