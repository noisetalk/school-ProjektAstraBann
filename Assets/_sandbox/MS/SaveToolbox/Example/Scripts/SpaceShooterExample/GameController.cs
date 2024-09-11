using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace SaveToolbox.Example.Scripts.SpaceShooterExample
{
	public class GameController : MonoBehaviour
	{
		private const int ROCK_DESTROYED_SCORE = 100;
		private const float END_GAME_TIME = 2f;

		public int PlayerScore { get; private set; }

		[SerializeField]
		private SpaceShooterPlayerController spaceShooterPlayerController;

		[SerializeField]
		private SpaceRock[] spaceRocks;

		[SerializeField]
		private GameObject uiObject;

		[SerializeField]
		private Button playButton;

		[SerializeField]
		private float temporaryVelocityThreshold = 0.01f;

		private bool isEndingGame;
		private float currentEndGameTime;
		private float cachedVelocityThreshold;

		public event Action<int> OnPlayerScoreUpdated;

		private void Awake()
		{
			cachedVelocityThreshold = Physics2D.velocityThreshold;
			Physics2D.velocityThreshold = temporaryVelocityThreshold;
		}

		private void OnEnable()
		{
			playButton.onClick.AddListener(StartGame);
			spaceShooterPlayerController.OnDeath += StartEndingGame;
			foreach (var spaceRock in spaceRocks)
			{
				spaceRock.OnExploded += HandleRockDestroyed;
			}
		}

		private void StartGame()
		{
			isEndingGame = false;
			spaceShooterPlayerController.gameObject.SetActive(true);
			spaceShooterPlayerController.ResetState();

			foreach (var spaceRock in spaceRocks)
			{
				spaceRock.gameObject.SetActive(true);
				spaceRock.ResetState();
			}

			uiObject.gameObject.SetActive(false);

			PlayerScore = 0;
			OnPlayerScoreUpdated?.Invoke(PlayerScore);
		}

		private void HandleRockDestroyed()
		{
			PlayerScore += ROCK_DESTROYED_SCORE;
			OnPlayerScoreUpdated?.Invoke(PlayerScore);

			if (isEndingGame) return;

			if (spaceRocks.Any(spaceRock => spaceRock.isActiveAndEnabled)) return;

			foreach (var spaceRock in spaceRocks)
			{
				spaceRock.ResetState();
			}
		}

		private void StartEndingGame()
		{
			isEndingGame = true;
			currentEndGameTime = END_GAME_TIME;
		}

		private void Update()
		{
			if (isEndingGame)
			{
				currentEndGameTime -= Time.deltaTime;

				if (currentEndGameTime < 0)
				{
					// Cleanup projectiles.
					for (var index = SpaceShooterProjectile.allCurrentProjectiles.Count - 1; index >= 0; index--)
					{
						var spaceShooterProjectile = SpaceShooterProjectile.allCurrentProjectiles[index];
						Destroy(spaceShooterProjectile.gameObject);
					}

					spaceShooterPlayerController.gameObject.SetActive(false);
					foreach (var spaceRock in spaceRocks)
					{
						spaceRock.gameObject.SetActive(false);
					}

					uiObject.gameObject.SetActive(true);
					isEndingGame = false;
				}
			}
		}

		private void OnDisable()
		{
			spaceShooterPlayerController.OnDeath -= StartEndingGame;
			foreach (var spaceRock in spaceRocks)
			{
				spaceRock.OnExploded -= HandleRockDestroyed;
			}
		}

		private void OnDestroy()
		{
			Physics2D.velocityThreshold = cachedVelocityThreshold;
		}
	}
}
