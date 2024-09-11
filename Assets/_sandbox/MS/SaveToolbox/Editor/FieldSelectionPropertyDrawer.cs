using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaveToolbox.Runtime.Attributes;
using SaveToolbox.Runtime.BasicSaveableMonoBehaviours;
using SaveToolbox.Runtime.Serialization;
using UnityEditor;
using UnityEngine;

namespace SaveToolbox.Editor
{
	[CustomPropertyDrawer(typeof(FieldsSelectionAttribute))]
	public class FieldSelectionPropertyDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			if (!(attribute is FieldsSelectionAttribute fieldsSelectionAttribute)) return;

			var stbCustomComponent = (StbComponentSaver)property.serializedObject.targetObject;
			if (fieldInfo == null) return;
			if (!stbCustomComponent.TryGetComponentOfIdentifier(fieldsSelectionAttribute.ComponentIdentifier, out var customComponent)) return;

			if (customComponent == null)
			{
				Debug.LogError("Tried to get fields of a non-component.");
				return;
			}

			var fieldNames = new List<string>();
			var monoBehaviourType = customComponent.GetType();

			var monoBehaviourProperties = monoBehaviourType.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
			var monoBehaviourFields = monoBehaviourType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

			fieldNames.AddRange(GetPropertyNamesOfSuitableTypes(monoBehaviourProperties));
			fieldNames.AddRange(GetFieldNamesOfSuitableTypes(monoBehaviourFields));

			if (property.propertyType == SerializedPropertyType.String)
			{
				var name = property.name;
				var fieldNamesArray = fieldNames.ToArray();
				var indexOfCurrentSelected = Array.IndexOf(fieldNamesArray, property.stringValue);
				var indexOfNewSelectedOption = EditorGUI.Popup(position, name, indexOfCurrentSelected, fieldNamesArray);
				if (indexOfNewSelectedOption == -1) return;

				property.stringValue = fieldNames[indexOfNewSelectedOption];
				property.serializedObject.ApplyModifiedProperties();
			}
			else
			{
				EditorGUI.LabelField(position,"Please provide a string.");
			}
		}

		private List<string> GetFieldNamesOfSuitableTypes(FieldInfo[] fieldInfos)
		{
			return (from fieldInfo in fieldInfos where IsOfQualifyingType(fieldInfo.FieldType) select fieldInfo.Name).ToList();
		}

		private List<string> GetPropertyNamesOfSuitableTypes(PropertyInfo[] propertyInfos)
		{
			return (from propertyInfo in propertyInfos where IsOfQualifyingType(propertyInfo.PropertyType) select propertyInfo.Name).ToList();
		}

		private static bool IsOfQualifyingType(Type type)
		{
			type = GetCollectionType(type);

			return type == typeof(string) || type == typeof(bool) || type == typeof(int) || type == typeof(float) ||
			       type == typeof(Vector3) || type == typeof(Quaternion) || type == typeof(Color) || type == typeof(byte) ||
			       type == typeof(Enum) || type == typeof(double);
		}

		private static Type GetCollectionType(Type type)
		{
			if (type.IsArray) // Check if it is an array.
			{
				return type.GetElementType();
			}

			if (type.IsList()) // Check if it is a list.
			{
				return type.GenericTypeArguments[0];
			}

			return type;
		}
	}
}
