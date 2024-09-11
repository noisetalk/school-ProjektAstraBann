using UnityEngine;

namespace SaveToolbox.Runtime.Core.MonoBehaviours
{
	/// <summary>
	/// A class to save/load the game state on the button press of a custom button in the inspector.
	/// It can also be called from an external script as the API is public.
	/// </summary>
	public class SaveSystemController : MonoBehaviour
	{
		[field: SerializeField]
		public bool HasCustomSaveSettings { get; set; }

		[field: SerializeField]
		public SaveSettings CustomSaveSettings { get; set; }

		/// <summary>
		/// Saves the current game state.
		/// </summary>
		/// <param name="saveSlotIndex">The slot index of the game save data. defaults to -1, which is equivalent to no slot.</param>
		[ContextMenu("Try Save")]
		public void SaveGame(int saveSlotIndex = 0)
		{
#if STB_ASYNCHRONOUS_SAVING
#pragma warning disable CS4014
			SaveToolboxSystem.Instance.TrySaveGameAsync(saveSlotIndex);
#pragma warning restore CS4014
#else
			SaveToolboxSystem.Instance.TrySaveGame(saveSlotIndex);
#endif
		}

		/// <summary>
		/// Saves the current game state.
		/// </summary>
		/// <param name="saveSettings">The args in which to save the data with.</param>
		[ContextMenu("Try Save")]
		public void SaveGame(SaveSettings saveSettings, int slotIndex = 0)
		{
#if STB_ASYNCHRONOUS_SAVING
#pragma warning disable CS4014
			SaveToolboxSystem.Instance.TrySaveGameAsync(saveSettings, slotIndex);
#pragma warning restore CS4014
#else
			SaveToolboxSystem.Instance.TrySaveGame(saveSettings, slotIndex);
#endif
		}

		/// <summary>
		/// Loads the current game state at slot.
		/// </summary>
		/// <param name="saveSlotIndex">The slot index of the game save data. defaults to -1, which is equivalent to no slot.</param>
		[ContextMenu("Try Load")]
		public void Load(int saveSlotIndex = 0)
		{
#if STB_ASYNCHRONOUS_SAVING
#pragma warning disable CS4014
			SaveToolboxSystem.Instance.TryLoadGameAsync(saveSlotIndex);
#pragma warning restore CS4014
#else
			SaveToolboxSystem.Instance.TryLoadGame(saveSlotIndex);
#endif
		}

		[ContextMenu("Try Load")]
		public void Load(SaveSettings saveSettings, int slotIndex = 0)
		{
#if STB_ASYNCHRONOUS_SAVING
#pragma warning disable CS4014
			SaveToolboxSystem.Instance.TryLoadGameAsync(saveSettings, slotIndex);
#pragma warning restore CS4014
#else
			SaveToolboxSystem.Instance.TryLoadGame(saveSettings, slotIndex);
#endif
		}

		/// <summary>
		/// Deletes the game save at slot.
		/// </summary>
		/// <param name="saveSlotIndex">The slot index of the game save data. defaults to -1, which is equivalent to no slot.</param>
		[ContextMenu("Try Delete")]
		public void TryDelete(int saveSlotIndex = 0)
		{
			SaveToolboxSystem.Instance.TryDeleteSaveInSlot(saveSlotIndex);
		}

		/// <summary>
		/// Loads all the game meta datas at the persistent data path.
		/// </summary>
		[ContextMenu("Load all meta datas")]
		public void LoadAllMetaDatas()
		{
			SaveToolboxSystem.Instance.LoadAllSaveGameMetaDatas<SaveMetaData>();
		}
	}
}