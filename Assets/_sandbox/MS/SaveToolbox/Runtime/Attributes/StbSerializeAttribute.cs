using System;
using UnityEngine;

namespace SaveToolbox.Runtime.Attributes
{
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
	public class StbSerialize : PropertyAttribute { }
}