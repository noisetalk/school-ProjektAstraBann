using System;
using SaveToolbox.Runtime.Core;
using SaveToolbox.Runtime.Core.ScriptableObjects;
using UnityEngine;
using UnityEngine.UI;

namespace SaveToolbox.Example.Scripts.MetaDataExample
{
	public class ExampleMetaDataSceneUIController : MonoBehaviour
	{
		private const string SAVE_FOLDER_NAME = "ExampleMetaDataScene";

		[SerializeField]
		private Camera targetCamera;

		[SerializeField]
		private ExampleMetaDataSelectionMenu exampleMetaDataSelectionMenu;

		[SerializeField]
		private Button saveSlot1Button;

		[SerializeField]
		private Button saveSlot2Button;

		[SerializeField]
		private Button saveSlot3Button;

		[SerializeField]
		private Button backToMenuButton;

		private void OnEnable()
		{
			saveSlot1Button.onClick.AddListener(SaveSlot1);
			saveSlot2Button.onClick.AddListener(SaveSlot2);
			saveSlot3Button.onClick.AddListener(SaveSlot3);
			backToMenuButton.onClick.AddListener(BackToMenu);
		}

		private void SaveSlot1()
		{
			var saveSettings = SaveToolboxPreferences.Instance.DefaultSaveSettings.Copy();
			saveSettings.RelativeFolderPath = $"{SAVE_FOLDER_NAME}";
			SaveToolboxSystem.Instance.TrySaveGame(new ExampleMetaSaveData(CaptureCameraViewToTexture2D(), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), 1, 0.11), saveSettings, 1);
		}

		private void SaveSlot2()
		{
			var saveSettings = SaveToolboxPreferences.Instance.DefaultSaveSettings.Copy();
			saveSettings.RelativeFolderPath = $"{SAVE_FOLDER_NAME}";
			SaveToolboxSystem.Instance.TrySaveGame(new ExampleMetaSaveData(CaptureCameraViewToTexture2D(), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), 2, 0.22), saveSettings, 2);
		}

		private void SaveSlot3()
		{
			var saveSettings = SaveToolboxPreferences.Instance.DefaultSaveSettings.Copy();
			saveSettings.RelativeFolderPath = $"{SAVE_FOLDER_NAME}";
			SaveToolboxSystem.Instance.TrySaveGame(new ExampleMetaSaveData(CaptureCameraViewToTexture2D(), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), 3, 0.33), saveSettings, 3);
		}

		private Texture2D CaptureCameraViewToTexture2D()
		{
			var renderTexture = new RenderTexture(targetCamera.pixelWidth, targetCamera.pixelHeight, 24);
			targetCamera.targetTexture = renderTexture;
			var screenShot = new Texture2D(targetCamera.pixelWidth, targetCamera.pixelHeight, TextureFormat.RGB24, false);
			targetCamera.Render();
			RenderTexture.active = renderTexture;
			screenShot.ReadPixels(new Rect(0, 0, targetCamera.pixelWidth, targetCamera.pixelHeight), 0, 0);
			targetCamera.targetTexture = null;
			RenderTexture.active = null;
			Destroy(renderTexture);
			return screenShot;
		}

		private void BackToMenu()
		{
			exampleMetaDataSelectionMenu.gameObject.SetActive(true);
			gameObject.SetActive(false);
		}

		private void OnDisable()
		{
			saveSlot1Button.onClick.RemoveListener(SaveSlot1);
			saveSlot2Button.onClick.RemoveListener(SaveSlot2);
			saveSlot3Button.onClick.RemoveListener(SaveSlot3);
			backToMenuButton.onClick.RemoveListener(BackToMenu);
		}
	}
}
