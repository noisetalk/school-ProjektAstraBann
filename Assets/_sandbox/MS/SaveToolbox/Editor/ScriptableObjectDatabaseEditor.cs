using SaveToolbox.Runtime.Core.ScriptableObjects;
using UnityEditor;
using UnityEngine;

namespace SaveToolbox.Editor
{
	[CustomEditor(typeof(ScriptableObjectDatabase))]
	public class ScriptableObjectDatabaseEditor : UnityEditor.Editor
	{
		private GUIStyle databaseInfoBoxStyle;
		private GUIStyle dragAndDropInfoStyle;
		private GUIStyle databaseBoxStyle;
		private GUIStyle buttonStyle;

		private Rect lastRect;

		private SerializedProperty scriptableObjectsEntityProperty;

		private void CreateStyles()
		{
			if (databaseInfoBoxStyle == null)
			{
				databaseInfoBoxStyle = new GUIStyle(EditorStyles.helpBox)
				{
					fontSize = 13,
					alignment = TextAnchor.UpperCenter,
					richText = true
				};
			}

			if (dragAndDropInfoStyle == null)
			{
				dragAndDropInfoStyle = new GUIStyle(EditorStyles.helpBox)
				{
					fontSize = 18,
					alignment = TextAnchor.UpperCenter,
					richText = true,
					fontStyle = FontStyle.Bold,
					normal = new GUIStyleState
					{
						textColor = Color.white,
						background = CreateTexture(new Color(0f, 1f, 0f, 0.3f))
					}
				};
			}

			if (databaseBoxStyle == null)
			{
				databaseBoxStyle = new GUIStyle(GUI.skin.box)
				{
					padding = new RectOffset(15, 15, 15, 15),
					margin = new RectOffset(15, 15, 15, 15)
				};
			}

			if (buttonStyle == null)
			{
				buttonStyle = new GUIStyle(GUI.skin.button)
				{
					fontSize = 22,
					fixedHeight = 45,
					fixedWidth = 200,
					alignment = TextAnchor.MiddleCenter
				};
			}
		}

		private void InitializeProperty()
		{
			if (scriptableObjectsEntityProperty != null) return;

			scriptableObjectsEntityProperty = serializedObject.FindProperty("scriptableObjectEntries");
		}

		public override void OnInspectorGUI()
		{
			CreateStyles();

			var scriptableObjectDatabase = (ScriptableObjectDatabase)target;
			if (scriptableObjectDatabase == null) return;

			InitializeProperty();

			var draggingMultipleObjects = TryHandleMultipleItemsDraggedAndDropped();

			if (draggingMultipleObjects)
			{
				GUI.enabled = false;
			}
			else
			{
				EditorGUILayout.BeginVertical();
			}

			EditorGUILayout.LabelField("This is the scriptable object database. This references a scriptable object in the SaveToolbox package." +
			                           "\n For Save Toolbox to save and load the scriptable objects you must assign them in this database." +
			                           "\n This database uses the asset guid to reference scriptable objects, any changes to this asset guid will cause the reference to break." +
			                           "\n This should not happen under normal uses cases but if it does just reassign the scriptable object to the database.", databaseInfoBoxStyle);

			if (draggingMultipleObjects)
			{
				EditorGUILayout.LabelField("Drag and drop your scriptable objects to add them to the database.", dragAndDropInfoStyle);
			}

			EditorGUILayout.Space();

			EditorGUI.indentLevel++;

			for (var i = 0; i < scriptableObjectsEntityProperty.arraySize; i++)
			{
				DrawEntry(i);
			}

			EditorGUI.indentLevel--;

			if (GUILayout.Button("Add Scriptable Object"))
			{
				AddElement();
			}
			serializedObject.ApplyModifiedProperties();

			GUILayout.Label($"Total database scriptable objects: {scriptableObjectDatabase.scriptableObjectEntries.Count}", databaseInfoBoxStyle);

			if (draggingMultipleObjects)
			{
				GUI.enabled = true;
				if (lastRect.height < Screen.height)
				{
					EditorGUILayout.Space(Screen.height - lastRect.height);
				}
			}
			else
			{
				EditorGUILayout.EndVertical();
				lastRect = GUILayoutUtility.GetLastRect();
			}
		}

