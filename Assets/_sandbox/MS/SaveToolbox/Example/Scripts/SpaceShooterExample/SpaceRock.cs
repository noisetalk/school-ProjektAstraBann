using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace SaveToolbox.Example.Scripts.SpaceShooterExample
{
	public class SpaceRock : MonoBehaviour
	{
		[SerializeField]
		private Rigidbody2D rockRigidbody;

		[SerializeField]
		private Vector2 minRandomVelocity;

		[SerializeField]
		private Vector2 maxRandomVelocity;

		[SerializeField]
		private ParticleSystem explosionParticleSystem;

		[SerializeField]
		private AudioClip destroyedAudioClip;

		private Vector3 startPosition;
		private Quaternion startRotation;

		public event Action OnExploded;

		private void Awake()
		{
			var randomVelocity = new Vector2(Random.Range(minRandomVelocity.x, maxRandomVelocity.x), Random.Range(minRandomVelocity.y, maxRandomVelocity.y));
			rockRigidbody.AddForce(randomVelocity, ForceMode2D.Impulse);
			startPosition = transform.position;
			startRotation = transform.rotation;
		}

		public void ResetState()
		{
			gameObject.SetActive(true);
			transform.position = startPosition;
			transform.rotation = startRotation;
			rockRigidbody.velocity = Vector2.zero;
			rockRigidbody.angularVelocity = 0f;
			explosionParticleSystem.transform.SetParent(transform, true);
			explosionParticleSystem.transform.localPosition = Vector3.zero;
			var randomVelocity = new Vector2(Random.Range(minRandomVelocity.x, maxRandomVelocity.x), Random.Range(minRandomVelocity.y, maxRandomVelocity.y));
			rockRigidbody.AddForce(randomVelocity, ForceMode2D.Impulse);
		}

		public void Explode()
		{
			explosionParticleSystem.transform.SetParent(null);
			explosionParticleSystem.Play();
			StbBasicAudioController.Instance.PlaySound(destroyedAudioClip, transform.position);
			gameObject.SetActive(false);
			OnExploded?.Invoke();
		}
	}
}
