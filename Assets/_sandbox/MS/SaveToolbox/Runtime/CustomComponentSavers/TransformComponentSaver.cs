using System;
using SaveToolbox.Runtime.BasicSaveableMonoBehaviours;
using SaveToolbox.Runtime.Utils;
using UnityEngine;

namespace SaveToolbox.Runtime.CustomComponentSavers
{
	[Serializable]
	public class TransformComponentSaver : AbstractComponentSaver<Transform>
	{
		/// <summary>
		/// Should the transform save the position?
		/// </summary>
		[SerializeField]
		private bool loadPosition = true;

		/// <summary>
		/// Should the transform save the rotation?
		/// </summary>
		[SerializeField]
		private bool loadRotation = true;

		/// <summary>
		/// Should the transform save the scale?
		/// </summary>
		[SerializeField]
		private bool loadScale = true;

		public override object Serialize()
		{
			var transformData = Target;
			var saveData = new TransformSaveData(loadPosition, transformData.position,
				loadRotation, transformData.rotation,
				loadScale, transformData.lossyScale);
			return saveData;
		}

		public override void Deserialize(object data)
		{
			var transformSaveData = (TransformSaveData)data;

			loadPosition = transformSaveData.SavePosition;
			loadRotation = transformSaveData.SaveRotation;
			loadScale = transformSaveData.SaveScale;

			// Apply values if need be.
			if (loadPosition) Target.position = transformSaveData.Position;
			if (loadRotation) Target.rotation = transformSaveData.Rotation;
			if (loadScale) Target.SetLossyScale(transformSaveData.Scale);

		}
	}
}