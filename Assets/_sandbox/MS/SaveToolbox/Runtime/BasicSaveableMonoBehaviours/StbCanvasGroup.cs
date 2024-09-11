using System;
using SaveToolbox.Runtime.Attributes;
using SaveToolbox.Runtime.Core.MonoBehaviours;
using UnityEngine;

namespace SaveToolbox.Runtime.BasicSaveableMonoBehaviours
{
	/// <summary>
	/// Saves data about the canvas group that is referenced.
	/// </summary>
	[AddComponentMenu("SaveToolbox/SavingBehaviours/StbCanvasGroup")]
	public class StbCanvasGroup : SaveableMonoBehaviour
	{
		/// <summary>
		/// The target canvas group.
		/// </summary>
		[SerializeField]
		private CanvasGroup canvasGroup;

		public override object Serialize()
		{
			if (canvasGroup == null)
			{
				if (!TryGetComponent(out canvasGroup)) throw new Exception($"Could not serialize object of type canvasGroup as there isn't one referenced or attached to the game object.");
			}
			return new CanvasGroupSaveData(canvasGroup);
		}

		public override void Deserialize(object data)
		{
			if (canvasGroup == null)
			{
				if (!TryGetComponent(out canvasGroup)) throw new Exception($"Could not deserialize object of type canvasGroup as there isn't one referenced or attached to the game object.");
			}
			var canvasGroupSaveData = (CanvasGroupSaveData)data;
			canvasGroup.alpha = canvasGroupSaveData.Alpha;
			canvasGroup.ignoreParentGroups = canvasGroupSaveData.IgnoreParentGroups;
			canvasGroup.interactable = canvasGroupSaveData.Interactable;
			canvasGroup.blocksRaycasts = canvasGroupSaveData.BlocksRaycast;
		}
	}

	[Serializable]
	public struct CanvasGroupSaveData
	{
		[SerializeField, StbSerialize]
		private float alpha;
		public float Alpha => alpha;

		[SerializeField, StbSerialize]
		private bool ignoreParentGroups;
		public bool IgnoreParentGroups => ignoreParentGroups;

		[SerializeField, StbSerialize]
		private bool interactable;
		public bool Interactable => interactable;

		[SerializeField, StbSerialize]
		private bool blocksRaycast;
		public bool BlocksRaycast => blocksRaycast;

		public CanvasGroupSaveData(float alpha, bool ignoreParentGroups, bool interactable, bool blocksRaycast)
		{
			this.alpha = alpha;
			this.ignoreParentGroups = ignoreParentGroups;
			this.interactable = interactable;
			this.blocksRaycast = blocksRaycast;
		}

		public CanvasGroupSaveData(CanvasGroup canvasGroup)
		{
			alpha = canvasGroup.alpha;
			ignoreParentGroups = canvasGroup.ignoreParentGroups;
			interactable = canvasGroup.interactable;
			blocksRaycast = canvasGroup.blocksRaycasts;
		}
	}
}