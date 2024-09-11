using UnityEngine;
using UnityEngine.UI;

namespace SaveToolbox.Runtime.Core.MonoBehaviours.Ui
{
	[AddComponentMenu("SaveToolbox/UI/SaveGameDataButton")]
	public class UiSaveGameDataButton : MonoBehaviour
	{
		[SerializeField]
		private Button saveButton;

		[SerializeField]
		private int slotIndex;

		private void OnEnable()
		{
			saveButton.onClick.AddListener(SaveGame);
		}

		private void SaveGame()
		{
#if STB_ASYNCHRONOUS_SAVING
#pragma warning disable CS4014
			SaveToolboxSystem.Instance.TrySaveGameAsync(slotIndex);
#pragma warning restore CS4014
#else
			SaveToolboxSystem.Instance.TrySaveGame(slotIndex);
#endif
		}

		private void OnDisable()
		{
			saveButton.onClick.RemoveListener(SaveGame);
		}
	}
}