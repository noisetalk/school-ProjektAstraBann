using SaveToolbox.Runtime.BasicSaveableMonoBehaviours;
using SaveToolbox.Runtime.Core;
using UnityEngine;

namespace SaveToolbox.Example.Scripts
{
	/// <summary>
	/// An example of a save migrator. This example shows a way to pick up on a save data of a different version
	/// using the meta data. Once it has been identified the slot data objects can then be adjusted.
	/// </summary>
	public class ExampleSaveMigrator : AbstractSaveMigrator
	{
		public override SaveData Migrate(SaveData saveData, SaveMetaData saveMetaData)
		{
			if (saveMetaData.SaveVersion == 0.1)
			{
				foreach (var loadableObjectSaveData in saveData.SaveDataEntityObjects)
				{
					if (loadableObjectSaveData.ObjectValue is TransformSaveData transformSaveData)
					{
						// Reset all transform save data.
						var newTransformData = new TransformSaveData(true, Vector3.zero, true, Quaternion.identity, true, Vector3.one);
						loadableObjectSaveData.ObjectValue = newTransformData;
					}
				}
			}

			return saveData;
		}
	}
}