using System.Collections.Generic;
using SaveToolbox.Runtime.Core.MonoBehaviours;
using SaveToolbox.Runtime.Interfaces;

namespace SaveToolbox.Runtime.Core
{
	public abstract class AbstractSaveDataEntityCollector
	{
		public abstract List<ISaveDataEntity> GetAllISaveDataEntities(bool orderedByPriority = true);
		public abstract List<LoadableObject> GetLoadableObjects();
		public abstract List<ISaveDataEntity> GetMonoBehaviourISaveDataEntities();
	}
}