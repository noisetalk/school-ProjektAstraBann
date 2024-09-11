namespace SaveToolbox.Runtime.Interfaces
{
	public interface ISaveEntityDeserializationPriority
	{
		/// <summary>
		/// A priority in which this ISaveDataEntity should be deserialized again. The higher the int value
		/// the sooner it will serialize in comparison to other ISaveDataEntities. Order is ambiguous
		/// for ISaveDataEntities of the same priority.
		/// </summary>
		int DeserializationPriority { get; set; }

	}
}