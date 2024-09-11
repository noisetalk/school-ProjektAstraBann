using SaveToolbox.Runtime.CallbackHandlers;
using SaveToolbox.Runtime.Core;
using UnityEngine;
using UnityEngine.UI;

namespace SaveToolbox.Example.Scripts.SpaceShooterExample
{
	public class InGameUI : MonoBehaviour, IStbSavedCallbackHandler, IStbLoadedCallbackHandler
	{
		private const float CALLBACK_DISPLAY_TIME = 2f;
		private const string SAVE_CALLBACK_TEXT = "SAVED.";
		private const string LOAD_CALLBACK_TEXT = "LOADED.";

		[SerializeField]
		private GameController gameController;

		[SerializeField]
		private Text scoreTextField;

		[SerializeField]
		private Text saveCallbackTextField;

		private bool isShowingCallbackText;
		private float currentCallbackDisplayTime;

		private void Awake()
		{
			gameController.OnPlayerScoreUpdated += HandleScoreUpdated;
		}

		private void HandleScoreUpdated(int score)
		{
			scoreTextField.text = score.ToString();
		}

		private void Update()
		{
			if (!isShowingCallbackText) return;

			currentCallbackDisplayTime -= Time.deltaTime;
			if (currentCallbackDisplayTime < 0f)
			{
				saveCallbackTextField.gameObject.SetActive(false);
				isShowingCallbackText = false;
			}
		}

		private void ShowSaveCallback(string text)
		{
			saveCallbackTextField.text = text;
			currentCallbackDisplayTime = CALLBACK_DISPLAY_TIME;
			isShowingCallbackText = true;
			saveCallbackTextField.gameObject.SetActive(true);
		}

		void IStbSavedCallbackHandler.HandleDataSaved(SaveData saveData)
		{
			ShowSaveCallback(SAVE_CALLBACK_TEXT);
			HandleScoreUpdated(gameController.PlayerScore);
		}

		void IStbLoadedCallbackHandler.HandleDataLoaded(SaveData saveData)
		{
			ShowSaveCallback(LOAD_CALLBACK_TEXT);
			HandleScoreUpdated(gameController.PlayerScore);
		}

		private void OnDestroy()
		{
			gameController.OnPlayerScoreUpdated -= HandleScoreUpdated;
		}
	}
}