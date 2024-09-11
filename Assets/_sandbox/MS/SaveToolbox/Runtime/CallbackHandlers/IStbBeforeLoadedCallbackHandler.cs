namespace SaveToolbox.Runtime.CallbackHandlers
{
	public interface IStbBeforeLoadedCallbackHandler
	{
		/// <summary>
		/// A callback just before the game state is attempted to be loaded.
		/// Useful for behaviours that need functionality just before a load.
		/// </summary>
		void HandleBeforeDataLoaded();
	}
}