using SaveToolbox.Runtime.Core;

namespace SaveToolbox.Runtime.CallbackHandlers
{
	/// <summary>
	/// An interface that has a callback to notify when save game data has been saved.
	/// </summary>
	public interface IStbSavedCallbackHandler
	{
		/// <summary>
		/// The data saved callback.
		/// </summary>
		/// <param name="slotSaveData">The data object that was saved.</param>
		void HandleDataSaved(SaveData slotSaveData);
	}
}