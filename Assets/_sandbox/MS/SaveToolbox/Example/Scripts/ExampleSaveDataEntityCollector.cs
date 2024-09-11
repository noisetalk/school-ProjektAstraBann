using System.Collections.Generic;
using System.Linq;
using SaveToolbox.Runtime.Core;
using SaveToolbox.Runtime.Core.MonoBehaviours;
using SaveToolbox.Runtime.Interfaces;
using SaveToolbox.Runtime.Utils;

namespace SaveToolbox.Example.Scripts
{
	/// <summary>
	/// An example of how to use the save entity collector. This one when collecting data will not collect any loadable objects
	/// with the id of 0. So no objects with an id of 0 will be saved or spawned on load.
	/// </summary>
	public class ExampleSaveDataEntityCollector : AbstractSaveDataEntityCollector
	{
		public override List<ISaveDataEntity> GetAllISaveDataEntities(bool orderedByPriority = true)
		{
			var saveDataEntityObjects = StbUtilities.GetAllObjectsInAllScenes<ISaveDataEntity>();
			saveDataEntityObjects.AddRange(SaveToolboxSystem.GetAllNonMonoBehaviourISaveDataEntities());

			if (orderedByPriority)
			{
				saveDataEntityObjects = saveDataEntityObjects.OrderByDescending(saveEntityObject =>
				{
					if (saveEntityObject is ISaveEntityDeserializationPriority iSaveEntityDeserializationPriority)
					{
						return iSaveEntityDeserializationPriority.DeserializationPriority;
					}

					return 0;
				}).ToList();
			}

			return saveDataEntityObjects;
		}

		public override List<LoadableObject> GetLoadableObjects()
		{
			var loadableObjects = StbUtilities.GetAllObjectsInAllScenes<LoadableObject>();
			for (var i = loadableObjects.Count - 1; i >= 0; i--)
			{
				// Remove any objects with an id of 1.
				if (loadableObjects[i].LoadableObjectId == 0)
				{
					loadableObjects.RemoveAt(i);
				}
			}
			return loadableObjects;
		}

		public override List<ISaveDataEntity> GetMonoBehaviourISaveDataEntities()
		{
			return StbUtilities.GetAllObjectsInAllScenes<ISaveDataEntity>();
		}
	}
}