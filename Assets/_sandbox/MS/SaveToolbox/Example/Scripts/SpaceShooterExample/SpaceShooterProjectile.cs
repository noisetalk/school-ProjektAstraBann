using System.Collections.Generic;
using UnityEngine;

namespace SaveToolbox.Example.Scripts.SpaceShooterExample
{
	public class SpaceShooterProjectile : MonoBehaviour
	{
		[SerializeField]
		private Rigidbody2D projectileRigidbody;

		[SerializeField]
		private float projectileSpeed = 10f;

		[SerializeField]
		private float lifetimeDuration = 5f;

		public static List<SpaceShooterProjectile> allCurrentProjectiles = new List<SpaceShooterProjectile>();

		private void Awake()
		{
			// Seems to lose reference when imported to lower editor versions, so set it here if it is null.
			if (projectileRigidbody == null)
			{
				projectileRigidbody = GetComponent<Rigidbody2D>();
			}

			allCurrentProjectiles.Add(this);
		}

		private void OnTriggerEnter2D(Collider2D other)
		{
			if (other.gameObject.TryGetComponent<SpaceRock>(out var spaceRock))
			{
				spaceRock.Explode();
				Destroy(gameObject);
			}
		}

		private void Update()
		{
			lifetimeDuration -= Time.deltaTime;
			if (lifetimeDuration < 0f)
			{
				Destroy(gameObject);
			}
		}

		public void Fire()
		{
			projectileRigidbody.AddForce(projectileSpeed * transform.up, ForceMode2D.Impulse);
		}

		private void OnDestroy()
		{
			allCurrentProjectiles.Remove(this);
		}
	}
}
