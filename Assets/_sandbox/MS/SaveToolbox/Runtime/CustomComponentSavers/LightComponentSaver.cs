using SaveToolbox.Runtime.BasicSaveableMonoBehaviours;
using UnityEngine;

namespace SaveToolbox.Runtime.CustomComponentSavers
{
	public class LightComponentSaver : AbstractComponentSaver<Light>
	{
		public override object Serialize()
		{
			return new LightSaveData(Target);
		}

		public override void Deserialize(object data)
		{
			var castData = (LightSaveData)data;
			Target.type = (LightType)castData.Type;
			Target.range = castData.Range;
			Target.intensity = castData.Intensity;
			Target.shadows = (LightShadows)castData.ShadowType;
			Target.renderMode = (LightRenderMode)castData.RenderMode;
			Target.cullingMask = castData.CullingMask;
			Target.spotAngle = castData.SpotAngle;
			Target.color = castData.Color;
		}
	}
}