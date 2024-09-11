using System;
using UnityEngine;

namespace SaveToolbox.Runtime.Attributes
{
	/// <summary>
	/// An attribute to make a field readonly.
	/// </summary>
	[AttributeUsage(AttributeTargets.Field)]
	public class ReadOnlyAttribute : PropertyAttribute {}
}