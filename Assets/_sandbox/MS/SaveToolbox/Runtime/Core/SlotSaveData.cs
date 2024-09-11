using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using SaveToolbox.Runtime.Attributes;
using SaveToolbox.Runtime.Core.MonoBehaviours;
using SaveToolbox.Runtime.Interfaces;
using UnityEngine;

namespace SaveToolbox.Runtime.Core
{
	/// <summary>
	/// The data structure in which game save data is stored.
	/// </summary>
	[Serializable]
	public class SlotSaveData
	{
		/// <summary>
		/// The meta data of the save. By default holds the slot index, version and an object to store any additional meta data.
		/// </summary>
		[SerializeField, StbSerialize]
		private SaveMetaData saveMetaData;
		public SaveMetaData SaveMetaData => saveMetaData;

		/// <summary>
		/// The actual game save data, that holds all scene objects ISaveDataEntities, LoadableObjects and saveable Non-MonoBehaviour class
		/// instances.
		/// </summary>
		[SerializeField, StbSerialize]
		private SaveData saveData;
		public SaveData SaveData => saveData;

		public SlotSaveData(SaveMetaData saveMetaData, SaveData saveData)
		{
			this.saveMetaData = saveMetaData;
			this.saveData = saveData;
		}

		public SlotSaveData()
		{
			saveData = new SaveData();
			saveMetaData = new SaveMetaData();
		}
	}

	/// <summary>
	/// The meta data of the save. By default holds the slot index, version and an object to store any additional meta data.
	/// </summary>
	[Serializable]
	public class SaveMetaData
	{
		/// <summary>
		/// The save version, useful for defining if a save should be migrated.
		/// </summary>
		[SerializeField, StbSerialize]
		protected double saveVersion;
		public double SaveVersion => saveVersion;

		/// <summary>
		/// Any additional meta data.
		/// </summary>
		[SerializeField, StbSerialize]
		protected object metaData;
		public object MetaData => metaData;

		public SaveMetaData(double saveVersion = 0.1, object metaData = null)
		{
			this.saveVersion = saveVersion;
			this.metaData = metaData;
		}

		public SaveMetaData()
		{
			saveVersion = 0.1;
			metaData = null;
		}
	}

	/// <summary>
	/// The actual game save data, that holds all scene objects ISaveDataEntities, LoadableObjects and saveable Non-MonoBehaviour class
	/// instances.
	/// </summary>
	[Serializable]
	public class SaveData
	{
		/// <summary>
		/// All save entity object data for the current state of the game.
		/// </summary>
		[SerializeField, StbSerialize]
		private List<SaveEntityObjectData> saveDataEntityObjects = new List<SaveEntityObjectData>();
		public ReadOnlyCollection<SaveEntityObjectData> SaveDataEntityObjects => saveDataEntityObjects.AsReadOnly();

		/// <summary>
		/// All LoadableObjects entities data.
		/// </summary>
		[SerializeField, StbSerialize]
		private List<LoadableObjectSaveData> loadableObjectSaveEntityDatas = new List<LoadableObjectSaveData>();
		public ReadOnlyCollection<LoadableObjectSaveData> LoadableObjectSaveEntityDatas => loadableObjectSaveEntityDatas.AsReadOnly();

		/// <summary>
		/// If we are saving scene paths in the save settings then when we save this will populate with all the paths of all currently
		/// active scenes if the user has additive scenes. NOTE: The first entry will be the active scene.
		/// </summary>
		[SerializeField, StbSerialize]
		private List<string> scenePaths;
		public List<string> ScenePaths => scenePaths;

		private Dictionary<string, object> saveDataEntityDictionary = new Dictionary<string, object>();
		private Dictionary<string, object> loadableObjectDictionary = new Dictionary<string, object>();

		public SaveData(List<SaveEntityObjectData> saveDataEntityObjects, List<LoadableObjectSaveData> loadableObjectSaveEntityDatas, List<string> scenePaths = null)
		{
			this.saveDataEntityObjects = saveDataEntityObjects;
			foreach (var saveDataEntityObject in this.saveDataEntityObjects)
			{
				saveDataEntityDictionary.Add(saveDataEntityObject.Identifier, null);
			}

			this.loadableObjectSaveEntityDatas = loadableObjectSaveEntityDatas;
			this.scenePaths = scenePaths ?? new List<string>();
		}

		public SaveData()
		{
			scenePaths = new List<string>();
			loadableObjectSaveEntityDatas = new List<LoadableObjectSaveData>();
			saveDataEntityObjects = new List<SaveEntityObjectData>();
		}

		/// <summary>
		/// Try get a save entity object of a ISaveDataEntity identifier.
		/// </summary>
		/// <param name="identifier">The ISaveDataEntity identifier.</param>
		/// <param name="data">The returned data.</param>
		/// <returns>If it was successful.</returns>
		public bool TryGetSaveDataEntityObject(string identifier, out object data)
		{
			data = null;
			if (saveDataEntityDictionary.TryGetValue(identifier, out var value))
			{
				data = value;
				return true;
			}

			return false;
		}

		/// <summary>
		/// Try find LoadableObject data of a specific ISaveDataEntity identifier.
		/// </summary>
		/// <param name="identifier">The ISaveDataEntity identifier.</param>
		/// <param name="data">The returned data.</param>
		/// <returns>If it was successful.</returns>
		public bool TryGetLoadableObjectData(string identifier, out object data)
		{
			data = null;
			if (loadableObjectDictionary.TryGetValue(identifier, out var value))
			{
				data = value;
				return true;
			}

			return false;
		}
	}
}
