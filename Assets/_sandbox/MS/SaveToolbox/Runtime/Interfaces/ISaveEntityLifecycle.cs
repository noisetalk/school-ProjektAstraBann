namespace SaveToolbox.Runtime.Interfaces
{
	/// <summary>
	/// An interface that can be implemented that holds basic lifecycle events of an ISaveDataEntity.
	/// </summary>
	public interface ISaveEntityLifecycle
	{
		/// <summary>
		/// Called when the saving of a game save is completed.
		/// </summary>
		void OnSaveCompleted();

		/// <summary>
		/// Called when a loadable objects has been instantiated.
		/// </summary>
		void OnLoadingSpawned();

		/// <summary>
		/// Called when the loading of a game save is completed.
		/// </summary>
		void OnLoadCompleted();
	}
}