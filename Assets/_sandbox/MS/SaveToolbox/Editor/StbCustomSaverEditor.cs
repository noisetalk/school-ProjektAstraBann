#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using SaveToolbox.Runtime.BasicSaveableMonoBehaviours;
using SaveToolbox.Runtime.CustomComponentSavers;
using SaveToolbox.Runtime.Utils;
using UnityEditor;
using UnityEngine;

namespace SaveToolbox.Editor
{
	[CustomEditor(typeof(StbComponentSaver), true)]
	public class StbComponentSaverEditor : UnityEditor.Editor
	{
		private const string REGEX_EXPRESSION = "<([^>]+)>k__BackingField";

		private int componentHoldersCount;
		private readonly Dictionary<string, bool> foldoutDictionary = new Dictionary<string, bool>();
		private readonly Dictionary<string, Array> fieldsToSaveDictionary = new Dictionary<string, Array>();
		private readonly Dictionary<string, List<string>> componentMemberNamesDictionary = new Dictionary<string, List<string>>();
		private readonly Dictionary<string, List<(string, Type)>> customSaversForTypeDictionary = new Dictionary<string, List<(string, Type)>>();
		private readonly Dictionary<object, List<MemberInfo>> customSaversMemberInfos = new Dictionary<object, List<MemberInfo>>();

		private List<Type> allCustomComponentSaverTypes = new List<Type>();

		private GUIStyle foldoutStyle;
		private GUIStyle fieldsToSaveStyle;
		private GUIStyle fieldsBoxStyle;
		private GUIStyle removeComponentButtonStyle;
		private GUIStyle standardFieldsButtonStyle;
		private GUIStyle standardComponentSaverButtonStyle;
		private int currentSelectionIndex;

		private Type elementType;
		private StbComponentSaver stbComponentSaverInstance;
		private PropertyInfo mainIdentifierPropertyInfo;
		private PropertyInfo customComponentSaverIdentifierPropertyInfo;
		private FieldInfo identifierInfo;
		private FieldInfo deserializationPriorityInfo;
		private FieldInfo componentFieldInfo;
		private FieldInfo fieldsToSaveInfo;
		private FieldInfo customComponentSaversFieldInfo;
		private FieldInfo customComponentHoldersFieldInfo;
		private object customComponentHoldersFieldValue;

