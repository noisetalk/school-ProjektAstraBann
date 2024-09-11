using SaveToolbox.Runtime.Core;
using SaveToolbox.Runtime.Core.ScriptableObjects;
using UnityEngine;
using UnityEngine.UI;

namespace SaveToolbox.Example.Scripts.MetaDataExample
{
	public class ExampleMetaDataSelectionMenu : MonoBehaviour
	{
		private const string SAVE_FOLDER_NAME = "ExampleMetaDataScene";

		[SerializeField]
		private ExampleMetaDataMenuElement[] metaDataSelectionMenus;

		[SerializeField]
		private ExampleMetaDataSceneUIController exampleMetaDataSceneUIController;

		[SerializeField]
		private Button createSaveButton;

		private void OnEnable()
		{
			UpdateMetaDatas();
			createSaveButton.onClick.AddListener(OpenSceneController);
			foreach (var metaDataSelectionMenu in metaDataSelectionMenus)
			{
				metaDataSelectionMenu.OnLoadedData += HandleDataLoaded;
			}
		}

		private void HandleDataLoaded()
		{
			OpenSceneController();
		}

		private void OpenSceneController()
		{
			exampleMetaDataSceneUIController.gameObject.SetActive(true);
			gameObject.SetActive(false);
		}

		private void UpdateMetaDatas()
		{
			var saveDataArgs = new FileSaveSettings(SaveToolboxPreferences.Instance.DefaultSaveSettings);
			saveDataArgs.CustomSavePath = $"{Application.persistentDataPath}/{SAVE_FOLDER_NAME}";
			var saveMetaDatas = SaveToolboxSystem.Instance.LoadAllSaveGameMetaDatas<ExampleMetaSaveData>(saveDataArgs);
			for (var i = 0; i < 3; i++)
			{
				var currentMetaData = metaDataSelectionMenus[i];
				if (i < saveMetaDatas.Length && saveMetaDatas[i] != null)
				{
					currentMetaData.UpdateElement(saveMetaDatas[i]);
				}
				else
				{
					currentMetaData.UpdateElement(null);
				}
			}
		}

		private void OnDisable()
		{
			createSaveButton.onClick.RemoveListener(OpenSceneController);
			foreach (var metaDataSelectionMenu in metaDataSelectionMenus)
			{
				metaDataSelectionMenu.OnLoadedData -= HandleDataLoaded;
			}
		}
	}
}
