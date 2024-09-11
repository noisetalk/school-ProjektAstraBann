using System;
using SaveToolbox.Runtime.Attributes;
using SaveToolbox.Runtime.Core.MonoBehaviours;
using SaveToolbox.Runtime.Utils;
using UnityEngine;

namespace SaveToolbox.Runtime.BasicSaveableMonoBehaviours
{
	/// <summary>
	/// Saves data about the transform of the GameObject this component is attached to. Can also choose which parts of the transform are saved.
	/// </summary>
	[AddComponentMenu("SaveToolbox/SavingBehaviours/StbTransform")]
	public class StbTransform : SaveableMonoBehaviour
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

		private StbTransform()
		{
			DeserializationPriority = 1;
		}

		public override object Serialize()
		{
			var transformData = transform;
			var saveData = new TransformSaveData(loadPosition, transformData.position,
				loadRotation, transformData.rotation,
				loadScale, transformData.lossyScale);
			return saveData;
		}

		public override void Deserialize(object data)
		{
			if (data is TransformSaveData saveData)
			{
				loadPosition = saveData.SavePosition;
				loadRotation = saveData.SaveRotation;
				loadScale = saveData.SaveScale;

				// Apply values if need be.
				if (loadPosition) transform.position = saveData.Position;
				if (loadRotation) transform.rotation = saveData.Rotation;
				if (loadScale) transform.SetLossyScale(saveData.Scale);
			}
		}
	}

	[Serializable]
	public struct TransformSaveData
	{
		[SerializeField, StbSerialize]
		private bool savePosition;
		public bool SavePosition => savePosition;

		[SerializeField, StbSerialize]
		private Vector3 position;
		public Vector3 Position => position;

		[SerializeField, StbSerialize]
		private bool saveRotation;
		public bool SaveRotation => saveRotation;

		[SerializeField, StbSerialize]
		private Quaternion rotation;
		public Quaternion Rotation => rotation;

		[SerializeField, StbSerialize]
		private bool saveScale;
		public bool SaveScale => saveScale;

		[SerializeField, StbSerialize]
		private Vector3 scale;
		public Vector3 Scale => scale;

		public TransformSaveData(Vector3 position, Quaternion rotation, Vector3 scale)
		{
			this.position = position;
			this.rotation = rotation;
			this.scale = scale;
			savePosition = true;
			saveRotation = true;
			saveScale = true;
		}

		public TransformSaveData(bool savePosition, Vector3 position, bool saveRotation, Quaternion rotation, bool saveScale, Vector3 scale)
		{
			this.savePosition = savePosition;
			this.position = position;
			this.saveRotation = saveRotation;
			this.rotation = rotation;
			this.saveScale = saveScale;
			this.scale = scale;
		}
	}
}

