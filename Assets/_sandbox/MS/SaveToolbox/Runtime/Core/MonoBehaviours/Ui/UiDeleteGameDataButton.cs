using UnityEngine;
using UnityEngine.UI;

namespace SaveToolbox.Runtime.Core.MonoBehaviours.Ui
{
	[AddComponentMenu("SaveToolbox/UI/DeleteGameDataButton")]
	public class UiDeleteGameDataButton : MonoBehaviour
	{
		[SerializeField]
		private Button deleteButton;

		[SerializeField]
		private int slotIndex;

		private void OnEnable()
		{
			deleteButton.onClick.AddListener(DeleteSave);
		}

		private void DeleteSave()
		{
			SaveToolboxSystem.Instance.TryDeleteSaveInSlot(slotIndex);
		}

		private void OnDisable()
		{
			deleteButton.onClick.RemoveListener(DeleteSave);
		}
	}
}