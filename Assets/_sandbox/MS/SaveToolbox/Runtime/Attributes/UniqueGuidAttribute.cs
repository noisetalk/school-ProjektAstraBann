using System;
using UnityEngine;

namespace SaveToolbox.Runtime.Attributes
{
	/// <summary>
	/// An attribute to give a string field a unique Guid value.
	/// </summary>
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
	public class UniqueGuidAttribute : PropertyAttribute {}
}