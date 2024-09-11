using System;
using System.Security.Cryptography;
using SaveToolbox.Runtime.Attributes;
using SaveToolbox.Runtime.Serialization;
using UnityEngine;

namespace SaveToolbox.Runtime.Core
{
	[Serializable]
	public class SaveSettings
	{
		/// <summary>
		/// The name the game save data should be saved with.
		/// </summary>
		[field: SerializeField]
		public string SaveFileName { get; set; } = "SaveGameData";

		/// <summary>
		/// The folder path of where the game save data should be saved.
		/// </summary>
		[field: SerializeField]
		public string RelativeFolderPath { get; set; }

		/// <summary>
		/// The asset type that the game save data will be saved as, either JSON or Binary.
		/// </summary>
		[field: SerializeField]
		public StbFileFormat SaveFileFormat { get; set; } = StbFileFormat.Json;

		/// <summary>
		/// If saving as JSON should it use pretty print? With tabs and newline characters. Recommended for ease of reading.
		/// </summary>
		[field: SerializeField]
		public bool JsonPrettyPrint { get; set; } = true;

		/// <summary>
		/// The settings for serialization such as what will be serialized and how/under what conditions.
		/// </summary>
		[field: SerializeField]
		public StbSerializationSettings SerializationSettings { get; set; } = new StbSerializationSettings();

		/// <summary>
		/// The encryption settings of the game save data.
		/// </summary>
		[field: SerializeField]
		public StbEncryptionSettings StbEncryptionSettings { get; set; } = new StbEncryptionSettings();

		/// <summary>
		/// The compression settings of the game save data.
		/// </summary>
		[field: SerializeField]
		public StbCompressionType CompressionType { get; set; }

		/// <summary>
		/// If the loadable object already exists in the scene should we destroy it and rebuild it? Recommended to be true but less performant.
		/// </summary>
		[field: SerializeField]
		public bool RebuildLoadableObjects { get; set; } = true;

		/// <summary>
		/// Syncs the transforms for physics objects that are loaded in the scene.
		/// </summary>
		[field: SerializeField]
		public bool PhysicsSyncTransformsOnLoad { get; set; } = true;

		/// <summary>
		/// Should the scenes that are open be saved as well? This will cause them to be loaded when a game save data is loaded.
		/// </summary>
		[field: SerializeField]
		public bool SaveScene { get; set; }

		/// <summary>
		/// If we are saving scenes, how should we save their reference? By build index or path?
		/// </summary>
		[field: SerializeField]
		public StbSceneSavingType StbSceneSavingType { get; set; } = StbSceneSavingType.BuildIndex;

		/// <summary>
		/// If the data is saved/loaded asynchronously, should it freeze timescale when loading all the objects in.
		/// Recommended to be true otherwise you may get unexpected results.
		/// </summary>
		[field: SerializeField]
		public bool FreezeTimeScaleOnAsynchronousSaveLoad { get; set; } = true;

		/// <summary>
		/// Regenerates the initialization vector for the Aes encryption.
		/// </summary>
		public void RegenerateInitializationVector()
		{
			var aes = Aes.Create();
			aes.GenerateIV();
			StbEncryptionSettings.EncryptionInitializationVector = StbSaveEncryption.ConvertBytesToString(aes.IV);
		}

		/// <summary>
		/// Resets the serialization settings such as serialization targets and how to determine them to default.
		/// </summary>
		public void ResetSerializationSettingsToDefault()
		{
			SerializationSettings = new StbSerializationSettings();
		}

		public SaveSettings Copy()
		{
			var returnValue = new SaveSettings();
			returnValue.SaveFileName = SaveFileName;
			returnValue.RelativeFolderPath = RelativeFolderPath;
			returnValue.SaveFileFormat = SaveFileFormat;
			returnValue.JsonPrettyPrint = JsonPrettyPrint;
			returnValue.SerializationSettings = SerializationSettings;
			returnValue.StbEncryptionSettings = StbEncryptionSettings;
			returnValue.CompressionType = CompressionType;
			returnValue.RebuildLoadableObjects = RebuildLoadableObjects;
			returnValue.PhysicsSyncTransformsOnLoad = PhysicsSyncTransformsOnLoad;
			returnValue.SaveScene = SaveScene;
			returnValue.StbSceneSavingType = StbSceneSavingType;
			returnValue.FreezeTimeScaleOnAsynchronousSaveLoad = FreezeTimeScaleOnAsynchronousSaveLoad;
			return returnValue;
		}
	}

	public enum StbEncryptionType
	{
		None,
		Xor,
		Aes
	}

	public enum StbCompressionType
	{
		None,
		Gzip
	}

	public enum StbFileFormat
	{
		Json,
		Binary
	}

	public enum StbSceneSavingType
	{
		Path,
		BuildIndex
	}

	[Serializable]
	public class StbEncryptionSettings
	{
		[field: SerializeField]
		public StbEncryptionType EncryptionType { get; set; } = StbEncryptionType.None;

		[field: SerializeField]
		public string EncryptionKeyword { get; set; } = "S4v3T00lb0x";

		[field: SerializeField, ReadOnly]
		public string EncryptionInitializationVector { get; set; } = "xhWFy/bpsTyz4BBCzTmYNg==";
	}

	[Serializable]
	public class StbSerializationSettings
	{
		[field: SerializeField]
		public MemberSerializationSettings FieldSerializationSettings { get; set; } = new MemberSerializationSettings(true, false, true, true, true);

		[field: SerializeField]
		public MemberSerializationSettings PropertySerializationSettings { get; set; } = new MemberSerializationSettings(true, false, false, true, true);

		public StbSerializationSettings Copy()
		{
			return new StbSerializationSettings()
			{
				FieldSerializationSettings = new MemberSerializationSettings(FieldSerializationSettings.ValidTarget,
					FieldSerializationSettings.TargetAllPrivate, FieldSerializationSettings.TargetAllPublic,
					FieldSerializationSettings.TargetSerializeFieldAttribute,
					FieldSerializationSettings.TargetStbSerializeAttribute),

				PropertySerializationSettings = new MemberSerializationSettings(
					PropertySerializationSettings.ValidTarget, PropertySerializationSettings.TargetAllPrivate,
					PropertySerializationSettings.TargetAllPublic,
					PropertySerializationSettings.TargetSerializeFieldAttribute,
					PropertySerializationSettings.TargetStbSerializeAttribute)
			};
		}
	}

	[Serializable]
	public class MemberSerializationSettings
	{
		[field: SerializeField]
		public bool ValidTarget { get; set; }

		[field: SerializeField]
		public bool TargetAllPrivate { get; set; }

		[field: SerializeField]
		public bool TargetAllPublic { get; set; }

		[field: SerializeField]
		public bool TargetSerializeFieldAttribute { get; set; }

		[field: SerializeField]
		public bool TargetStbSerializeAttribute { get; set; }

		public MemberSerializationSettings() {}

		public MemberSerializationSettings(bool validTarget, bool targetAllPrivate, bool targetAllPublic, bool targetSerializeFieldAttribute, bool targetStbSerializeAttribute)
		{
			ValidTarget = validTarget;
			TargetAllPrivate = targetAllPrivate;
			TargetAllPublic = targetAllPublic;
			TargetSerializeFieldAttribute = targetSerializeFieldAttribute;
			TargetStbSerializeAttribute = targetStbSerializeAttribute;
		}
	}
}