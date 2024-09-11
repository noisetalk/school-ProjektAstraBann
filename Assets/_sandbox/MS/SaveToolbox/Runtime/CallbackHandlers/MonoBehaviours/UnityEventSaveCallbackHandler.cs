using SaveToolbox.Runtime.Core;
using UnityEngine;
using UnityEngine.Events;

namespace SaveToolbox.Runtime.CallbackHandlers.MonoBehaviours
{
	[AddComponentMenu("SaveToolbox/Callbacks/UnityEventSaveCallBackHandler")]
	public class UnityEventSaveCallbackHandler : MonoBehaviour,
		IStbBeforeSavedCallbackHandler,
		IStbSavedCallbackHandler,
		IStbBeforeLoadedCallbackHandler,
		IStbLoadedCallbackHandler
	{
		[SerializeField]
		private UnityEvent beforeSaved;

		[SerializeField]
		private UnityEvent afterSaved;

		[SerializeField]
		private UnityEvent beforeLoaded;

		[SerializeField]
		private UnityEvent afterLoaded;

		public void HandleBeforeDataLoaded()
		{
			beforeLoaded?.Invoke();
		}

		public void HandleBeforeSaved()
		{
			beforeSaved?.Invoke();
		}

		public void HandleDataSaved(SaveData slotSaveData)
		{
			afterSaved?.Invoke();
		}

		public void HandleDataLoaded(SaveData slotSaveData)
		{
			afterLoaded?.Invoke();
		}
	}
}
