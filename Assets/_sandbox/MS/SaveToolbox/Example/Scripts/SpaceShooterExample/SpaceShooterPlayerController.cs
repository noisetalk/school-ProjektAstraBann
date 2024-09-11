using System;
using SaveToolbox.Runtime.Core;
using SaveToolbox.Runtime.Core.ScriptableObjects;
using UnityEngine;

namespace SaveToolbox.Example.Scripts.SpaceShooterExample
{
	public class SpaceShooterPlayerController : MonoBehaviour
	{
		private const string SAVE_FOLDER_NAME = "ExampleMetaDataScene";

		[SerializeField]
		private SpaceShooterPlayerCollision spaceShooterPlayerCollision;

		[SerializeField]
		private Rigidbody2D playerRigidbody;

		[SerializeField]
		private float movementSpeed = 10f;

		[SerializeField]
		private float rotationSpeed = 10f;

		[SerializeField]
		private float shootCooldown = 0.5f;

		[SerializeField]
		private Transform projectileSpawnPoint;

		[SerializeField]
		private SpaceShooterProjectile spaceShooterProjectile;

		[SerializeField]
		private ParticleSystem explosionParticleSystem;

		[SerializeField]
		private AudioClip firedAudioClip;

		[SerializeField]
		private AudioClip destroyedAudioClip;

#if STB_HAS_INPUT_SYSTEM
		private ExampleSpaceShooterInput exampleSpaceShooterInput;
#endif

		private Vector3 startPosition;
		private Quaternion startRotation;

		private float horizontalAxis;
		private float verticalAxis;
		private float timeSinceLastShot = 999999f; // default to a high number to ensure we can shoot from start
		private bool shouldShoot;

		private bool CanShoot => shootCooldown - timeSinceLastShot < 0;

		public event Action OnDeath;

		private void Awake()
		{
			startPosition = transform.position;
			startRotation = transform.rotation;
#if STB_HAS_INPUT_SYSTEM
			exampleSpaceShooterInput = new ExampleSpaceShooterInput();
#endif
		}

		private void OnEnable()
		{
			spaceShooterPlayerCollision.OnCollision += HandleCollision;
#if STB_HAS_INPUT_SYSTEM
			exampleSpaceShooterInput.Game.Enable();
#endif
		}

		private void HandleCollision()
		{
			explosionParticleSystem.transform.SetParent(null);
			explosionParticleSystem.Play();
			StbBasicAudioController.Instance.PlaySound(destroyedAudioClip, transform.position);
			gameObject.SetActive(false);
			OnDeath?.Invoke();
		}

		private void Update()
		{
			GetInputs();

			if (shouldShoot && CanShoot)
			{
				Shoot();
			}

			timeSinceLastShot += Time.deltaTime;
		}

		private void FixedUpdate()
		{
			ApplyInputs();
		}

		private void GetInputs()
		{
#if STB_HAS_INPUT_SYSTEM
			verticalAxis = exampleSpaceShooterInput.Game.VerticalAxis.ReadValue<float>();
			horizontalAxis = exampleSpaceShooterInput.Game.HorizontalAxis.ReadValue<float>();
			shouldShoot = exampleSpaceShooterInput.Game.Shoot.triggered;

			if (exampleSpaceShooterInput.Game.Save.triggered)
			{
				var saveDataArgs = new FileSaveSettings(SaveToolboxPreferences.Instance);
				saveDataArgs.CustomSavePath = $"{Application.persistentDataPath}/{SAVE_FOLDER_NAME}";
				SaveToolboxSystem.Instance.TrySaveGame(saveDataArgs);
			}

			if (exampleSpaceShooterInput.Game.Load.triggered)
			{
				var saveDataArgs = new FileSaveSettings(SaveToolboxPreferences.Instance);
				saveDataArgs.CustomSavePath = $"{Application.persistentDataPath}/{SAVE_FOLDER_NAME}";
				SaveToolboxSystem.Instance.TryLoadGame(saveDataArgs);
			}
#else
			if (Input.GetKey(KeyCode.W))
			{
				verticalAxis = 1f;
			}
			else if (Input.GetKey(KeyCode.S))
			{
				verticalAxis = -1f;
			}
			else
			{
				verticalAxis = 0f;
			}

			if (Input.GetKey(KeyCode.A))
			{
				horizontalAxis = 1f;
			}
			else if (Input.GetKey(KeyCode.D))
			{
				horizontalAxis = -1f;
			}
			else
			{
				horizontalAxis = 0f;
			}

			shouldShoot = Input.GetKeyDown(KeyCode.Space);

			if (Input.GetKeyDown(KeyCode.Alpha1))
			{
				var saveSettings = SaveToolboxPreferences.Instance.DefaultSaveSettings.Copy();
				saveSettings.RelativeFolderPath = $"{SAVE_FOLDER_NAME}";
				SaveToolboxSystem.Instance.TrySaveGame(saveSettings);
			}

			if (Input.GetKeyDown(KeyCode.Alpha2))
			{
				var saveSettings = SaveToolboxPreferences.Instance.DefaultSaveSettings.Copy();
				saveSettings.RelativeFolderPath = $"{SAVE_FOLDER_NAME}";
				SaveToolboxSystem.Instance.TryLoadGame(saveSettings);
			}
#endif
		}

		private void ApplyInputs()
		{
			playerRigidbody.AddTorque(rotationSpeed * horizontalAxis * Time.fixedDeltaTime, ForceMode2D.Force);
			playerRigidbody.AddForce(movementSpeed * verticalAxis * Time.fixedDeltaTime * transform.up, ForceMode2D.Force);
		}

		private void Shoot()
		{
			var projectile = Instantiate(spaceShooterProjectile, projectileSpawnPoint.position, projectileSpawnPoint.rotation);
			projectile.Fire();
			StbBasicAudioController.Instance.PlaySound(firedAudioClip, transform.position);
			timeSinceLastShot = 0f;
		}

		public void ResetState()
		{
			gameObject.SetActive(true);
			explosionParticleSystem.transform.SetParent(transform);
			explosionParticleSystem.transform.position = transform.position;
			transform.position = startPosition;
			transform.rotation = startRotation;
			playerRigidbody.velocity = Vector2.zero;
			playerRigidbody.angularVelocity = 0f;
		}

		private void OnDisable()
		{
			spaceShooterPlayerCollision.OnCollision -= HandleCollision;
#if STB_HAS_INPUT_SYSTEM
			exampleSpaceShooterInput.Game.Disable();
#endif
		}
	}
}
