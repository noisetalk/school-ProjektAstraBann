using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using SaveToolbox.Runtime.Attributes;
using SaveToolbox.Runtime.Core;
using SaveToolbox.Runtime.Core.MonoBehaviours;
using SaveToolbox.Runtime.Interfaces;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SaveToolbox.Runtime.Utils
{
	public static class StbUtilities
	{
		private static List<ISaveDataEntity> cachedSaveDataEntities = new List<ISaveDataEntity>();

		/// <summary>
		/// Returns all objects of a specific type in all currently loaded scenes.
		/// </summary>
		/// <typeparam name="T">The type of objects to return.</typeparam>
		/// <returns>ALl object instance of type type params.</returns>
		public static List<T> GetAllObjectsInAllScenes<T>()
		{
			var activeSceneCount = SceneManager.sceneCount;
			var foundObjectsList = new List<T>();
			for (var i = 0; i < activeSceneCount; i++)
			{
				var scene = SceneManager.GetSceneAt(i);
				var rootGameObjects = scene.GetRootGameObjects();
				foreach (var rootGameObject in rootGameObjects)
				{
					foundObjectsList.AddRange(rootGameObject.GetComponentsInChildren<T>(true));
				}
			}

			return foundObjectsList;
		}

		public static void CacheSaveDataEntities()
		{
			cachedSaveDataEntities = GetAllObjectsInAllScenes<ISaveDataEntity>();
		}

		/// <summary>
		/// Finds an object implements ISaveDataEntity using it's identifier.
		/// </summary>
		/// <param name="id">The identifier used of the save data entity.</param>
		/// <param name="saveDataEntity">The save data entity returned.</param>
		/// <returns>If a save data entity with a matching identifier was found.</returns>
		public static bool TryGetISaveDataEntity(string id, out ISaveDataEntity saveDataEntity)
		{
			saveDataEntity = default;
			if (cachedSaveDataEntities.Count == 0)
			{
				CacheSaveDataEntities();
			}

			foreach (var entity in cachedSaveDataEntities)
			{
				if (entity.SaveIdentifier == id)
				{
					saveDataEntity = entity;
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Gets all the MemberInfos of serializable fields and properties on a type.
		/// </summary>
		/// <param name="objectType">The type the member infos should be found.</param>
		/// <param name="preventFieldsAndPropertyDuplicates">Do we want to prevent field duplicates that are found as backing fields for properties.</param>
		/// <param name="useUnitySerializationRules">Do we only want to find properties and fields that use Unity serializable rules such as private [SerializeFields] and public fields.</param>
		/// <returns>All found MemberInfos of serializable properties and fields.</returns>
		public static List<MemberInfo> GetAllSerializableFieldsAndProperties(Type objectType, bool preventFieldsAndPropertyDuplicates = true, bool useUnitySerializationRules = true)
		{
			List<MemberInfo> membersInfo = new List<MemberInfo>();

			var allFields = objectType.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
			foreach (var fieldInfo in allFields)
			{
				if ((useUnitySerializationRules && !Attribute.IsDefined(fieldInfo, typeof(SerializeField)) && !fieldInfo.IsPublic) && !Attribute.IsDefined(fieldInfo, typeof(StbSerialize))) continue;

				membersInfo.Add(fieldInfo);
			}

			var allProperties = objectType.GetProperties(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
			foreach (var propertyInfo in allProperties)
			{
				if (!propertyInfo.CanRead) continue;

				if (preventFieldsAndPropertyDuplicates)
				{
					var backingFieldName = $"<{propertyInfo.Name}>k__BackingField";
					var backingField = propertyInfo.DeclaringType?.GetField(backingFieldName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
					if (membersInfo.Contains(backingField)) continue;
				}

				if (useUnitySerializationRules && !Attribute.IsDefined(propertyInfo, typeof(SerializeField)) && propertyInfo.SetMethod == null && !Attribute.IsDefined(propertyInfo, typeof(StbSerialize))) continue;

				membersInfo.Add(propertyInfo);
			}

			return membersInfo;
		}

		/// <summary>
		/// Gets all the MemberInfos of serializable fields and properties on a type using serialization settings.
		/// </summary>
		/// <param name="objectType">The type the member infos should be found.</param>
		/// <param name="serializationSettings">The settings which determine with MemberInfos will be returned by this method from the provided type.</param>
		/// <returns></returns>
		public static List<MemberInfo> GetAllSerializableFieldsAndProperties(Type objectType, StbSerializationSettings serializationSettings)
		{
			List<MemberInfo> membersInfo = new List<MemberInfo>();

			var fieldSettings = serializationSettings.FieldSerializationSettings;
			var targetsFields = fieldSettings.ValidTarget;

			var propertySettings = serializationSettings.PropertySerializationSettings;
			var targetsProperties = propertySettings.ValidTarget;

			if (targetsFields)
			{
				var allFields = objectType.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
				foreach (var fieldInfo in allFields)
				{
					if ((fieldSettings.TargetSerializeFieldAttribute && Attribute.IsDefined(fieldInfo, typeof(SerializeField))) ||
					    (fieldSettings.TargetStbSerializeAttribute && Attribute.IsDefined(fieldInfo, typeof(StbSerialize))) ||
					    (fieldSettings.TargetAllPublic && fieldInfo.IsPublic) ||
					    (fieldSettings.TargetAllPrivate && !fieldInfo.IsPublic))
					{
						membersInfo.Add(fieldInfo);
					}
				}
			}

			if (targetsProperties)
			{
				var allProperties = objectType.GetProperties(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
				foreach (var propertyInfo in allProperties)
				{
					// If we can't read/write to it, we can't serialize it.
					var hasGetterAndSetter = propertyInfo.GetMethod != null && propertyInfo.SetMethod != null;
					if (!propertyInfo.CanRead || !hasGetterAndSetter) continue;

					// Check for duplicate field and continue if it is already getting saved.
					var backingFieldName = $"<{propertyInfo.Name}>k__BackingField";
					var backingField = propertyInfo.DeclaringType?.GetField(backingFieldName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
					if (membersInfo.Contains(backingField)) continue;

					if ((propertySettings.TargetSerializeFieldAttribute && Attribute.IsDefined(propertyInfo, typeof(SerializeField))) ||
					    (propertySettings.TargetStbSerializeAttribute && Attribute.IsDefined(propertyInfo, typeof(StbSerialize))) ||
					    (propertySettings.TargetAllPublic && propertyInfo.GetMethod.IsPublic) ||
					    (propertySettings.TargetAllPrivate && !propertyInfo.GetMethod.IsPublic))
					{
						membersInfo.Add(propertyInfo);
					}
				}
			}

			return membersInfo;
		}

		public static object GetMemberInfoValue(this MemberInfo memberInfo, object targetObject)
		{
			switch (memberInfo)
			{
				case FieldInfo fieldInfo:
					return fieldInfo.GetValue(targetObject);
				case PropertyInfo propertyInfo:
					return propertyInfo.GetValue(targetObject);
				default:
					Debug.LogWarning("Tried to get a value from a type that wasn't a member info.");
					return null;
			}
		}

		public static void SetMemberInfoValue(this MemberInfo memberInfo, object targetObject, object value)
		{
			switch (memberInfo)
			{
				case FieldInfo fieldInfo:
					fieldInfo.SetValue(targetObject, value);
					break;
				case PropertyInfo propertyInfo:
					propertyInfo.SetValue(targetObject, value);
					break;
				default:
					Debug.LogWarning("Tried to set a value from a type that wasn't a member info.");
					break;
			}
		}

		public static Type GetMemberInfoType(this MemberInfo memberInfo)
		{
			switch (memberInfo)
			{
				case FieldInfo fieldInfo:
					return fieldInfo.FieldType;
				case PropertyInfo propertyInfo:
					return propertyInfo.PropertyType;
				default:
					Debug.LogWarning("Tried to get a type from a type that wasn't a member info.");
					return null;
			}
		}

#if UNITY_EDITOR
		public static void DrawMemberInfos(object targetObject, List<MemberInfo> memberInfos)
		{
			foreach (var memberInfo in memberInfos)
			{
				DrawMemberInfo(targetObject, memberInfo);
			}
		}

		public static void DrawMemberInfo(object targetObject, MemberInfo memberInfo)
		{
			var memberType = memberInfo.GetMemberInfoType();

			if (memberType == typeof(string))
			{
				memberInfo.SetMemberInfoValue(targetObject, EditorGUILayout.TextField(ObjectNames.NicifyVariableName(memberInfo.Name), (string)memberInfo.GetMemberInfoValue(targetObject)));
			}
			else if (memberType == typeof(float))
			{
				memberInfo.SetMemberInfoValue(targetObject, EditorGUILayout.FloatField(ObjectNames.NicifyVariableName(memberInfo.Name), (float)memberInfo.GetMemberInfoValue(targetObject)));
			}
			else if (memberType == typeof(int))
			{
				memberInfo.SetMemberInfoValue(targetObject, EditorGUILayout.IntField(ObjectNames.NicifyVariableName(memberInfo.Name), (int)memberInfo.GetMemberInfoValue(targetObject)));
			}
			else if (memberType.IsEnum)
			{
				var enumType = (Enum)memberInfo.GetMemberInfoValue(targetObject);
				memberInfo.SetMemberInfoValue(targetObject, EditorGUILayout.EnumPopup(ObjectNames.NicifyVariableName(memberInfo.Name), enumType));
			}
			else if (memberType == typeof(double))
			{
				memberInfo.SetMemberInfoValue(targetObject, EditorGUILayout.DoubleField(ObjectNames.NicifyVariableName(memberInfo.Name), (double)memberInfo.GetMemberInfoValue(targetObject)));
			}
			else if (memberType == typeof(decimal))
			{
				var decimalValue = (decimal)memberInfo.GetMemberInfoValue(targetObject);
				var floatValue = EditorGUILayout.FloatField(ObjectNames.NicifyVariableName(memberInfo.Name), (float)decimalValue);
				memberInfo.SetMemberInfoValue(targetObject, (decimal)floatValue);
			}
			else if (memberType == typeof(bool))
			{
				memberInfo.SetMemberInfoValue(targetObject, EditorGUILayout.Toggle(ObjectNames.NicifyVariableName(memberInfo.Name), (bool)memberInfo.GetMemberInfoValue(targetObject)));
			}
			else if (memberType.BaseType == typeof(object) && memberType.BaseType != typeof(Component))
			{
				DrawMemberInfos(memberInfo.GetMemberInfoValue(targetObject), GetAllSerializableFieldsAndProperties(memberType));
			}
			else if (memberType.BaseType == typeof(Component))
			{
				var value = memberInfo.GetMemberInfoValue(targetObject) as UnityEngine.Object;
				memberInfo.SetMemberInfoValue(targetObject, EditorGUILayout.ObjectField(ObjectNames.NicifyVariableName(memberInfo.Name), value, memberType, true));
			}
		}

		public static void DrawCustomEditor(object targetObject)
		{
			var memberInfos = GetAllSerializableFieldsAndProperties(targetObject.GetType());
			DrawMemberInfos(targetObject, memberInfos);
		}
#endif

		internal static string GetAssetTypeFileExtension(StbFileFormat assetType)
		{
			switch (assetType)
			{
				case StbFileFormat.Json:
					return ".json";
				case StbFileFormat.Binary:
					return ".bin";
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		public static Task ConvertToTask(this AsyncOperation asyncOperation)
		{
			var taskCompletionSource = new TaskCompletionSource<bool>();
			asyncOperation.completed += _ =>
			{
				taskCompletionSource.SetResult(true);
			};
			return taskCompletionSource.Task;
		}

		/// <summary>
		/// Lossy scale has no setter so we need to apply lossy scale through the local scale.
		/// </summary>
		/// <param name="target">The target transform.</param>
		/// <param name="lossyScaleToApply">The lossy scale that should be applied.</param>
		public static void SetLossyScale(this Transform target, Vector3 lossyScaleToApply)
		{
			// Calculate the cumulative parent scale of the target
			var parent = target.parent;
			var runningTotalScale = Vector3.one;

			// Get all the parents above the transform.
			while (parent != null)
			{
				var parentLocalScale = parent.localScale;
				runningTotalScale = Vector3.Scale(runningTotalScale, parentLocalScale);
				parent = parent.parent;
			}

			var normalizedParentScale = new Vector3(1f / runningTotalScale.x, 1f / runningTotalScale.y, 1f / runningTotalScale.z);
			var scale = new Vector3(normalizedParentScale.x * lossyScaleToApply.x, normalizedParentScale.y * lossyScaleToApply.y, normalizedParentScale.z * lossyScaleToApply.z);
			target.localScale = scale;
		}

		public static bool CheckAllScenesForDuplicateIDs(out List<string> duplicateIds)
		{
			var iSaveDataEntities = StbUtilities.GetAllObjectsInAllScenes<ISaveDataEntity>();
			var ids = new List<string>();
			duplicateIds = new List<string>();
			var hasDuplicates = false;

			foreach (var iSaveDataEntity in iSaveDataEntities)
			{
				if (ids.Contains(iSaveDataEntity.SaveIdentifier))
				{
					hasDuplicates = true;
					duplicateIds.Add(iSaveDataEntity.SaveIdentifier);
				}
				else
				{
					ids.Add(iSaveDataEntity.SaveIdentifier);
				}
			}

			return hasDuplicates;
		}

		public static List<(string, MonoBehaviour)> GetAllMonoBehavioursWithDuplicateIds()
		{
			var iSaveDataEntities = GetAllObjectsInAllScenes<ISaveDataEntity>();
			var returnObjects = new List<(string, MonoBehaviour)>();
			var idDictionary = new Dictionary<string, MonoBehaviour>();

			foreach (var iSaveDataEntity in iSaveDataEntities)
			{
				if (idDictionary.TryGetValue(iSaveDataEntity.SaveIdentifier, out var value))
				{
					var monoBehaviour = iSaveDataEntity as MonoBehaviour;
					if (monoBehaviour == null) continue;

					returnObjects.Add((iSaveDataEntity.SaveIdentifier, monoBehaviour));
					var alreadyHasDupe = false;
					foreach (var returnObject in returnObjects)
					{
						if (returnObject.Item2 == monoBehaviour)
						{
							alreadyHasDupe = true;
							break;
						}
					}

					if (!alreadyHasDupe)
					{
						returnObjects.Add((iSaveDataEntity.SaveIdentifier, value));
					}
				}
				else
				{
					if (iSaveDataEntity is MonoBehaviour monoBehaviour)
					{
						idDictionary.Add(iSaveDataEntity.SaveIdentifier, monoBehaviour);
					}
				}
			}

			return returnObjects;
		}

		public static void GenerateNewIdsForAllSceneSaveDataEntities()
		{
			var iSaveDataEntities = GetAllObjectsInAllScenes<ISaveDataEntity>();
#if STB_ABOVE_2021_3
			foreach (var iSaveDataEntity in iSaveDataEntities)
			{
				iSaveDataEntity.GenerateNewIdentifier();
			}

#else
			foreach (var iSaveDataEntity in iSaveDataEntities)
			{
				iSaveDataEntity.SaveIdentifier = Guid.NewGuid().ToString();
			}
#endif

		}

		public static bool ImplementsType(this Type sourceType, Type targetType)
		{
			var implements = targetType.IsAssignableFrom(sourceType);
			return implements;
		}
	}
}