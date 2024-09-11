using SaveToolbox.Runtime.BasicSaveableMonoBehaviours;
using UnityEngine;

namespace SaveToolbox.Runtime.CustomComponentSavers
{
	public class CanvasGroupComponentSaver : AbstractComponentSaver<CanvasGroup>
	{
		public override object Serialize()
		{
			return new CanvasGroupSaveData(Target);
		}

		public override void Deserialize(object data)
		{
			var canvasGroupSaveData = (CanvasGroupSaveData)data;
			Target.alpha = canvasGroupSaveData.Alpha;
			Target.ignoreParentGroups = canvasGroupSaveData.IgnoreParentGroups;
			Target.interactable = canvasGroupSaveData.Interactable;
			Target.blocksRaycasts = canvasGroupSaveData.BlocksRaycast;
		}
	}
}