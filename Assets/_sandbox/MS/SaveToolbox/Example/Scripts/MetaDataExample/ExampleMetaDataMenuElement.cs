using System;
using SaveToolbox.Runtime.Core;
using SaveToolbox.Runtime.Core.ScriptableObjects;
using UnityEngine;
using UnityEngine.UI;

namespace SaveToolbox.Example.Scripts.MetaDataExample
{
	public class ExampleMetaDataMenuElement : MonoBehaviour
	{
		private const string SAVE_FOLDER_NAME = "ExampleMetaDataScene";

		public bool IsValid { get; set; }

		[SerializeField]
		private Text metaDataSlot;

		[SerializeField]
		private Text version;

		[SerializeField]
		private Text metaDataTime;

		[SerializeField]
		private Image metaDataImage;

		[SerializeField]
		private Button loadButton;

		[SerializeField]
		private GameObject enabledObject;

		[SerializeField]
		private GameObject disabledObject;

		private ExampleMetaSaveData exampleData;

		public event Action OnLoadedData;

		private int slot;

		private void OnEnable()
		{
			loadButton.onClick.AddListener(LoadSave);
		}

		private void LoadSave()
		{
			if (exampleData == null) return;

			var saveDataArgs = SaveToolboxPreferences.Instance.DefaultSaveSettings.Copy();
			saveDataArgs.RelativeFolderPath = $"{SAVE_FOLDER_NAME}";
			SaveToolboxSystem.Instance.TryLoadGame(saveDataArgs, exampleData.SlotIndex);
			OnLoadedData?.Invoke();
		}

		public void UpdateElement(SaveMetaData saveMetaData)
		{
			if (saveMetaData == null)
			{
				IsValid = false;
				exampleData = null;
				enabledObject.SetActive(false);
				disabledObject.SetActive(true);
				return;
			}

			enabledObject.SetActive(true);
			disabledObject.SetActive(false);

			// Is valid.
			if (saveMetaData is ExampleMetaSaveData exampleMetaSaveData)
			{
				exampleData = exampleMetaSaveData;
				IsValid = true;
				var sprite = Sprite.Create(exampleMetaSaveData.ImageTexture, new Rect(0, 0, exampleMetaSaveData.ImageTexture.width, exampleMetaSaveData.ImageTexture.height), Vector2.one * 0.5f);
				metaDataImage.sprite = sprite;
				metaDataSlot.text = exampleMetaSaveData.SlotIndex.ToString();
				version.text = saveMetaData.SaveVersion.ToString("F2");
				metaDataTime.text = exampleMetaSaveData.SaveTime;
			}
			else
			{
				IsValid = false;
				metaDataSlot.text = "INVALID.";
			}
		}

		private void OnDisable()
		{
			loadButton.onClick.RemoveListener(LoadSave);
		}
	}
}