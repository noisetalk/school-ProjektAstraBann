using System;
using SaveToolbox.Runtime.Attributes;
using UnityEditor;
using UnityEngine;

namespace SaveToolbox.Editor
{
	[CustomPropertyDrawer(typeof(UniqueGuidAttribute))]
	public class UniqueGuidPropertyDrawer  : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			if (!(attribute is UniqueGuidAttribute _)) return;

			if (string.IsNullOrEmpty(property.stringValue))
			{
				property.stringValue = Guid.NewGuid().ToString();
			}

			EditorGUI.PropertyField(position, property, label, true);
		}
	}
}