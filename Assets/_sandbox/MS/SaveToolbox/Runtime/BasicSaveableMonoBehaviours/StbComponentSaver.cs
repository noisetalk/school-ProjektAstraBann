using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaveToolbox.Runtime.Attributes;
using SaveToolbox.Runtime.Core.MonoBehaviours;
using SaveToolbox.Runtime.Core.ScriptableObjects;
using SaveToolbox.Runtime.Interfaces;
using SaveToolbox.Runtime.Serialization;
using SaveToolbox.Runtime.Utils;
using UnityEngine;

namespace SaveToolbox.Runtime.BasicSaveableMonoBehaviours
{
	/// <summary>
	/// An important component within the Save Toolbox library. It can hold reference to multiple different components
	/// through the use of a custom component inspector allows you to choose between a bunch of serializable fields on the
	/// target components on the data that is intended to be saved and loaded.
	/// </summary>
	[AddComponentMenu("SaveToolbox/SavingBehaviours/StbComponentSaver")]
	public class StbComponentSaver : SaveableMonoBehaviour
	{
		/// <summary>
		/// All the custom components that are referenced are held in this array of CustomComponentHolders.
		/// </summary>
		[SerializeField]
		public CustomComponentHolder[] customComponentHolders = Array.Empty<CustomComponentHolder>();

		private List<string> fieldNames;

		public override object Serialize()
		{
			var customBehaviourData = new List<CustomSaveBehaviourData>();
			var sortedComponentHolders = customComponentHolders.OrderByDescending(componentHolder => componentHolder.DeserializationPriority);
			foreach (var customComponentHolder in sortedComponentHolders)
			{
				if (!TryGetComponentOfIdentifier(customComponentHolder.Identifier, out var component)) continue;

				CustomSaveBehaviourData customData = null;
				foreach (var fieldName in customComponentHolder.FieldsToSave)
				{
					if (TryGetFieldValue(component, fieldName, out var objectValue))
					{
						if (objectValue == null || string.IsNullOrEmpty(fieldName)) continue;

						if (customData == null)
						{
							customData = new CustomSaveBehaviourData(customComponentHolder.Identifier);
						}
						customData.AddCustomMemberSaveData(new CustomMemberSaveData(fieldName, objectValue));
					}
				}

				var saveDataEntities = customComponentHolder.GetAllComponentSavers();
				var sortedList = saveDataEntities.OrderByDescending(saveEntityObject =>
				{
					if (saveEntityObject is ISaveEntityDeserializationPriority iSaveEntityDeserializationPriority)
					{
						return iSaveEntityDeserializationPriority.DeserializationPriority;
					}

					return 0;
				}).ToList();

				foreach (var saveDataEntity in sortedList)
				{
					if (customData == null)
					{
						customData = new CustomSaveBehaviourData(customComponentHolder.Identifier);
					}

					customData.AddCustomComponentSaverSaveData(new CustomComponentSaverSaveData(saveDataEntity.SaveIdentifier, saveDataEntity.Serialize()));
				}

				customBehaviourData.Add(customData);
			}

			return customBehaviourData;
		}

		public override void Deserialize(object data)
		{
			var customSaveBehaviourData = (List<CustomSaveBehaviourData>)data;
			foreach (var customBehaviourData in customSaveBehaviourData)
			{
				if (!TryGetComponentHolderOfIdentifier(customBehaviourData.Identifier, out var componentHolder)) continue;

				foreach (var customMemberSaveData in customBehaviourData.CustomMemberSaveDatas)
				{
					TrySetFieldValue(componentHolder.Component, customMemberSaveData.DataFieldName, customMemberSaveData.DataFieldValue);
				}

				var saveDataEntities = componentHolder.GetAllComponentSavers();
				foreach (var customComponentSaverSaveData in customBehaviourData.CustomComponentSaverSaveDatas)
				{
					if (GetSaveDataEntityOfIdentifier(customComponentSaverSaveData.Identifier, out var customSaver))
					{
						customSaver.Deserialize(customComponentSaverSaveData.DataFieldValue);
					}
				}

				bool GetSaveDataEntityOfIdentifier(string identifier, out ISaveDataEntity returnSaveDataEntity)
				{
					returnSaveDataEntity = null;
					foreach (var saveDataEntity in saveDataEntities)
					{
						if (saveDataEntity.SaveIdentifier == identifier)
						{
							returnSaveDataEntity = saveDataEntity;
							return true;
						}
					}

					return false;
				}
			}
		}

