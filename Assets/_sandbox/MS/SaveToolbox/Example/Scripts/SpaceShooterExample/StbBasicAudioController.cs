using System.Collections.Generic;
using UnityEngine;

namespace SaveToolbox.Example.Scripts.SpaceShooterExample
{
	public class StbBasicAudioController : MonoBehaviour
	{
		public static StbBasicAudioController Instance { get; private set; }

		[SerializeField]
		private int audioSourceCount = 8;

		private List<AudioSource> audioSources = new List<AudioSource>();

		private int audioSourceIndex;

		private void Awake()
		{
			if (Instance == null)
			{
				Instance = this;
			}
			else
			{
				Destroy(gameObject);
				return;
			}

			for (var i = 0; i < audioSourceCount; i++)
			{
				var newAudioGameObject = new GameObject($"Audio Source. {i}");
				newAudioGameObject.transform.parent = transform;
				var audioSource = newAudioGameObject.AddComponent<AudioSource>();
				audioSource.playOnAwake = false;
				audioSources.Add(audioSource);
			}
		}

		public void PlaySound(AudioClip audioClip, Vector3 worldPosition)
		{
			var audioSource = audioSources[audioSourceIndex];
			audioSource.clip = audioClip;
			audioSource.transform.position = worldPosition;
			audioSource.Play();
			audioSourceIndex++;
			if (audioSourceIndex >= audioSourceCount)
			{
				audioSourceIndex = 0;
			}
		}
	}
}
