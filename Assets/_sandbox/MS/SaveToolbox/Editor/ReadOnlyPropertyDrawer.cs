using SaveToolbox.Runtime.Attributes;
using UnityEditor;
using UnityEngine;

namespace SaveToolbox.Editor
{
	[CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
	public class ReadOnlyPropertyDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			var previousState = GUI.enabled;
			GUI.enabled = false;

			EditorGUI.PropertyField(position, property, label, true);

			GUI.enabled = previousState;
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return EditorGUI.GetPropertyHeight(property, label, true);
		}
	}
}