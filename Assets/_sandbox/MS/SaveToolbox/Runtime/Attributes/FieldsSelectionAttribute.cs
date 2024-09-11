using System;
using UnityEngine;

namespace SaveToolbox.Runtime.Attributes
{
	/// <summary>
	/// An attribute used by the StbComponentSaver to show a dropdown of any
	/// custom fields that can be saved.
	/// </summary>
	[AttributeUsage(AttributeTargets.Field)]
	public class FieldsSelectionAttribute : PropertyAttribute
	{
		public string ComponentIdentifier { get; private set; }

		public FieldsSelectionAttribute(string componentIdentifier)
		{
			ComponentIdentifier = componentIdentifier;
		}
	}
}
