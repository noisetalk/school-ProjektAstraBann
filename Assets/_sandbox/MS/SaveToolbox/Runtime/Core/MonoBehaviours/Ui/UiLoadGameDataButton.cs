using UnityEngine;
using UnityEngine.UI;

namespace SaveToolbox.Runtime.Core.MonoBehaviours.Ui
{
	[AddComponentMenu("SaveToolbox/UI/LoadGameDataButton")]
	public class UiLoadGameDataButton : MonoBehaviour
	{
		[SerializeField]
		private Button loadButton;

		[SerializeField]
		private int slotIndex;

		private void OnEnable()
		{
			loadButton.onClick.AddListener(LoadGame);
		}

		private void LoadGame()
		{
#if STB_ASYNCHRONOUS_SAVING
#pragma warning disable CS4014
			SaveToolboxSystem.Instance.TryLoadGameAsync(slotIndex);
#pragma warning restore CS4014
#else
			SaveToolboxSystem.Instance.TryLoadGame(slotIndex);
#endif
		}

		private void OnDisable()
		{
			loadButton.onClick.RemoveListener(LoadGame);
		}
	}
}