using System.Collections.Generic;
using SaveToolbox.Runtime.CallbackHandlers;
using SaveToolbox.Runtime.Core;
using SaveToolbox.Runtime.Core.ScriptableObjects;
using UnityEngine;

namespace SaveToolbox.Example.Scripts
{
	/// <summary>
	/// An example of how the IStbSavedCallbackHandler and IStbLoadedCallbackHandler callbacks work for MonoBehaviours.
	/// It will provide a unity debug.log (as long as logging is enabled) to show it working. Also has context menu options to
	/// add a Non_MonoBehaviour callback instance.
	/// </summary>
	[AddComponentMenu("SaveToolbox/Example/ExampleMonoBehaviourCallback")]
	public class ExampleMonoBehaviourCallback : MonoBehaviour, IStbLoadedCallbackHandler, IStbSavedCallbackHandler
	{
		private readonly List<NonMonoBehaviourCallbackExample> nonMonoBehaviourCallbackExamples = new List<NonMonoBehaviourCallbackExample>();

		void IStbSavedCallbackHandler.HandleDataSaved(SaveData saveData)
		{
			if (SaveToolboxPreferences.Instance.LoggingEnabled) Debug.Log("MonoBehaviour saved callback received.");
		}

		void IStbLoadedCallbackHandler.HandleDataLoaded(SaveData saveData)
		{
			if (SaveToolboxPreferences.Instance.LoggingEnabled) Debug.Log("MonoBehaviour loaded callback received.");
		}

		[ContextMenu("AddNonMonoBehaviourExample")]
		private void AddNonMonoBehaviourExample()
		{
			var addedCallbackHandler = new NonMonoBehaviourCallbackExample();
			nonMonoBehaviourCallbackExamples.Add(addedCallbackHandler);
			addedCallbackHandler.HandleAdded();
		}

		[ContextMenu("RemoveNonMonoBehaviourExample")]
		private void RemoveNonMonoBehaviourExample()
		{
			if (nonMonoBehaviourCallbackExamples.Count == 0) return;

			var removedCallbackHandler = nonMonoBehaviourCallbackExamples[nonMonoBehaviourCallbackExamples.Count - 1];
			nonMonoBehaviourCallbackExamples.Remove(removedCallbackHandler);
			removedCallbackHandler.HandleRemoved();
		}
	}

	public class NonMonoBehaviourCallbackExample : IStbSavedCallbackHandler, IStbLoadedCallbackHandler
	{
		public void HandleAdded()
		{
			SaveToolboxSystem.Instance.StbCallbackDistributor.AddNonMonoBehaviourSaveCallbackHandler(this);
			SaveToolboxSystem.Instance.StbCallbackDistributor.AddNonMonoBehaviourLoadCallbackHandler(this);
		}

		void IStbSavedCallbackHandler.HandleDataSaved(SaveData saveData)
		{
			if (SaveToolboxPreferences.Instance.LoggingEnabled) Debug.Log("Non-MonoBehaviour Saved callback received.");
		}

		void IStbLoadedCallbackHandler.HandleDataLoaded(SaveData saveData)
		{
			if (SaveToolboxPreferences.Instance.LoggingEnabled) Debug.Log("Non-MonoBehaviour Loaded callback received.");
		}

		public void HandleRemoved()
		{
			SaveToolboxSystem.Instance.StbCallbackDistributor.RemoveNonMonoBehaviourSaveCallbackHandler(this);
			SaveToolboxSystem.Instance.StbCallbackDistributor.RemoveNonMonoBehaviourLoadCallbackHandler(this);
		}
	}
}