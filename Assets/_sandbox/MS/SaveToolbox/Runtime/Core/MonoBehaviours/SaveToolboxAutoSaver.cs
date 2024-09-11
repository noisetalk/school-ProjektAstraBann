using UnityEngine;

namespace SaveToolbox.Runtime.Core.MonoBehaviours
{
	/// <summary>
	/// A MonoBehaviour class to automatically save the game state on MonoBehaviour lifecycle events such as
	/// Awake, Start and OnDestroy etc.
	/// </summary>
	[AddComponentMenu("SaveToolbox/Core/SaveToolboxAutoSaver")]
	public class SaveToolboxAutoSaver : MonoBehaviour
	{
		private enum UnityLifecycleEvent
		{
			None,
			Awake,
			OnEnable,
			Start,
			OnDisable,
			OnDestroy
		}

		[SerializeField]
		private UnityLifecycleEvent saveEvent = UnityLifecycleEvent.OnDisable;

		[SerializeField]
		private UnityLifecycleEvent loadEvent = UnityLifecycleEvent.OnEnable;

		private void Awake()
		{
#pragma warning disable CS4014
#if STB_ASYNCHRONOUS_SAVING
			if (loadEvent == UnityLifecycleEvent.Awake) SaveToolboxSystem.Instance.TryLoadGameAsync(0);
			if (saveEvent == UnityLifecycleEvent.Awake) SaveToolboxSystem.Instance.TrySaveGameAsync(0);
#else
			if (loadEvent == UnityLifecycleEvent.Awake) SaveToolboxSystem.Instance.TryLoadGame(0);
			if (saveEvent == UnityLifecycleEvent.Awake) SaveToolboxSystem.Instance.TrySaveGame(0);
#endif
		}

		private void OnEnable()
		{
#if STB_ASYNCHRONOUS_SAVING
			if (loadEvent == UnityLifecycleEvent.OnEnable) SaveToolboxSystem.Instance.TryLoadGameAsync(0);
			if (saveEvent == UnityLifecycleEvent.OnEnable) SaveToolboxSystem.Instance.TrySaveGameAsync(0);
#else
			if (loadEvent == UnityLifecycleEvent.OnEnable) SaveToolboxSystem.Instance.TryLoadGame(0);
			if (saveEvent == UnityLifecycleEvent.OnEnable) SaveToolboxSystem.Instance.TrySaveGame(0);
#endif
		}

		private void Start()
		{
#if STB_ASYNCHRONOUS_SAVING
			if (loadEvent == UnityLifecycleEvent.Start) SaveToolboxSystem.Instance.TryLoadGameAsync(0);
			if (saveEvent == UnityLifecycleEvent.Start) SaveToolboxSystem.Instance.TrySaveGameAsync(0);
#else
			if (loadEvent == UnityLifecycleEvent.Start) SaveToolboxSystem.Instance.TryLoadGame(0);
			if (saveEvent == UnityLifecycleEvent.Start) SaveToolboxSystem.Instance.TrySaveGame(0);
#endif
		}

		private void OnDisable()
		{
#if STB_ASYNCHRONOUS_SAVING
			if (loadEvent == UnityLifecycleEvent.OnDisable) SaveToolboxSystem.Instance.TryLoadGameAsync(0);
			if (saveEvent == UnityLifecycleEvent.OnDisable) SaveToolboxSystem.Instance.TrySaveGameAsync(0);
#else
			if (loadEvent == UnityLifecycleEvent.OnDisable) SaveToolboxSystem.Instance.TryLoadGame(0);
			if (saveEvent == UnityLifecycleEvent.OnDisable) SaveToolboxSystem.Instance.TrySaveGame(0);
#endif
		}

		private void OnDestroy()
		{
#if STB_ASYNCHRONOUS_SAVING
			if (loadEvent == UnityLifecycleEvent.OnDestroy) SaveToolboxSystem.Instance.TryLoadGameAsync(0);
			if (saveEvent == UnityLifecycleEvent.OnDestroy) SaveToolboxSystem.Instance.TrySaveGameAsync(0);
#else
			if (loadEvent == UnityLifecycleEvent.OnDestroy) SaveToolboxSystem.Instance.TryLoadGame(0);
			if (saveEvent == UnityLifecycleEvent.OnDestroy) SaveToolboxSystem.Instance.TrySaveGame(0);
#endif
#pragma warning restore CS4014
		}
	}
}