		private void OnEnable()
		{
			allCustomComponentSaverTypes = AppDomain.CurrentDomain.GetAssemblies()
				.SelectMany(a => a.GetTypes())
				.Where(type => type.BaseType != null && type.BaseType.IsGenericType && type.BaseType.GetGenericTypeDefinition() == typeof(AbstractComponentSaver<>)).ToList();

			elementType = typeof(CustomComponentHolder);
			stbComponentSaverInstance = (StbComponentSaver)serializedObject.targetObject;
			mainIdentifierPropertyInfo = stbComponentSaverInstance.GetType().GetProperty("SaveIdentifier", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
			identifierInfo = elementType.GetField("identifier", BindingFlags.Instance | BindingFlags.NonPublic);
			deserializationPriorityInfo = elementType.GetField("deserializationPriority", BindingFlags.Instance | BindingFlags.NonPublic);
			componentFieldInfo = elementType.GetField("component", BindingFlags.Instance | BindingFlags.NonPublic);
			fieldsToSaveInfo = elementType.GetField("fieldsToSave", BindingFlags.Instance | BindingFlags.NonPublic);
			customComponentHoldersFieldInfo = typeof(StbComponentSaver).GetField("customComponentHolders");
			customComponentHoldersFieldValue = customComponentHoldersFieldInfo.GetValue(serializedObject.targetObject);
			customComponentSaversFieldInfo = elementType.GetField("customComponentSavers", BindingFlags.Instance | BindingFlags.NonPublic);
		}

		private void TryCreateStyles()
		{
			if (foldoutStyle == null)
			{
				foldoutStyle = new GUIStyle
				{
					normal =
					{
						background = CreateTexture2D(1, 1, new Color(0.25f, 0.25f, 0.25f, 1f))
					}
				};
			}

			if (fieldsToSaveStyle == null)
			{
				fieldsToSaveStyle = new GUIStyle(GUI.skin.label)
				{
					fontSize = 14,
					fontStyle = FontStyle.Bold,
					padding = new RectOffset(0, 5, 0, 0)
				};
			}

			if (fieldsBoxStyle == null)
			{
				fieldsBoxStyle = new GUIStyle(GUI.skin.box)
				{
					padding = new RectOffset(5, 5, 5, 5),
					margin = new RectOffset(5, 5, 5, 5)
				};
			}

			if (removeComponentButtonStyle == null)
			{
				removeComponentButtonStyle = new GUIStyle(GUI.skin.button)
				{
					fontStyle = FontStyle.Bold,
					fontSize = 11,
					fixedWidth = 140f,
					normal =
					{
						background = CreateTexture2D(1, 1, new Color(0.5f, 0.1f, 0.1f, 1f))
					}
				};
			}

			if (standardFieldsButtonStyle == null)
			{
				standardFieldsButtonStyle = new GUIStyle(GUI.skin.button)
				{
					fontStyle = FontStyle.Bold,
					fontSize = 11,
					fixedWidth = 120
				};
			}

			if (standardComponentSaverButtonStyle == null)
			{
				standardComponentSaverButtonStyle = new GUIStyle(GUI.skin.button)
				{
					fontStyle = FontStyle.Bold,
					fontSize = 11,
					fixedWidth = 140
				};
			}
		}

		public override void OnInspectorGUI()
		{
			TryCreateStyles();
			serializedObject.Update();

			// Makes the identifier readonly.
			var previousState = GUI.enabled;
			GUI.enabled = false;
			EditorGUILayout.TextField("Save Identifier: ", (string)mainIdentifierPropertyInfo?.GetValue(serializedObject.targetObject));
			GUI.enabled = previousState;

			// Return if there is no custom component holders field.
			if (customComponentHoldersFieldValue == null) return;

			// Cache it to an array.
			var customComponentHoldersArray = (Array)customComponentHoldersFieldValue;
			if (customComponentHoldersFieldInfo.FieldType.IsArray && customComponentHoldersArray.Length != componentHoldersCount)
			{
				stbComponentSaverInstance.ValidateComponentHolders();
			}

			// Iterate through the array and draw each one correctly.
			for (var i = 0; i < customComponentHoldersArray.Length; i++)
			{
				var element = customComponentHoldersArray.GetValue(i);
				var customComponentHolder = (CustomComponentHolder)element;

				// Gather all necessary fields/properties.
				var identifierValue = identifierInfo.GetValue(element);
				var deserializationPriorityValue = deserializationPriorityInfo.GetValue(element);
				var componentFieldValue = componentFieldInfo.GetValue(element);
				var customComponentSaversValue = customComponentSaversFieldInfo.GetValue(element);

				EditorGUILayout.BeginHorizontal();
				EditorGUI.BeginChangeCheck();
				var previousType = componentFieldValue?.GetType();
				var selectedComponent = EditorGUILayout.ObjectField("Component: ", (Component)componentFieldValue, typeof(Component), true);
				var hasComponentChanged = EditorGUI.EndChangeCheck() && previousType != selectedComponent.GetType();

				// If the component changed and it is a component of a different type, set it's new value and remove all custom savers.
				if (hasComponentChanged)
				{
					customComponentSaversFieldInfo.SetValue(element, Array.Empty<object>());
					componentFieldInfo.SetValue(element, selectedComponent);
				}

				if (GUILayout.Button("Remove Component", removeComponentButtonStyle))
				{
					RemoveElementAtIndex<CustomComponentHolder>(serializedObject.targetObject, customComponentHoldersFieldInfo, i);
					customComponentHoldersFieldValue = customComponentHoldersFieldInfo.GetValue(serializedObject.targetObject);
					EditorGUILayout.EndFoldoutHeaderGroup();
					EditorPrefs.DeleteKey(identifierValue.ToString());
					i--;
					continue;
				}

				EditorGUILayout.EndHorizontal();

				// If there is no component currently set, continue to the next component holder as nothing needs to be drawn.
				if (customComponentHolder.Component == null) continue;

				var targetComponentType = customComponentHolder.Component != null ? customComponentHolder.Component.GetType() : null;

				// Get the identifier for the component.
				var componentIdentifierString = (string)identifierValue;

				if (!fieldsToSaveDictionary.ContainsKey(componentIdentifierString))
				{
					fieldsToSaveDictionary.Add(componentIdentifierString, (Array)fieldsToSaveInfo.GetValue(element));
				}

				if (!foldoutDictionary.ContainsKey(componentIdentifierString))
				{
					foldoutDictionary.Add(componentIdentifierString, EditorPrefs.GetBool(componentIdentifierString));
				}

				foldoutDictionary[componentIdentifierString] = EditorGUILayout.BeginFoldoutHeaderGroup(foldoutDictionary[componentIdentifierString], targetComponentType?.Name);
				EditorPrefs.SetBool(componentIdentifierString, foldoutDictionary[componentIdentifierString]);

				// If the component holder foldout is closed, stop drawing here.
				if (!foldoutDictionary[componentIdentifierString])
				{
					EditorGUILayout.EndFoldoutHeaderGroup();
					continue;
				}

				EditorGUILayout.BeginVertical(foldoutStyle);

				var hasComponentMemberNamesEntry = componentMemberNamesDictionary.ContainsKey(componentIdentifierString);
				if (!hasComponentMemberNamesEntry)
				{
					componentMemberNamesDictionary.Add(componentIdentifierString, new List<string>());
				}

				// If we didn't have any field names for the target component OR the target component has changed.
				if (!hasComponentMemberNamesEntry || hasComponentChanged)
				{
					componentMemberNamesDictionary[componentIdentifierString].Clear();
					if (targetComponentType != null)
					{
						var memberNames = stbComponentSaverInstance.GetAllSerializableFieldInfos(targetComponentType).Select(targetMemberInfo => targetMemberInfo.Name).ToArray();
						for (var index = 0; index < memberNames.Length; index++)
						{
							var memberName = memberNames[index];
							var match = Regex.Match(memberName, REGEX_EXPRESSION);
							if (match.Success)
							{
								memberNames[index] = match.Groups[1].Value;
							}
						}

						componentMemberNamesDictionary[componentIdentifierString].AddRange(memberNames);
					}
				}

				var hasCustomSaversForTypeEntry = customSaversForTypeDictionary.ContainsKey(componentIdentifierString);
				if (!hasCustomSaversForTypeEntry)
				{
					customSaversForTypeDictionary.Add(componentIdentifierString, new List<(string, Type)>());
				}

				if (!hasCustomSaversForTypeEntry || hasComponentChanged)
				{
					customSaversForTypeDictionary[componentIdentifierString].Clear();
					var customSaversForType = GetAllCustomSaversForType(targetComponentType);
					foreach (var customType in customSaversForType)
					{
						customSaversForTypeDictionary[componentIdentifierString].Add((customType.Name, customType));
					}
				}

				EditorGUILayout.BeginHorizontal();

				// Makes the identifier readonly.
				previousState = GUI.enabled;
				GUI.enabled = false;
				EditorGUILayout.TextField("Component identifier", (string)identifierValue);
				GUI.enabled = previousState;

				EditorGUILayout.EndHorizontal();

				deserializationPriorityValue = EditorGUILayout.IntField("Deserialization Priority", (int)deserializationPriorityValue);
				deserializationPriorityInfo.SetValue(element, deserializationPriorityValue);

				EditorGUILayout.BeginVertical(fieldsBoxStyle);

				// The header for the fields to be saved.
				EditorGUILayout.LabelField("Fields To Save", fieldsToSaveStyle);

				// If the type is of the same class type this component targets, stop drawing. This is to prevent recursive issues.
				if (targetComponentType == typeof(StbComponentSaver))
				{
					EditorGUILayout.LabelField("You cannot serialize a Stb Custom Component.");
					EditorGUILayout.EndFoldoutHeaderGroup();
					EditorGUILayout.EndVertical();
					continue;
				}

				if (fieldsToSaveDictionary[componentIdentifierString].Length == 0)
				{
					EditorGUILayout.LabelField("No fields currently selected. Add a field to save data.");
				}

				// Draw the fields that should be saved. Also apply any changes that are made to them.
				for (var targetFieldIndex = 0; targetFieldIndex < fieldsToSaveDictionary[componentIdentifierString].Length; targetFieldIndex++)
				{
					// Get the array element at the index.
					var currentFieldOptionValue = fieldsToSaveDictionary[componentIdentifierString].GetValue(targetFieldIndex);
					var fieldNamesArray = componentMemberNamesDictionary[componentIdentifierString].ToArray();
					var indexOfCurrentSelected = Array.IndexOf(fieldNamesArray, (string)currentFieldOptionValue);

					EditorGUILayout.BeginHorizontal();

					var indexOfNewSelectedOption = EditorGUILayout.Popup($"{targetComponentType.Name} field {targetFieldIndex + 1}:", indexOfCurrentSelected, fieldNamesArray);

					// Draw the remove field button.
					if (GUILayout.Button("Remove Field", standardFieldsButtonStyle, GUILayout.Width(120f)))
					{
						RemoveElementAtIndex<string>(element, fieldsToSaveInfo, targetFieldIndex);
						fieldsToSaveDictionary[componentIdentifierString] = (Array)fieldsToSaveInfo.GetValue(element);
						EditorGUILayout.EndHorizontal();
						// Set dirty to ensure saving.
						EditorUtility.SetDirty(target);
						break;
					}

					EditorGUILayout.EndHorizontal();
					if (indexOfNewSelectedOption == -1) continue;

					fieldsToSaveDictionary[componentIdentifierString].SetValue(fieldNamesArray[indexOfNewSelectedOption], targetFieldIndex);
				}

				// Draw the add field button.
				EditorGUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				if (GUILayout.Button("Add Field", standardFieldsButtonStyle))
				{
					InsertElementAtIndex(element, fieldsToSaveInfo, string.Empty, fieldsToSaveDictionary[componentIdentifierString].Length);
					fieldsToSaveDictionary[componentIdentifierString] = (Array)fieldsToSaveInfo.GetValue(element);
					// Set dirty to ensure saving.
					EditorUtility.SetDirty(target);
				}
				EditorGUILayout.EndHorizontal();
				EditorGUILayout.EndFoldoutHeaderGroup();
				EditorGUILayout.EndVertical();

				DrawCustomSavers();

				void DrawCustomSavers()
				{
					if (componentFieldValue != null && customSaversForTypeDictionary[componentIdentifierString].Count > 0)
					{
						// Create an array from the dictionary's tuple types.
						var saverNamesArray = customSaversForTypeDictionary[componentIdentifierString].Select(tuple => tuple.Item1).ToArray();
						var customSaversArray = (Array)customComponentSaversValue;

						// Add a bit of space to separe the custom savers for the fields to save.
						EditorGUILayout.Space(15);

						EditorGUILayout.BeginHorizontal();

						// Selection box for choosing a custom saver.
						currentSelectionIndex = EditorGUILayout.Popup("Use A Custom Component Saver?", currentSelectionIndex, saverNamesArray);

						string componentSaverIdentifierValue;

						if (GUILayout.Button("Add Custom Saver", standardComponentSaverButtonStyle))
						{
							// Create an instance of the new custom saver.
							var newInstance = Activator.CreateInstance(customSaversForTypeDictionary[componentIdentifierString][currentSelectionIndex].Item2);

							// Reflection is ok here as it will only be done once it is created.
							customComponentSaverIdentifierPropertyInfo = newInstance.GetType().GetProperty("SaveIdentifier", BindingFlags.Instance | BindingFlags.Public);
							if (customComponentSaverIdentifierPropertyInfo != null)
							{
								componentSaverIdentifierValue = (string)customComponentSaverIdentifierPropertyInfo.GetValue(newInstance);
								EditorPrefs.SetBool(componentSaverIdentifierValue, true);
								// Using editor prefs cache whether or not the foldout is open.
								InsertElementAtIndex(element, customComponentSaversFieldInfo, newInstance, customSaversArray.Length);
							}
							// Set dirty to ensure saving.
							EditorUtility.SetDirty(target);
						}
						EditorGUILayout.EndHorizontal();

						for (var j = 0; j < customSaversArray.Length; j++)
						{
							DrawCustomSaver(ref j);
						}

						void DrawCustomSaver(ref int saverIndex)
						{
							// Get the current custom saver and cache it's base type.
							var customSaverElement = customSaversArray.GetValue(saverIndex);
							var baseType = customSaverElement.GetType().BaseType;

							// Ensure the custom saver is of the correct type.
							if (baseType != null && baseType.GetGenericTypeDefinition() == typeof(AbstractComponentSaver<>))
							{
								// Increase the indent level.
								EditorGUI.indentLevel++;

								EditorGUILayout.BeginHorizontal();

								customComponentSaverIdentifierPropertyInfo = customSaverElement.GetType().GetProperty("SaveIdentifier", BindingFlags.Instance | BindingFlags.Public);
								componentSaverIdentifierValue = (string)customComponentSaverIdentifierPropertyInfo.GetValue(customSaverElement);
								if (!foldoutDictionary.ContainsKey(componentSaverIdentifierValue))
								{
									foldoutDictionary.Add(componentSaverIdentifierValue, EditorPrefs.GetBool(componentSaverIdentifierValue));
								}

								foldoutDictionary[componentSaverIdentifierValue] = EditorGUILayout.Foldout(foldoutDictionary[componentSaverIdentifierValue], ObjectNames.NicifyVariableName(customSaverElement.GetType().Name), true);
								EditorPrefs.SetBool(componentSaverIdentifierValue, foldoutDictionary[componentSaverIdentifierValue]);

								// Button for deleting the custom saver.
								var wasRemovePressed = GUILayout.Button("Remove Custom Saver", standardComponentSaverButtonStyle);

								EditorGUILayout.EndHorizontal();

								if (wasRemovePressed)
								{
									customSaversMemberInfos.Remove(componentSaverIdentifierValue);
									RemoveElementAtIndex<object>(element, customComponentSaversFieldInfo, saverIndex);
									EditorPrefs.DeleteKey(componentSaverIdentifierValue);
									--saverIndex;
									// Set dirty to ensure saving.
									EditorUtility.SetDirty(target);
								}
								else if (foldoutDictionary[componentSaverIdentifierValue])
								{
									if (!customSaversMemberInfos.ContainsKey(componentSaverIdentifierValue))
									{
										customSaversMemberInfos.Add(componentSaverIdentifierValue, new List<MemberInfo>());
										customSaversMemberInfos[componentSaverIdentifierValue].AddRange(StbUtilities.GetAllSerializableFieldsAndProperties(customSaverElement.GetType(), true, false));
									}

									var tempList = new List<MemberInfo>(customSaversMemberInfos[componentSaverIdentifierValue]);

									// Handle the Target in a custom way as we don't want it to be editable.
									for (var index = customSaversMemberInfos[componentSaverIdentifierValue].Count - 1; index >= 0; index--)
									{
										var memberInfo = customSaversMemberInfos[componentSaverIdentifierValue][index];
										switch (memberInfo.Name)
										{
											case "Target":
											{
												EditorGUI.BeginDisabledGroup(true);
												memberInfo.SetMemberInfoValue(customSaverElement, componentFieldValue);
												var value = memberInfo.GetMemberInfoValue(customSaverElement) as UnityEngine.Object;
												EditorGUILayout.ObjectField(ObjectNames.NicifyVariableName(memberInfo.Name), value, memberInfo.GetMemberInfoType(), true);
												tempList.RemoveAt(index);
												EditorGUI.EndDisabledGroup();
												break;
											}
											case "SaveIdentifier":
											{
												EditorGUI.BeginDisabledGroup(true);
												memberInfo.SetMemberInfoValue(customSaverElement, componentSaverIdentifierValue);
												var value = memberInfo.GetMemberInfoValue(customSaverElement) as string;
												EditorGUILayout.TextField(ObjectNames.NicifyVariableName(memberInfo.Name), value);
												tempList.RemoveAt(index);
												EditorGUI.EndDisabledGroup();
												break;
											}
										}
									}

									StbUtilities.DrawMemberInfos(customSaverElement, tempList);
								}
								EditorGUILayout.EndFoldoutHeaderGroup();
								EditorGUI.indentLevel--;
							}
						}
					}
					EditorGUILayout.EndVertical();
					EditorGUILayout.EndFoldoutHeaderGroup();
				}

				EditorGUILayout.Space(20f);
			}

			componentHoldersCount = customComponentHoldersArray.Length;

			if (GUILayout.Button("Add A New Component", GUILayout.Width(160f)))
			{
				var newCustomComponentHolder = new CustomComponentHolder();
				InsertElementAtIndex(serializedObject.targetObject, customComponentHoldersFieldInfo, newCustomComponentHolder, customComponentHoldersArray.Length);
				customComponentHoldersFieldValue = customComponentHoldersFieldInfo.GetValue(serializedObject.targetObject);
				EditorPrefs.SetBool(newCustomComponentHolder.Identifier, true);
			}
		}

		private void RemoveElementAtIndex<T>(object obj, FieldInfo fieldInfo, int targetIndex)
		{
			if (fieldInfo != null)
			{
				var fieldValue = fieldInfo.GetValue(obj);
				if (fieldValue != null && fieldValue.GetType().IsArray)
				{
					var currentArray = (Array)fieldValue;
					Array newArray = new T[currentArray.Length - 1];

					var newArrayIndex = 0;
					for (var index = 0; index < currentArray.Length; index++)
					{
						if (index != targetIndex)
						{
							newArray.SetValue(currentArray.GetValue(index), newArrayIndex++);
						}
					}

					fieldInfo.SetValue(obj, newArray);
				}
			}
		}

		private void InsertElementAtIndex<T>(object targetObject, FieldInfo fieldInfo, T element, int targetIndex)
		{
			if (fieldInfo != null)
			{
				var fieldValue = fieldInfo.GetValue(targetObject);
				if (fieldValue != null && fieldValue.GetType().IsArray)
				{
					var existingArray = (Array)fieldValue;
					Array newArray = new T[existingArray.Length + 1];

					for (var index = 0; index < newArray.Length; index++)
					{
						if (index < targetIndex)
						{
							newArray.SetValue(existingArray.GetValue(index), index);
						}
						else if (index == targetIndex)
						{
							newArray.SetValue(element, index);
						}
						else
						{
							newArray.SetValue(existingArray.GetValue(index - 1), index);
						}
					}
					fieldInfo.SetValue(targetObject, newArray);
				}
			}
		}

		private List<Type> GetAllCustomSaversForType(Type type)
		{
			var returnValue = new List<Type>();
			foreach (var allCustomComponentSaverType in allCustomComponentSaverTypes)
			{
				if (allCustomComponentSaverType == null || allCustomComponentSaverType.BaseType == null) continue;

				var genericArguments = allCustomComponentSaverType.BaseType.GetGenericArguments();
				if (genericArguments.Length > 0 && genericArguments[0] == type)
				{
					returnValue.Add(allCustomComponentSaverType);
				}
			}

			return returnValue;
		}

		private bool TypeHasCustomComponentSaver(Type testType, out Type newType)
		{
			newType = null;
			foreach (var allCustomComponentSaverType in allCustomComponentSaverTypes)
			{
				if (allCustomComponentSaverType == null || allCustomComponentSaverType.BaseType == null) continue;

				var genericArguments = allCustomComponentSaverType.BaseType.GetGenericArguments();
				if (genericArguments.Length > 0 && genericArguments[0] == testType)
				{
					newType = allCustomComponentSaverType;
					return true;
				}
			}

			return false;
		}

		private Texture2D CreateTexture2D(int width, int height, Color color)
		{
			var pixels = new Color[width * height];
			for (var i = 0; i < pixels.Length; i++)
			{
				pixels[i] = color;
			}

			var returnTexture = new Texture2D(width, height);
			returnTexture.SetPixels(pixels);
			returnTexture.Apply();

			return returnTexture;
		}
	}
}
#endif