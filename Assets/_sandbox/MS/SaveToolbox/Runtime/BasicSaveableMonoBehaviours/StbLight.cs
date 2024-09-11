using System;
using SaveToolbox.Runtime.Attributes;
using SaveToolbox.Runtime.Core.MonoBehaviours;
using UnityEngine;

namespace SaveToolbox.Runtime.BasicSaveableMonoBehaviours
{
	/// <summary>
	/// Saves data about the light that is referenced.
	/// </summary>
	[AddComponentMenu("SaveToolbox/SavingBehaviours/StbLight")]
	public class StbLight : SaveableMonoBehaviour
	{
		/// <summary>
		/// The referenced light.
		/// </summary>
		[SerializeField]
		private new Light light;

		public override object Serialize()
		{
			if (light == null)
			{
				if (!TryGetComponent(out light)) throw new Exception($"Could not serialize object of type light as there isn't one referenced or attached to the game object.");
			}
			return new LightSaveData(light);
		}

		public override void Deserialize(object data)
		{
			if (light == null)
			{
				if (!TryGetComponent(out light)) throw new Exception($"Could not deserialize object of type light as there isn't one referenced or attached to the game object.");
			}
			var castData = (LightSaveData)data;
			light.type = (LightType)castData.Type;
			light.range = castData.Range;
			light.intensity = castData.Intensity;
			light.shadows = (LightShadows)castData.ShadowType;
			light.renderMode = (LightRenderMode)castData.RenderMode;
			light.cullingMask = castData.CullingMask;
			light.spotAngle = castData.SpotAngle;
			light.color = castData.Color;
		}
	}

	[Serializable]
	public struct LightSaveData
	{
		[SerializeField, StbSerialize]
		private int type;
		public int Type => type;

		[SerializeField, StbSerialize]
		private float range;
		public float Range => range;

		[SerializeField, StbSerialize]
		private float intensity;
		public float Intensity => intensity;

		[SerializeField, StbSerialize]
		private int shadowType;
		public int ShadowType => shadowType;

		[SerializeField, StbSerialize]
		private int renderMode;
		public int RenderMode => renderMode;

		[SerializeField, StbSerialize]
		private int cullingMask;
		public int CullingMask => cullingMask;

		[SerializeField, StbSerialize]
		private float spotAngle;
		public float SpotAngle => spotAngle;

		[SerializeField, StbSerialize]
		private Color color;
		public Color Color => color;

		public LightSaveData(Light light)
		{
			type = (int)light.type;
			range = light.range;
			intensity = light.intensity;
			shadowType = (int)light.shadows;
			renderMode = (int)light.renderMode;
			cullingMask = light.cullingMask;
			spotAngle = light.spotAngle;
			color = light.color;
		}
	}
}