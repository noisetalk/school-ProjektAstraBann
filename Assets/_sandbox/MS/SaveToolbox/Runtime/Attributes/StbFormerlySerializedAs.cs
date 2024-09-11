using System;

namespace SaveToolbox.Runtime.Attributes
{
	/// <summary>
	/// An attribute to mark any serialized data fields that the namespace has changed so
	/// that the field type can be easily found again.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true)]
	public class StbFormerlySerializedAs : Attribute
	{
		public readonly string formerNamespace;

		public StbFormerlySerializedAs(string value)
		{
			formerNamespace = value;
		}
	}
}
