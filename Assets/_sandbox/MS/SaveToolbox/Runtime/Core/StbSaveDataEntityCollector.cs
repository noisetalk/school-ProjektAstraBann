using System.Collections.Generic;
using System.Linq;
using SaveToolbox.Runtime.Core.MonoBehaviours;
using SaveToolbox.Runtime.Interfaces;
using SaveToolbox.Runtime.Utils;

namespace SaveToolbox.Runtime.Core
{
	public class StbSaveDataEntityCollector : AbstractSaveDataEntityCollector
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
			return StbUtilities.GetAllObjectsInAllScenes<LoadableObject>();
		}

		public override List<ISaveDataEntity> GetMonoBehaviourISaveDataEntities()
		{
			return StbUtilities.GetAllObjectsInAllScenes<ISaveDataEntity>();
		}
	}
}