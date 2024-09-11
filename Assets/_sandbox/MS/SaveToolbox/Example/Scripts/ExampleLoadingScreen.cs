using System;
using SaveToolbox.Runtime.Core;
using SaveToolbox.Runtime.Core.ScriptableObjects;
using UnityEngine;
using UnityEngine.UI;

namespace SaveToolbox.Example.Scripts
{
	/// <summary>
	/// An example implementation of a loading screen.
	/// </summary>
	[AddComponentMenu("SaveToolbox/Example/ExampleLoadingScreen")]
	public class ExampleLoadingScreen : MonoBehaviour
	{
		[SerializeField]
		private CanvasGroup canvasGroup;

		[SerializeField]
		private Text loadingText;

		[SerializeField]
		private ExampleLoadingBar exampleLoadingBar;

		private void Awake()
		{
			SaveToolboxSystem.Instance.OnLoadingStateChanged += HandleLoadingStateChanged;
			SaveToolboxSystem.Instance.OnSavingStateChanged += HandleSavingStateChanged;
			HandleLoadingStateChanged(SaveToolboxSystem.Instance.LoadingState);
		}

		private void HandleLoadingStateChanged(LoadingState loadingState)
		{
			// No need for a loading screen in non-asynchronous loading.
			if (!SaveToolboxPreferences.Instance.AsynchronousSaving) return;

			switch (loadingState.LoadState)
			{
				case LoadState.Failed:
				case LoadState.None:
					canvasGroup.alpha = 0f;
					canvasGroup.interactable = false;
					break;
				case LoadState.LoadingObjects:
					canvasGroup.alpha = 1f;
					canvasGroup.interactable = true;
					loadingText.text = "Loading Objects...";
					break;
				case LoadState.ApplyingData:
					canvasGroup.alpha = 1f;
					canvasGroup.interactable = true;
					loadingText.text = "Applying Data...";
					break;

				default:
					throw new ArgumentOutOfRangeException(nameof(loadingState), loadingState, null);
			}

			exampleLoadingBar.UpdateValue(loadingState.GetOverallProgression());
		}

		private void HandleSavingStateChanged(SavingState savingState)
		{
			// No need for a loading screen in non-asynchronous loading.
			if (!SaveToolboxPreferences.Instance.AsynchronousSaving) return;

			switch (savingState.SaveState)
			{
				case SaveState.Failed:
				case SaveState.None:
					canvasGroup.alpha = 0f;
					canvasGroup.interactable = false;
					break;
				case SaveState.SavingLoadables:
					canvasGroup.alpha = 1f;
					canvasGroup.interactable = true;
					loadingText.text = "Saving Loadables...";
					break;
				case SaveState.SavingNonLoadables:
					canvasGroup.alpha = 1f;
					canvasGroup.interactable = true;
					loadingText.text = "Saving Non-Loadables...";
					break;
				case SaveState.SavingNonMonoBehaviours:
					canvasGroup.alpha = 1f;
					canvasGroup.interactable = true;
					loadingText.text = "Saving Non-MonoBehaviours";
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			exampleLoadingBar.UpdateValue(savingState.GetOverallProgression());
		}

		private void OnDestroy()
		{
			SaveToolboxSystem.Instance.OnLoadingStateChanged -= HandleLoadingStateChanged;
			SaveToolboxSystem.Instance.OnSavingStateChanged -= HandleSavingStateChanged;
		}
	}
}
