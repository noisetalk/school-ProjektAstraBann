using System;
using SaveToolbox.Runtime.Attributes;
using SaveToolbox.Runtime.Core.MonoBehaviours;
using UnityEngine;

namespace SaveToolbox.Runtime.BasicSaveableMonoBehaviours
{
	[AddComponentMenu("SaveToolbox/SavingBehaviours/StbAudioSource")]
	public class StbAudioSource : SaveableMonoBehaviour
	{
		[SerializeField]
		private AudioSource audioSource;

		public override object Serialize()
		{
			if (audioSource == null)
			{
				if (!TryGetComponent(out audioSource)) throw new Exception($"Could not serialize object of type audioSource as there isn't one referenced or attached to the game object.");
			}

			return new AudioSourceSaveData(audioSource);
		}

		public override void Deserialize(object data)
		{
			if (audioSource == null)
			{
				if (!TryGetComponent(out audioSource)) throw new Exception($"Could not deserialize object of type audioSource as there isn't one referenced or attached to the game object.");
			}

			var audioSourceSaveData = (AudioSourceSaveData)data;

			audioSource.mute = audioSourceSaveData.Mute;
			audioSource.bypassEffects = audioSourceSaveData.BypassEffects;
			audioSource.bypassListenerEffects = audioSourceSaveData.BypassListenerEffects;
			audioSource.bypassReverbZones = audioSourceSaveData.BypassReverbZones;
			audioSource.playOnAwake = audioSourceSaveData.PlayOnAwake;
			audioSource.loop = audioSourceSaveData.Loop;
			audioSource.priority = audioSourceSaveData.Priority;
			audioSource.volume = audioSourceSaveData.Volume;
			audioSource.pitch = audioSourceSaveData.Pitch;
			audioSource.panStereo = audioSourceSaveData.StereoPan;
			audioSource.spatialBlend = audioSourceSaveData.SpatialBlend;
			audioSource.reverbZoneMix = audioSourceSaveData.ReverbZoneMix;
		}
	}

	[Serializable]
	public struct AudioSourceSaveData
	{
		[SerializeField, StbSerialize]
		private bool mute;
		public bool Mute => mute;

		[SerializeField, StbSerialize]
		private bool bypassEffects;
		public bool BypassEffects => bypassEffects;

		[SerializeField, StbSerialize]
		private bool bypassListenerEffects;
		public bool BypassListenerEffects => bypassListenerEffects;

		[SerializeField, StbSerialize]
		private bool bypassReverbZones;
		public bool BypassReverbZones => bypassReverbZones;

		[SerializeField, StbSerialize]
		private bool playOnAwake;
		public bool PlayOnAwake => playOnAwake;

		[SerializeField, StbSerialize]
		private bool loop;
		public bool Loop => loop;

		[SerializeField, StbSerialize]
		private int priority;
		public int Priority => priority;

		[SerializeField, StbSerialize]
		private float volume;
		public float Volume => volume;

		[SerializeField, StbSerialize]
		private float pitch;
		public float Pitch => pitch;

		[SerializeField, StbSerialize]
		private float stereoPan;
		public float StereoPan => stereoPan;

		[SerializeField, StbSerialize]
		private float spatialBlend;
		public float SpatialBlend => spatialBlend;

		[SerializeField, StbSerialize]
		private float reverbZoneMix;
		public float ReverbZoneMix => reverbZoneMix;

		public AudioSourceSaveData(bool mute, bool bypassEffects, bool bypassListenerEffects, bool bypassReverbZones, bool playOnAwake, bool loop, int priority, float volume, float pitch, float stereoPan, float spatialBlend, float reverbZoneMix)
		{
			this.mute = mute;
			this.bypassEffects = bypassEffects;
			this.bypassListenerEffects = bypassListenerEffects;
			this.bypassReverbZones = bypassReverbZones;
			this.playOnAwake = playOnAwake;
			this.loop = loop;
			this.priority = priority;
			this.volume = volume;
			this.pitch = pitch;
			this.stereoPan = stereoPan;
			this.spatialBlend = spatialBlend;
			this.reverbZoneMix = reverbZoneMix;
		}

		public AudioSourceSaveData(AudioSource audioSource)
		{
			mute = audioSource.mute;
			bypassEffects = audioSource.bypassEffects;
			bypassListenerEffects = audioSource.bypassListenerEffects;
			bypassReverbZones = audioSource.bypassReverbZones;
			playOnAwake = audioSource.playOnAwake;
			loop = audioSource.loop;
			priority = audioSource.priority;
			volume = audioSource.volume;
			pitch = audioSource.pitch;
			stereoPan = audioSource.panStereo;
			spatialBlend = audioSource.spatialBlend;
			reverbZoneMix = audioSource.reverbZoneMix;
		}
	}
}