using System;
using SaveToolbox.Runtime.Core;
using SaveToolbox.Runtime.Interfaces;
using UnityEngine;

namespace SaveToolbox.Example.Scripts
{
	/// <summary>
	/// An example of Non-MonoBehaviour data saving.
	/// </summary>
	[Serializable]
	public class ExampleTestData : ISaveDataEntity
	{
		public string SaveIdentifier { get; set; } = "ExampleTestData";
		public int DeserializationPriority { get; set; }
		public string LoadableObjectId { get; set; }

		[SerializeField]
		private float randomNumber = 5f;

		public ExampleTestData()
		{
			SaveToolboxSystem.AddSaveGameDataEntity(this);
		}

		object ISaveDataEntity.Serialize()
		{
			return this;
		}

		void ISaveDataEntity.Deserialize(object data)
		{
			if (data is ExampleTestData testData)
			{
				randomNumber = testData.randomNumber;
			}
		}

		~ExampleTestData()
		{
			SaveToolboxSystem.RemoveSaveGameDataEntity(this);
		}
	}
}