		private bool TryGetFieldValue(Component component, string methodName, out object objectValue)
		{
			objectValue = null;
			var behaviourType = component.GetType();
			var field = behaviourType.GetField(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
			if (field != null)
			{
				objectValue = field.GetValue(component);
				if (objectValue != null) return true;
			}

			var property = behaviourType.GetProperty(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
			if (property != null)
			{
				objectValue = property.GetValue(component);

				if (objectValue != null) return true;
			}

			return true;
		}

		private bool TrySetFieldValue(Component component, string methodName, object objectValue)
		{
			var behaviourType = component.GetType();
			var field = behaviourType.GetField(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
			if (field != null)
			{
				field.SetValue(component, objectValue);
				return true;
			}

			var property = behaviourType.GetProperty(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
			if (property != null)
			{
				property.SetValue(component, objectValue);
				return true;
			}

			return false;
		}

		/// <summary>
		/// Finds a component that is referenced by this component with an identifier.
		/// </summary>
		/// <param name="identifier">The identifier of the component that is referenced. The identifier is the
		/// identifier of the ISaveDataEntity.</param>
		/// <param name="component">The out component that will be returned if it successfully found the component.</param>
		/// <returns>If it successfully found a component of the passed in identifier.</returns>
		public bool TryGetComponentOfIdentifier(string identifier, out Component component)
		{
			component = null;
			foreach (var customComponentHolder in customComponentHolders)
			{
				if (customComponentHolder.Identifier == identifier)
				{
					component = customComponentHolder.Component;
					return true;
				}
			}

			return false;
		}

		public bool TryGetComponentHolderOfIdentifier(string identifier, out CustomComponentHolder componentHolder)
		{
			componentHolder = null;
			foreach (var customComponentHolder in customComponentHolders)
			{
				if (customComponentHolder.Identifier == identifier)
				{
					componentHolder = customComponentHolder;
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Validates all referenced components. Checks all the identifiers and resets them if one of the same identifier
		/// already in the list.
		/// </summary>
		public void ValidateComponentHolders()
		{
			var preExistingIds = new List<string>();
			foreach (var customComponentHolder in customComponentHolders)
			{
				if (string.IsNullOrEmpty(customComponentHolder.Identifier) || preExistingIds.Contains(customComponentHolder.Identifier))
				{
					customComponentHolder.GenerateNewIdentifier();
				}

				preExistingIds.Add(customComponentHolder.Identifier);
			}
		}

		/// <summary>
		/// Used to check if a type of field can be serialized. Inherit from this class and override this function if you
		/// wish to change which types are looked for in the serializer. This taked into account collections.
		/// </summary>
		/// <param name="type">The type that is being checked whether or not it can be serialized in the component fields.</param>
		/// <returns>Whether or not a field of this type will be serialized by this component.</returns>
		public virtual bool IsOfQualifyingType(Type type)
		{
			type = GetCollectionType(type);

			return IsTypeAccepted(type);
		}

		public virtual List<MemberInfo> GetAllSerializableFieldInfos(Type type)
		{
			var memberInfos = StbUtilities.GetAllSerializableFieldsAndProperties(type, true, false);
			for (var index = memberInfos.Count - 1; index >= 0; index--)
			{
				var memberInfo = memberInfos[index];

				switch (memberInfo)
				{
					case FieldInfo fieldInfo when !IsOfQualifyingType(fieldInfo.FieldType):
					case PropertyInfo propertyInfo when !IsOfQualifyingType(propertyInfo.PropertyType):
						memberInfos.RemoveAt(index);
						break;
				}
			}

			return memberInfos;
		}

		/// <summary>
		/// Used to check if a type of field can be serialized. Inherit from this class and override this function if you
		/// wish to change which types are looked for in the serializer. This taked into account collections.
		/// </summary>
		/// <param name="type">The type that is being checked whether or not it can be serialized in the component fields.</param>
		/// <returns>Whether or not a field of this type will be serialized by this component.</returns>
		protected virtual bool IsTypeAccepted(Type type)
		{
			return StbSerializationUtilities.IsPrimitiveType(type) ||
			       type == typeof(Vector2) ||
			       type == typeof(Vector3) ||
			       type == typeof(Vector4) ||
			       type == typeof(Color) ||
			       type == typeof(Quaternion) ||
			       type == typeof(Texture2D) ||
			       type.IsEnum ||
			       typeof(ScriptableObject).IsAssignableFrom(type) ||
			       type.IsGenericType && type.GetGenericTypeDefinition() == typeof(SaveEntityReference<>);
		}

		/// <summary>
		/// Checks which type a collection is. Override this to handle and different types of collections that aren't arrays or lists.
		/// </summary>
		/// <param name="type">The type that is passed in. Will be checked if it is a collection and then of which type it is, if it is a collection.</param>
		/// <returns>The type that the collection is.</returns>
		protected virtual Type GetCollectionType(Type type)
		{
			if (type.IsArray) // Check if it is an array.
			{
				return type.GetElementType();
			}

			if (type.IsSerializableCollection()) // Check if it is a list.
			{
				return type.GenericTypeArguments[0];
			}

			return type;
		}
	}

	/// <summary>
	/// A class that holds all the components that will be referenced along with an identifier and the fields that
	/// should be saved.
	/// </summary>
	[Serializable]
	public class CustomComponentHolder
	{
		/// <summary>
		/// The referenced component.
		/// </summary>
		[SerializeField]
		private Component component;
		public Component Component => component;

		/// <summary>
		/// The identifier.
		/// </summary>
		[SerializeField, ReadOnly]
		private string identifier = Guid.NewGuid().ToString();
		public string Identifier => identifier;

		[SerializeField]
		private int deserializationPriority;
		public int DeserializationPriority => deserializationPriority;

		/// <summary>
		/// The cached list of field names that will be saved.
		/// </summary>
		[SerializeField]
		private string[] fieldsToSave = Array.Empty<string>();
		public string[] FieldsToSave => fieldsToSave;

		[SerializeReference]
		private object[] customComponentSavers = Array.Empty<object>();
		public object[] CustomComponentSavers => customComponentSavers;

		/// <summary>
		/// Creates a new identifier from the guid and then converts it to a string.
		/// </summary>
		public void GenerateNewIdentifier()
		{
			identifier = Guid.NewGuid().ToString();
		}

		/// <summary>
		/// Resets the identifier and all the fields to save as well as resets the reference to the component back to null.
		/// </summary>
		public void ResetValues()
		{
			component = null;
			identifier = Guid.NewGuid().ToString();
			fieldsToSave = new string[0];
		}

		public bool TryGetCustomComponentSaverOfIdentifier(string identifier, out object customComponentSaverSaveData)
		{
			customComponentSaverSaveData = null;
			foreach (var componentSaver in customComponentSavers)
			{
				var fieldInfo = componentSaver.GetType().GetField("identifier", BindingFlags.Instance | BindingFlags.NonPublic);
				var newIdentifier = (string)fieldInfo?.GetValue(this);
				if (newIdentifier == identifier)
				{
					customComponentSaverSaveData = componentSaver;
					return true;
				}
			}

			return false;
		}

		public List<ISaveDataEntity> GetAllComponentSavers()
		{
			var returnList = new List<ISaveDataEntity>();
			foreach (var customComponentSaver in customComponentSavers)
			{
				returnList.Add((ISaveDataEntity)customComponentSaver);
			}

			return returnList;
		}
	}

	[Serializable]
	public class CustomSaveBehaviourData
	{
		[SerializeField, StbSerialize]
		private string identifier;
		public string Identifier => identifier;

		[SerializeField, StbSerialize]
		private List<CustomMemberSaveData> customMemberSaveDatas = new List<CustomMemberSaveData>();
		public List<CustomMemberSaveData> CustomMemberSaveDatas => customMemberSaveDatas;

		[SerializeField, StbSerialize]
		private List<CustomComponentSaverSaveData> customComponentSaverSaveDatas = new List<CustomComponentSaverSaveData>();
		public List<CustomComponentSaverSaveData> CustomComponentSaverSaveDatas => customComponentSaverSaveDatas;

		public CustomSaveBehaviourData() {}

		public CustomSaveBehaviourData(string identifier, List<CustomMemberSaveData> customMemberSaveDatas)
		{
			this.identifier = identifier;
			this.customMemberSaveDatas = customMemberSaveDatas;
		}

		public CustomSaveBehaviourData(string identifier)
		{
			this.identifier = identifier;
		}

		public void AddCustomMemberSaveData(CustomMemberSaveData customMemberSaveData)
		{
			customMemberSaveDatas.Add(customMemberSaveData);
		}

		public void AddCustomComponentSaverSaveData(CustomComponentSaverSaveData customComponentSaverSaveData)
		{
			customComponentSaverSaveDatas.Add(customComponentSaverSaveData);
		}

		public bool TryGetCustomComponentSaverOfIdentifier(string identifier, out CustomComponentSaverSaveData customComponentSaverSaveData)
		{
			customComponentSaverSaveData = default;
			foreach (var componentSaver in customComponentSaverSaveDatas)
			{
				if (componentSaver.Identifier == identifier)
				{
					customComponentSaverSaveData = componentSaver;
					return true;
				}
			}

			return false;
		}
	}

	[Serializable]
	public class CustomMemberSaveData
	{
		[SerializeField, StbSerialize]
		private string dataFieldName;
		public string DataFieldName => dataFieldName;

		[SerializeField, StbSerialize]
		private object dataFieldValue;
		public object DataFieldValue => dataFieldValue;

		public CustomMemberSaveData() {}

		public CustomMemberSaveData(string dataFieldName, object dataFieldValue)
		{
			this.dataFieldName = dataFieldName;
			this.dataFieldValue = dataFieldValue;
		}
	}

	[Serializable]
	public struct CustomComponentSaverSaveData
	{
		[SerializeField, StbSerialize]
		private string identifier;
		public string Identifier => identifier;

		[SerializeField, StbSerialize]
		private object dataFieldValue;
		public object DataFieldValue => dataFieldValue;

		public CustomComponentSaverSaveData(string identifier, object dataFieldValue)
		{
			this.identifier = identifier;
			this.dataFieldValue = dataFieldValue;
		}
	}
}