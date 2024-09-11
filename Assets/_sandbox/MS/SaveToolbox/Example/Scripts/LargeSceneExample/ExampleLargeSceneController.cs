using SaveToolbox.Runtime.Core;
using SaveToolbox.Runtime.Core.MonoBehaviours;
using SaveToolbox.Runtime.Core.ScriptableObjects;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace SaveToolbox.Example.Scripts.LargeSceneExample
{
	public class ExampleLargeSceneController : MonoBehaviour
	{
		private const string SAVE_FOLDER_NAME = "ExampleLargeScene";

		[SerializeField]
		private Button saveButton;

		[SerializeField]
		private Button loadButton;

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
			spawnOneButton.onClick.AddListener(SpawnOne);
			spawnTenButton.onClick.AddListener(SpawnTen);
			resetButton.onClick.AddListener(ResetState);
		}

		private void Save()
		{
			var saveSettings = SaveToolboxPreferences.Instance.DefaultSaveSettings.Copy();
			saveSettings.RelativeFolderPath = $"{SAVE_FOLDER_NAME}";
#if STB_ASYNCHRONOUS_SAVING
#pragma warning disable CS4014
			SaveToolboxSystem.Instance.TrySaveGameAsync(saveSettings, 0);
#pragma warning restore CS4014
#else
			SaveToolboxSystem.Instance.TrySaveGame(saveSettings, 0);
#endif
		}

		private void Load()
		{
			var saveSettings = SaveToolboxPreferences.Instance.DefaultSaveSettings.Copy();
			saveSettings.RelativeFolderPath = $"{SAVE_FOLDER_NAME}";
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
			spawnOneButton.onClick.RemoveListener(SpawnOne);
			spawnTenButton.onClick.RemoveListener(SpawnTen);
			resetButton.onClick.RemoveListener(ResetState);
		}
	}
}
