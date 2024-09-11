using System;
using SaveToolbox.Runtime.Core;
using UnityEngine;
using UnityEngine.Serialization;

namespace SaveToolbox.Example.Scripts
{
	/// <summary>
	/// A MonoBehaviour that is used to show the ability to save custom data objects, by custom data objects
	/// it is meant the data object that isn't a ISaveDataEntity and uses the SaveToolboxSystem. You can just pass the
	/// object to the SaveToolboxSystem and save it. Useful for situations where you may want to save data in a different file
	/// format.
	/// </summary>
	public class ExampleCustomSaveFile : MonoBehaviour
	{
		[SerializeField]
		private int saveSlot = -1;

		[SerializeField]
		private string fileName;

		[SerializeField]
		private StbFileFormat assetType;

		[SerializeField]
		private StbEncryptionSettings stbEncryptionSettings;

		[SerializeField]
		private StbCompressionType compressionType;

		[SerializeField]
		private CustomSaveData customSaveData;

		[ContextMenu("Save Custom File")]
		private void SaveCustomFile()
		{
			var saveArgs = new FileSaveSettings(fileName, slotIndex: saveSlot, saveFileFormat: assetType, stbEncryptionSettings: stbEncryptionSettings, compressionType: compressionType);
			SaveToolboxSystem.Instance.TryCreateSaveData(customSaveData, saveArgs);
		}

		[ContextMenu("Load Custom File")]
		private void LoadCustomFile()
		{
			var saveArgs = new FileSaveSettings(fileName, slotIndex: saveSlot, saveFileFormat: assetType, stbEncryptionSettings: stbEncryptionSettings, compressionType: compressionType);
			var loadedObject = SaveToolboxSystem.Instance.LoadData<CustomSaveData>(saveArgs);
			customSaveData = loadedObject;
		}
	}

	[Serializable]
	public class CustomSaveData
	{
		public string exampleString = "123";
		public int exampleInt = 456;
		public Vector3 exampleVector = new Vector3(7, 8, 9);
	}
}