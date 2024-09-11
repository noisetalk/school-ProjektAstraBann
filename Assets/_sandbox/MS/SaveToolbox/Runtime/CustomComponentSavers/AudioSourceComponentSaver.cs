using SaveToolbox.Runtime.BasicSaveableMonoBehaviours;
using UnityEngine;

namespace SaveToolbox.Runtime.CustomComponentSavers
{
	public class AudioSourceComponentSaver : AbstractComponentSaver<AudioSource>
	{
		public override object Serialize()
		{
			return new AudioSourceSaveData(Target);
		}

		public override void Deserialize(object data)
		{
			var audioSourceSaveData = (AudioSourceSaveData)data;

			Target.mute = audioSourceSaveData.Mute;
			Target.bypassEffects = audioSourceSaveData.BypassEffects;
			Target.bypassListenerEffects = audioSourceSaveData.BypassListenerEffects;
			Target.bypassReverbZones = audioSourceSaveData.BypassReverbZones;
			Target.playOnAwake = audioSourceSaveData.PlayOnAwake;
			Target.loop = audioSourceSaveData.Loop;
			Target.priority = audioSourceSaveData.Priority;
			Target.volume = audioSourceSaveData.Volume;
			Target.pitch = audioSourceSaveData.Pitch;
			Target.panStereo = audioSourceSaveData.StereoPan;
			Target.spatialBlend = audioSourceSaveData.SpatialBlend;
			Target.reverbZoneMix = audioSourceSaveData.ReverbZoneMix;
		}
	}
}