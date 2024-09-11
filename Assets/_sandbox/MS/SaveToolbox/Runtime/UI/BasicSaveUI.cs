using SaveToolbox.Runtime.Core;
using SaveToolbox.Runtime.Core.ScriptableObjects;
using UnityEngine;
using UnityEngine.UI;

namespace SaveToolbox.Runtime.UI
{
	/// <summary>
	/// An example of save ui that holds reference to 2 unity UI buttons.
	/// One for saving and one for loading.
	/// </summary>
	[AddComponentMenu("SaveToolbox/Example/BasicSaveUI")]
	public class BasicSaveUI : MonoBehaviour
	{
		private const string SAVE_FOLDER_NAME = "MonoBehaviourExamples";

		[SerializeField]
		private string saveFolderName = SAVE_FOLDER_NAME;

		[SerializeField]
		private Button saveButton;

		[SerializeField]
		private Button loadButton;

		private void OnEnable()
		{
			if (saveButton != null)
			{
				saveButton.onClick.AddListener(HandleSaveClicked);
			}

			if (loadButton != null)
			{
				loadButton.onClick.AddListener(HandleLoadClicked);
			}
		}

		private void HandleSaveClicked()
		{
			var saveSettings = SaveToolboxPreferences.Instance.DefaultSaveSettings.Copy();
			saveSettings.RelativeFolderPath = $"{saveFolderName}";
#if STB_ASYNCHRONOUS_SAVING
#pragma warning disable CS4014
			SaveToolboxSystem.Instance.TrySaveGameAsync(saveSettings);
#pragma warning restore CS4014
#else
			SaveToolboxSystem.Instance.TrySaveGame(saveSettings);
#endif
		}

		private void HandleLoadClicked()
		{
			var saveSettings = SaveToolboxPreferences.Instance.DefaultSaveSettings.Copy();
			saveSettings.RelativeFolderPath = $"{saveFolderName}";
#if STB_ASYNCHRONOUS_SAVING
#pragma warning disable CS4014
			SaveToolboxSystem.Instance.TryLoadGameAsync(saveSettings);
#pragma warning restore CS4014
#else
			SaveToolboxSystem.Instance.TryLoadGame(saveSettings);
#endif
		}

		private void OnDisable()
		{
			if (saveButton != null)
			{
				saveButton.onClick.RemoveListener(HandleSaveClicked);
			}

			if (loadButton != null)
			{
				loadButton.onClick.RemoveListener(HandleLoadClicked);
			}
		}
	}
}