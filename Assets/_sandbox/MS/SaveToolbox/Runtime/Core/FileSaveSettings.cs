using System;
using SaveToolbox.Runtime.Serialization.Binary;
using UnityEngine;

namespace SaveToolbox.Runtime.Core
{
	/// <summary>
	/// The save data arguments for saving something to memory. By default this is created by the SettingsScriptableObject for
	/// game save data. For any custom data you will need to pass in an instance of this class.
	/// </summary>
	[Serializable]
	public class FileSaveSettings
	{
		/// <summary>
		/// The name of the file.
		/// </summary>
		public string SaveFileName { get; set; }

		/// <summary>
		/// A custom save path for the file if it should have one.
		/// </summary>
		public string CustomSavePath { get; set; }

		/// <summary>
		/// The slot the data should be saved in, defaults to -1 which is no slot.
		/// </summary>
		public int SlotIndex { get; set; }

		/// <summary>
		/// What type of file should be created, JSON or Binary?
		/// </summary>
		public StbFileFormat SaveFileFormat { get; set; }

		/// <summary>
		/// What type of encryption should be used, if any?
		/// </summary>
		public StbEncryptionSettings StbEncryptionSettings { get; set; }

		/// <summary>
		/// What type of compression should be used, if any?
		/// </summary>
		public StbCompressionType CompressionType { get; set; }

		/// <summary>
		/// A custom json serializer if necessary.
		/// </summary>
		public StbJsonSerializer CustomJsonSerializer { get; set; }

		/// <summary>
		/// Custom serialization settings if necessary.
		/// </summary>
		public StbSerializationSettings SerializationSettings { get; set; }

		/// <summary>
		/// A custom binary serializer if necessary.
		/// </summary>
		public StbBinarySerializer CustomBinarySerializer { get; set; }

		public FileSaveSettings(string saveFileName = null, string customSavePath = null, int slotIndex = -1, StbFileFormat saveFileFormat = StbFileFormat.Json, StbSerializationSettings serializationSettings = null, StbEncryptionSettings stbEncryptionSettings = null, StbCompressionType compressionType = StbCompressionType.None, StbJsonSerializer customJsonSerializer = null, StbBinarySerializer customBinarySerializer = null)
		{
			if (string.IsNullOrEmpty(saveFileName))
			{
				throw new Exception("Tried creating save data without a name. A save file must have a name.");
			}

			SaveFileName = saveFileName;
			CustomSavePath = customSavePath;
			SlotIndex = slotIndex;
			SaveFileFormat = saveFileFormat;
			SerializationSettings = serializationSettings ?? new StbSerializationSettings();
			StbEncryptionSettings = stbEncryptionSettings ?? new StbEncryptionSettings();
			CompressionType = compressionType;
			CustomJsonSerializer = customJsonSerializer;
			CustomBinarySerializer = customBinarySerializer;
		}

		public FileSaveSettings(SaveSettings saveSettings, int slotIndex = -1)
		{
			SaveFileName = saveSettings.SaveFileName;
			CustomSavePath = $"{Application.persistentDataPath}/{saveSettings.RelativeFolderPath}";
			SlotIndex = slotIndex;
			SaveFileFormat = saveSettings.SaveFileFormat;
			SerializationSettings = saveSettings.SerializationSettings ?? new StbSerializationSettings();
			StbEncryptionSettings = saveSettings.StbEncryptionSettings ?? new StbEncryptionSettings();
			CompressionType = saveSettings.CompressionType;
		}

		public FileSaveSettings(FileSaveSettings fileSaveSettings)
		{
			SaveFileName = fileSaveSettings.SaveFileName;
			CustomSavePath = fileSaveSettings.CustomSavePath;
			SlotIndex = fileSaveSettings.SlotIndex;
			SaveFileFormat = fileSaveSettings.SaveFileFormat;
			SerializationSettings = fileSaveSettings.SerializationSettings ?? new StbSerializationSettings();
			StbEncryptionSettings = fileSaveSettings.StbEncryptionSettings ?? new StbEncryptionSettings();
			CompressionType = fileSaveSettings.CompressionType;
			CustomJsonSerializer = fileSaveSettings.CustomJsonSerializer;
			CustomBinarySerializer = fileSaveSettings.CustomBinarySerializer;
		}
	}
}