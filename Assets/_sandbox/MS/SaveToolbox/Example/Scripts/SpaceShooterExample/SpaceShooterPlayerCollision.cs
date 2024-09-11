using System;
using UnityEngine;

namespace SaveToolbox.Example.Scripts.SpaceShooterExample
{
	public class SpaceShooterPlayerCollision : MonoBehaviour
	{
		public event Action OnCollision;

		private void OnCollisionEnter2D(Collision2D other)
		{
			if (other.gameObject.TryGetComponent<SpaceRock>(out var spaceRock))
			{
				OnCollision?.Invoke();
				spaceRock.Explode();
			}
		}
	}
}
