using System;
using System.Collections.Generic;
using SaveToolbox.Runtime.Utils;
using UnityEditor;
using UnityEngine;

namespace SaveToolbox.Editor
{
	[CustomPropertyDrawer(typeof(SaveEntityReference<>))]
	public class SaveEntityReferencePropertyDrawer : PropertyDrawer
	{
		private bool hasInitialized;
		private SerializedProperty backingReferenceProperty;

		private void Initialize(SerializedProperty serializedProperty)
		{
			if (hasInitialized) return;

			backingReferenceProperty = serializedProperty.FindPropertyRelative("<EntityReference>k__BackingField").Copy();
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			Initialize(property);

			Type genericType;
			if (fieldInfo.FieldType.IsArray)
			{
				genericType = fieldInfo.FieldType.GetElementType()?.GetGenericArguments()[0];
			}
			else if (fieldInfo.FieldType.GetGenericTypeDefinition() == typeof(List<>))
			{
				genericType = fieldInfo.FieldType.GetGenericArguments()[0].GetGenericArguments()[0];
			}
			else
			{
				genericType = fieldInfo.FieldType.GetGenericArguments()[0];
			}

			EditorGUI.BeginChangeCheck();

			backingReferenceProperty.objectReferenceValue = EditorGUI.ObjectField(position, label, backingReferenceProperty.objectReferenceValue, genericType, true);

			var hasChanged = EditorGUI.EndChangeCheck();

			if (hasChanged)
			{
				EditorUtility.SetDirty(property.serializedObject.targetObject);
				property.serializedObject.ApplyModifiedProperties();
			}
		}
	}
}