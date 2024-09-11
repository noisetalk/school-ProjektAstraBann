using System;
using SaveToolbox.Runtime.Attributes;
using SaveToolbox.Runtime.Core;
using UnityEngine;

namespace SaveToolbox.Runtime.Interfaces
{
	/// <summary>
	/// The main interface that the majority of Save Toolbox is built around. It provides and API for getting serialized data off
	/// of object instances.
	/// </summary>
	[ExecuteAlways]
	public interface ISaveDataEntity : ISaveIdentifier
	{
		/// <summary>
		/// This function is used to return a serializable data object to be written to a file. The object can be
		/// of any btype however if it is not serializable it can cause breaks inside of the save pipeline.
		/// </summary>
		/// <returns>The serialize data object.</returns>
		object Serialize();

		/// <summary>
		/// A function to handle the data that was deserialized for this object.
		/// </summary>
		/// <param name="data"></param>
		void Deserialize(object data);

#if STB_ABOVE_2021_3
		/// <summary>
		/// Add the ISaveDataEntity to the save systems static list of ISaveDataEntities to save. This is useful for instances
		/// that are not MonoBehaviours.
		/// </summary>
		public void AddEntityToSaveData()
		{
			SaveToolboxSystem.AddSaveGameDataEntity(this);
		}

		/// <summary>
		/// Removes the ISaveDataEntity from the save systems static list of ISaveDataEntities to save. This is useful for instances
		/// that are not MonoBehaviours.
		/// </summary>
		public void RemoveEntityFromSaveData()
		{
			SaveToolboxSystem.RemoveSaveGameDataEntity(this);
		}
#endif
	}

	[Serializable]
	public class SaveEntityObjectData
	{
		[SerializeField, StbSerialize]
		private string identifier;
		public string Identifier => identifier;

		[field: SerializeField, StbSerialize]
		public object ObjectValue { get; set; }

		public SaveEntityObjectData(string identifier, object objectValue)
		{
			this.identifier = identifier;
			ObjectValue = objectValue;
		}

		public SaveEntityObjectData() {}
	}
}