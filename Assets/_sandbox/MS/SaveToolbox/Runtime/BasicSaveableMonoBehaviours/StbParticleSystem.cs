using System;
using SaveToolbox.Runtime.Attributes;
using SaveToolbox.Runtime.Core.MonoBehaviours;
using UnityEngine;

namespace SaveToolbox.Runtime.BasicSaveableMonoBehaviours
{
	[AddComponentMenu("SaveToolbox/SavingBehaviours/StbParticleSystem")]
	public class StbParticleSystem : SaveableMonoBehaviour
	{
		[SerializeField]
		private ParticleSystem targetParticleSystem;

		public override object Serialize()
		{
			if (targetParticleSystem == null)
			{
				if (!TryGetComponent(out targetParticleSystem)) throw new Exception($"Could not serialize object of type targetParticleSystem as there isn't one referenced or attached to the game object.");
			}
			return new ParticleSystemSaveData(targetParticleSystem);
		}

		public override void Deserialize(object data)
		{
			if (targetParticleSystem == null)
			{
				if (!TryGetComponent(out targetParticleSystem)) throw new Exception($"Could not deserialize object of type targetParticleSystem as there isn't one referenced or attached to the game object.");
			}

			targetParticleSystem.Stop();
			targetParticleSystem.Clear();

			if (data is ParticleSystemSaveData particleSystemSaveData)
			{
				var particleSystemMain = targetParticleSystem.main;
				particleSystemMain.duration = particleSystemSaveData.Duration;
				particleSystemMain.playOnAwake = particleSystemSaveData.PlayOnAwake;
				particleSystemMain.loop = particleSystemSaveData.Looping;

				targetParticleSystem.randomSeed = particleSystemSaveData.PlaySeed;
				if (particleSystemSaveData.IsPlaying)
				{
					targetParticleSystem.Play();
				}
				else
				{
					targetParticleSystem.Stop();
				}
				targetParticleSystem.time = particleSystemSaveData.CurrentPlayTime;
			}
		}

		[ContextMenu("PlayParticle")]
		private void PlayParticle()
		{
			targetParticleSystem.Play();
		}
	}

	[Serializable]
	public struct ParticleSystemSaveData
	{
		[SerializeField, StbSerialize]
		private bool isPlaying;
		public bool IsPlaying => isPlaying;

		[SerializeField, StbSerialize]
		private float currentPlayTime;
		public float CurrentPlayTime => currentPlayTime;

		[SerializeField, StbSerialize]
		private uint playSeed;
		public uint PlaySeed => playSeed;

		[SerializeField, StbSerialize]
		private float duration;
		public float Duration => duration;

		[SerializeField, StbSerialize]
		private bool playOnAwake;
		public bool PlayOnAwake => playOnAwake;

		[SerializeField, StbSerialize]
		private bool looping;
		public bool Looping => looping;

		public ParticleSystemSaveData(bool isPlaying, float currentPlayTime, uint playSeed, float duration, bool playOnAwake, bool looping)
		{
			this.isPlaying = isPlaying;
			this.currentPlayTime = currentPlayTime;
			this.playSeed = playSeed;
			this.duration = duration;
			this.playOnAwake = playOnAwake;
			this.looping = looping;
		}

		public ParticleSystemSaveData(ParticleSystem particleSystem)
		{
			isPlaying = particleSystem.isPlaying;
			currentPlayTime = particleSystem.time;
			playSeed = particleSystem.randomSeed;
			var main = particleSystem.main;
			duration = main.duration;
			playOnAwake = main.playOnAwake;
			looping = main.loop;
		}
	}
}