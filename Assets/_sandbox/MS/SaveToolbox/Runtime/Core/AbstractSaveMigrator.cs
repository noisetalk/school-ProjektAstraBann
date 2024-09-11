namespace SaveToolbox.Runtime.Core
{
	/// <summary>
	/// An abstract save migrator. This should be inherited from and have the abstract function to overriden to provide migration functionality.
	/// </summary>
	public abstract class AbstractSaveMigrator
	{
		/// <summary>
		/// Used to migrate old data to newer data system. It can use meta data for the game save data to determine if it should be migrated.
		/// Along with any structural difference in the data. Migration implementation is up to the user and can be defined in any inheriting classes.
		/// </summary>
		/// <param name="saveData">The data that may need to be migrated.</param>
		/// <returns>The migrated data.</returns>
		public abstract SaveData Migrate(SaveData saveData, SaveMetaData saveMetaData);
	}
}