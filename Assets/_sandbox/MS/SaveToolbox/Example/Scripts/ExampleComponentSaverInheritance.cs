using System;
using SaveToolbox.Runtime.BasicSaveableMonoBehaviours;

namespace SaveToolbox.Example.Scripts
{
	/// <summary>
	/// An example of a class derived from the StbComponentSaver and overriding the IsTypeAccepted
	/// to change which fields that looked for in the field selector dropdown.
	/// </summary>
	public class ExampleComponentSaverInheritance : StbComponentSaver
	{
		protected override bool IsTypeAccepted(Type type)
		{
			// Here you would be able to assign your own rules for whether or not the type is accepted.
			// You would also have to ensure that you have implemented any explicit serialization in the save
			// system using available tools. Please ensure you also create a custom editor for this class
			// that inherits from the StbCustomComponentEditor that to make use of the custom functionality.
			return base.IsTypeAccepted(type);
		}
	}
}