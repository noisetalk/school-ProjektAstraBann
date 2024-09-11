namespace SaveToolbox.Runtime.Interfaces
{
	public interface ILoadableObjectTarget
	{
		/// <summary>
		/// The loadable object Id. Used to determine which LoadableObject this ISaveDataEntity belongs to
		/// so that when deserialized it can be handled by the LoadableObject itself. If there is not LoadableObject
		/// this value should be set to -1.
		/// </summary>
		string LoadableObjectId { get; set; }

#if STB_ABOVE_2021_3
		/// <summary>
		/// returns whether or not this object is part of a LoadableObject.
		/// </summary>
		bool IsPartOfLoadableObject => !string.IsNullOrEmpty(LoadableObjectId);
#endif
	}
}