using SaveToolbox.Runtime.Core;

namespace SaveToolbox.Runtime.CallbackHandlers
{
	/// <summary>
	/// An interface that has a callback to notify when save game data has been loaded.
	/// </summary>
	public interface IStbLoadedCallbackHandler
	{
		/// <summary>
		/// A save game data loaded callback.
		/// </summary>
		/// <param name="slotSaveData">The data object that was loaded.</param>
		void HandleDataLoaded(SaveData slotSaveData);
	}
}
