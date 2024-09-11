namespace SaveToolbox.Runtime.CallbackHandlers
{
	public interface IStbBeforeSavedCallbackHandler
	{
		/// <summary>
		/// A callback just before the game state is attempted to be saved.
		/// Useful for behaviours that need functionality just before a save.
		/// </summary>
		void HandleBeforeSaved();
	}
}