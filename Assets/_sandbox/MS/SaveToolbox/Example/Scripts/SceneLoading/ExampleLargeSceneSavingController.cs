using SaveToolbox.Runtime.Core;
using SaveToolbox.Runtime.Core.MonoBehaviours;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SaveToolbox.Example.Scripts.SceneLoading
{
	public class ExampleLargeSceneSavingController : MonoBehaviour
	{
		private const string RELATIVE_FOLDER_PATH = "SceneSavingExample";
		private const string SCENE_NAME = "ExampleSceneSaving";

		[SerializeField]
		private Button saveButton;

		[SerializeField]
		private Button loadButton;

		[SerializeField]
		private Button goToSceneLoaderButton;

		[SerializeField]
		private Button spawnOneButton;

		[SerializeField]
		private Button spawnTenButton;

		[SerializeField]
		private Button resetButton;

		[SerializeField]
		private Transform spawnTransform;

		[SerializeField]
		private Vector3 spawnExtents;

		[SerializeField]
		private LoadableObject loadableObjectPrefab;

		private void Awake()
		{
			saveButton.onClick.AddListener(Save);
			loadButton.onClick.AddListener(Load);
			goToSceneLoaderButton.onClick.AddListener(HandleGoToSceneLoader);

			spawnOneButton.onClick.AddListener(SpawnOne);
			spawnTenButton.onClick.AddListener(SpawnTen);
			resetButton.onClick.AddListener(ResetState);
		}

		private void HandleGoToSceneLoader()
		{
			SceneManager.LoadScene(SCENE_NAME);
		}

		private void Save()
		{
			var saveSettings = new SaveSettings
			{
				SaveScene = true,
				RelativeFolderPath = RELATIVE_FOLDER_PATH
			};
#if STB_ASYNCHRONOUS_SAVING
#pragma warning disable CS4014
			SaveToolboxSystem.Instance.TrySaveGameAsync(saveSettings);
#pragma warning restore CS4014
#else
			SaveToolboxSystem.Instance.TrySaveGame(saveSettings);
#endif
		}

		private void Load()
		{
			var saveSettings = new SaveSettings
			{
				SaveScene = true,
				RelativeFolderPath = RELATIVE_FOLDER_PATH
			};
#if STB_ASYNCHRONOUS_SAVING
#pragma warning disable CS4014
			SaveToolboxSystem.Instance.TryLoadGameAsync(saveSettings);
#pragma warning restore CS4014
#else
			SaveToolboxSystem.Instance.TryLoadGame(saveSettings);
#endif
		}

		private void SpawnOne()
		{
			SpawnObject();
		}

		private void SpawnTen()
		{
			for (var i = 0; i < 10; i++)
			{
				SpawnObject();
			}
		}

		private void ResetState()
		{
			var loadableObjects = FindObjectsOfType<LoadableObject>();
			foreach (var loadableObject in loadableObjects)
			{
				Destroy(loadableObject.gameObject);
			}
		}

		private void SpawnObject()
		{
			var randomXPosition = Random.Range(-spawnExtents.x, spawnExtents.x);
			var randomYPosition = Random.Range(-spawnExtents.y, spawnExtents.y);
			var randomZPosition = Random.Range(-spawnExtents.z, spawnExtents.z);

			Instantiate(loadableObjectPrefab, spawnTransform.position + new Vector3(randomXPosition, randomYPosition, randomZPosition), Quaternion.identity);
		}


		private void OnDestroy()
		{
			saveButton.onClick.RemoveListener(Save);
			loadButton.onClick.RemoveListener(Load);
			goToSceneLoaderButton.onClick.RemoveListener(HandleGoToSceneLoader);

			spawnOneButton.onClick.RemoveListener(SpawnOne);
			spawnTenButton.onClick.RemoveListener(SpawnTen);
			resetButton.onClick.RemoveListener(ResetState);
		}
	}
}