		private void DrawEntry(int index)
		{
			var scriptableObjectDatabase = (ScriptableObjectDatabase)target;
			var entry = scriptableObjectDatabase.scriptableObjectEntries[index];

			EditorGUILayout.BeginHorizontal();
			EditorGUI.BeginChangeCheck();

			var newScriptableObject = (ScriptableObject)EditorGUILayout.ObjectField(entry.ScriptableObject, typeof(ScriptableObject), false);

			var previousState = GUI.enabled;
			GUI.enabled = false;

			EditorGUILayout.TextField(entry.ScriptableObjectAssetGuid);

			GUI.enabled = previousState;

			if (EditorGUI.EndChangeCheck())
			{
				entry.ScriptableObject = newScriptableObject;

				var elementAtIndex = scriptableObjectsEntityProperty.GetArrayElementAtIndex(index);
				var scriptableObjectReference = elementAtIndex.FindPropertyRelative("scriptableObject");
				var scriptableObjectAssetGuid = elementAtIndex.FindPropertyRelative("scriptableObjectAssetGuid");

				scriptableObjectReference.objectReferenceValue = newScriptableObject;
				scriptableObjectAssetGuid.stringValue = entry.ScriptableObjectAssetGuid;

				EditorUtility.SetDirty(target);
				serializedObject.ApplyModifiedProperties();
			}

			if (GUILayout.Button("Remove", GUILayout.Width(70f)))
			{
				DeleteElement(index);
			}

			EditorGUILayout.EndHorizontal();
		}

		private bool TryHandleMultipleItemsDraggedAndDropped()
		{
			var multipleSelected = DragAndDrop.objectReferences.Length > 1;
			if (!multipleSelected) return false;

			var fullWindowRect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.ExpandHeight(true));
			fullWindowRect.yMin = 0f;
			fullWindowRect.height = lastRect.height < Screen.height ? Screen.height : lastRect.height;

			// Add a grey rect to the screen.
			var currentEvent = Event.current;
			switch (currentEvent.type)
			{
				case EventType.DragUpdated:
					// Sets mouse visual.
					EditorGUI.DrawRect(fullWindowRect, new Color(0.8f, 0.8f, 0.8f, 0.2f));
					DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
					Event.current.Use();
					break;
				case EventType.DragPerform:
					// If the mouse is not in the drop area.
					if (!fullWindowRect.Contains(currentEvent.mousePosition)) return true;
					EditorGUI.DrawRect(fullWindowRect, new Color(0.8f, 0.8f, 0.8f, 0.2f));
					DragAndDrop.AcceptDrag();

					foreach (var draggedScriptableObject in DragAndDrop.objectReferences)
					{
						if (draggedScriptableObject is ScriptableObject scriptableObject)
						{
							var newElement = AddElement();
							var scriptableObjectReference = newElement.FindPropertyRelative("scriptableObject");
							var scriptableObjectAssetGuid = newElement.FindPropertyRelative("scriptableObjectAssetGuid");

							scriptableObjectReference.objectReferenceValue = scriptableObject;
							// Have to use asset database here instead of the setter on ScriptableObject because we can't cast boxed value on this version of 2021 unity.
#if UNITY_EDITOR
							if (AssetDatabase.TryGetGUIDAndLocalFileIdentifier(scriptableObject, out var guid, out long _))
							{
								scriptableObjectAssetGuid.stringValue = guid;

							}
#endif
							EditorUtility.SetDirty(target);
							serializedObject.ApplyModifiedProperties();
						}
					}
					Event.current.Use();
					break;
				default:
					if (!fullWindowRect.Contains(currentEvent.mousePosition)) return true;
					EditorGUI.DrawRect(fullWindowRect, new Color(0.8f, 0.8f, 0.8f, 0.2f));
					break;
			}

			return true;
		}

		private SerializedProperty AddElement()
		{
			scriptableObjectsEntityProperty.arraySize++;
			EditorUtility.SetDirty(target);
			serializedObject.ApplyModifiedProperties();

			var elementAtIndex = scriptableObjectsEntityProperty.GetArrayElementAtIndex(scriptableObjectsEntityProperty.arraySize - 1);
			var scriptableObjectReference = elementAtIndex.FindPropertyRelative("scriptableObject");
			var scriptableObjectAssetGuid = elementAtIndex.FindPropertyRelative("scriptableObjectAssetGuid");
			scriptableObjectReference.objectReferenceValue = null;
			scriptableObjectAssetGuid.stringValue = string.Empty;
			EditorUtility.SetDirty(target);
			serializedObject.ApplyModifiedProperties();
			return elementAtIndex;
		}

		private void DeleteElement(int index)
		{
			scriptableObjectsEntityProperty.DeleteArrayElementAtIndex(index);
			EditorUtility.SetDirty(target);
			serializedObject.ApplyModifiedProperties();
		}

		private Texture2D CreateTexture(Color textureColor)
		{
			var newTexture = new Texture2D(1, 1);
			newTexture.SetPixel(0, 0, textureColor);
			newTexture.Apply();
			return newTexture;
		}
	}
}