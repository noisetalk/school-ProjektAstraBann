namespace SaveToolbox.Runtime.Core.ScriptableObjects
{
	/// <summary>
	/// Legacy class that exists because of a naming error in versions 1.0.0 and 1.0.1.
	/// Needs to remain inside the package to prevent issues when updating from the above versions,
	/// as deleting or renaming a class in unity packages system does not cause it to be automatically removed
	/// when a user imports the updated version.
	/// Feel free to delete this script from your project/not import it.
	/// </summary>
	internal sealed class LoadablePrefabDatabase {}
}
