using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using SaveToolbox.Runtime.BasicSaveableMonoBehaviours;
using SaveToolbox.Runtime.Core;
using SaveToolbox.Runtime.Core.ScriptableObjects;
using SaveToolbox.Runtime.Serialization.Json;
using SaveToolbox.Runtime.Utils;
using UnityEditor;
using UnityEngine;

namespace SaveToolbox.Runtime.Serialization.Binary
{
	/// <summary>
	/// A custom json serializer built specifically for Save Toolbox however it can be used outside of Save Toolbox.
	/// </summary>
	public class StbJsonSerializer
	{
		/// <summary>
		/// Will this serializer use serialize data using "pretty print" meaning it will style it
		/// with tabs and new line characters. Helps with readability.
		/// </summary>
		public bool UsesPrettyPrint { get; set; } = true;

		public StbSerializationSettings serializationSettings = new StbSerializationSettings();

		private float currentFrameTime;
		private float startFrameTime;

		/// <summary>
		/// Convert the object to json data.
		/// </summary>
		/// <param name="objectValue">The object instance you want to be converted to json data string.</param>
		/// <returns>The json data in the form of a string.</returns>
		public string ToJson(object objectValue)
		{
			var rootNode = SerializeObject(null, objectValue);
			var stringBuilder = new StringBuilder();
			rootNode.Render(stringBuilder, UsesPrettyPrint);
			return stringBuilder.ToString();
		}

		/// <summary>
		/// Convert the object to json data asynchronously.
		/// </summary>
		/// <param nodeName="objectValue">The object instance you want to be converted to json data string.</param>
		/// <returns>The json data in a task in the form of a string.</returns>
		public async Task<string> ToJsonAsync(object objectValue)
		{
			var rootNode = await SerializeObjectAsync(null, objectValue);
			var stringBuilder = new StringBuilder();
			await rootNode.RenderAsync(stringBuilder, UsesPrettyPrint);
			return stringBuilder.ToString();
		}

		/// <summary>
		/// Converts a json string into an instance of the type in the type params.
		/// </summary>
		/// <param nodeName="jsonText">The string of json text.</param>
		/// <param name="jsonText">The json text to be deserialized.</param>
		/// <typeparam nodeName="T">The type of the object that is expected to be returned.</typeparam>
		/// <typeparam name="T">The type generic.</typeparam>
		/// <returns>An instance of an object of the type of the type param.</returns>
		public T FromJson<T>(string jsonText)
		{
			var rootNode = StbSerializationUtilities.CreateNodesFromData(jsonText, 0)[0];
			return DeserializeObject<T>(rootNode);
		}

		/// <summary>
		/// Converts a json string into an instance of the type in the type params.
		/// </summary>
		/// <param nodeName="jsonStringInBytes">A json string represented in bytes.</param>
		/// <typeparam nodeName="T">The type of the object that is expected to be returned.</typeparam>
		/// <returns>An instance of an object of the type of the type param.</returns>
		public T FromJson<T>(byte[] jsonStringInBytes)
		{
			var jsonString = Encoding.UTF8.GetString(jsonStringInBytes);
			return FromJson<T>(jsonString);
		}

		/// <summary>
		/// Converts a json string into an instance of the type in the type params asynchronously.
		/// </summary>
		/// <param nodeName="jsonText">The string of json text.</param>
		/// <typeparam nodeName="T">The type of the object that is expected to be returned.</typeparam>
		/// <returns>An instance of an object of the type of the type param. Inside of a task type.</returns>
		public async Task<T> FromJsonAsync<T>(string jsonText)
		{
			var rootNode = (await StbSerializationUtilities.CreateNodesFromDataAsync(jsonText, 0))[0];
			return await DeserializeObjectAsync<T>(rootNode);
		}

		/// <summary>
		/// Converts a json string into an instance of the type in the type params asynchronously.
		/// </summary>
		/// <param nodeName="jsonStringInBytes">A json string represented in bytes.</param>
		/// <typeparam nodeName="T">The type of the object that is expected to be returned.</typeparam>
		/// <returns>An instance of an object of the type of the type param.</returns>
		public async Task<T> FromJsonAsync<T>(byte[] jsonStringInBytes)
		{
			var jsonString = Encoding.UTF8.GetString(jsonStringInBytes);
			return await FromJsonAsync<T>(jsonString);
		}

		private JsonBaseNode SerializeObject(Type expectedType, object objectValue, string nodeName = "")
		{
			if (objectValue == null)
			{
				return new JsonObjectNode()
				{
					Name = nodeName
				};
			}

			var actualType = objectValue.GetType();

			JsonBaseNode returnJsonNode;

			// If it is a primitive serialize it.
			if (StbSerializationUtilities.IsPrimitiveType(actualType))
			{
				returnJsonNode = SerializePrimitive(actualType, objectValue);
			}
			// If it is an object type with custom serialization, use that.
			else if (CanSerialize(objectValue.GetType()))
			{
				returnJsonNode = Serialize(objectValue);
			}
			else
			{
				var isCollection = actualType.IsArray || actualType.IsSerializableCollection();

				// If it is a collection serialize it differently.
				if (isCollection)
				{
					returnJsonNode = SerializeCollection(actualType, objectValue);
				}
				// Otherwise serialize it by using selection.
				else
				{
					returnJsonNode = SerializeObjectThroughReflection(actualType, objectValue);
				}
			}

			SerializeTypeIfNecessary(expectedType, actualType, returnJsonNode);

			if (!string.IsNullOrEmpty(nodeName))
			{
				returnJsonNode.Name = nodeName;
			}

			return returnJsonNode;
		}

		private async Task<JsonBaseNode> SerializeObjectAsync(Type expectedType, object objectValue, string nodeName = "")
		{
			if (objectValue == null)
			{
				return new JsonObjectNode()
				{
					Name = nodeName
				};
			}

			var actualType = objectValue.GetType();

			JsonBaseNode returnJsonNode;

			// If it is a primitive serialize it.
			if (StbSerializationUtilities.IsPrimitiveType(actualType))
			{
				returnJsonNode = SerializePrimitive(actualType, objectValue);
			}
			// If it is an object type with custom serialization, use that.
			else if (CanSerialize(objectValue.GetType()))
			{
				returnJsonNode = Serialize(objectValue);
			}
			else
			{
				var isCollection = actualType.IsArray || actualType.IsSerializableCollection();

				// If it is a collection serialize it differently.
				if (isCollection)
				{
					returnJsonNode = await SerializeCollectionAsync(actualType, objectValue);
				}
				// Otherwise serialize it by using selection.
				else
				{
					returnJsonNode = await SerializeObjectThroughReflectionAsync(actualType, objectValue);
				}
			}

			SerializeTypeIfNecessary(expectedType, actualType, returnJsonNode);

			if (!string.IsNullOrEmpty(nodeName))
			{
				returnJsonNode.Name = nodeName;
			}

			await CheckFrameTime();
			return returnJsonNode;
		}

		private JsonBaseNode SerializePrimitive(Type type, object objectValue)
		{
			if (!type.IsArray)
			{
				return new JsonSimpleNode(objectValue);
			}

			var returnJsonNode = new JsonPrimitiveArrayNode();

			if (type == typeof(bool[]))
			{
				var array = (bool[])objectValue;
				returnJsonNode.PrimitiveChildren = new object[array.Length];
				Array.Copy(array, returnJsonNode.PrimitiveChildren, array.Length);
			}
			if (type == typeof(string[]))
			{
				var array = (string[])objectValue;
				returnJsonNode.PrimitiveChildren = new object[array.Length];
				Array.Copy(array, returnJsonNode.PrimitiveChildren, array.Length);
			}
			else if (type == typeof(char[]))
			{
				var array = (char[])objectValue;
				returnJsonNode.PrimitiveChildren = new object[array.Length];
				Array.Copy(array, returnJsonNode.PrimitiveChildren, array.Length);
			}
			else if (type == typeof(float[]))
			{
				var array = (float[])objectValue;
				returnJsonNode.PrimitiveChildren = new object[array.Length];
				Array.Copy(array, returnJsonNode.PrimitiveChildren, array.Length);
			}
			else if (type == typeof(double[]))
			{
				var array = (double[])objectValue;
				returnJsonNode.PrimitiveChildren = new object[array.Length];
				Array.Copy(array, returnJsonNode.PrimitiveChildren, array.Length);
			}
			else if (type == typeof(decimal[]))
			{
				var array = (decimal[])objectValue;
				returnJsonNode.PrimitiveChildren = new object[array.Length];
				Array.Copy(array, returnJsonNode.PrimitiveChildren, array.Length);
			}
			else if (type == typeof(int[]))
			{
				var array = (int[])objectValue;
				returnJsonNode.PrimitiveChildren = new object[array.Length];
				Array.Copy(array, returnJsonNode.PrimitiveChildren, array.Length);
			}
			else if (type == typeof(uint[]))
			{
				var array = (uint[])objectValue;
				returnJsonNode.PrimitiveChildren = new object[array.Length];
				Array.Copy(array, returnJsonNode.PrimitiveChildren, array.Length);
			}
			else if (type == typeof(long[]))
			{
				var array = (long[])objectValue;
				returnJsonNode.PrimitiveChildren = new object[array.Length];
				Array.Copy(array, returnJsonNode.PrimitiveChildren, array.Length);
			}
			else if (type == typeof(ulong[]))
			{
				var array = (ulong[])objectValue;
				returnJsonNode.PrimitiveChildren = new object[array.Length];
				Array.Copy(array, returnJsonNode.PrimitiveChildren, array.Length);
			}
			else if (type == typeof(short[]))
			{
				var array = (short[])objectValue;
				returnJsonNode.PrimitiveChildren = new object[array.Length];
				Array.Copy(array, returnJsonNode.PrimitiveChildren, array.Length);
			}
			else if (type == typeof(ushort[]))
			{
				var array = (ushort[])objectValue;
				returnJsonNode.PrimitiveChildren = new object[array.Length];
				Array.Copy(array, returnJsonNode.PrimitiveChildren, array.Length);
			}
			else if (type == typeof(byte[]))
			{
				var array = (byte[])objectValue;
				returnJsonNode.PrimitiveChildren = new object[array.Length];
				Array.Copy(array, returnJsonNode.PrimitiveChildren, array.Length);
			}
			else if (type == typeof(sbyte[]))
			{
				var array = (sbyte[])objectValue;
				returnJsonNode.PrimitiveChildren = new object[array.Length];
				Array.Copy(array, returnJsonNode.PrimitiveChildren, array.Length);
			}

			return returnJsonNode;
		}

		private async Task<JsonBaseNode> SerializeObjectThroughReflectionAsync(Type objectType, object objectValue)
		{
			// Create a new object node.
			var jsonObjectNode = new JsonObjectNode();
			try
			{
				var membersInfo = GetFieldsAndProperties(objectType);
				foreach (var memberInfo in membersInfo)
				{
					var memberName = string.Empty;
					Type memberType = null;
					object memberValue = null;

					switch (memberInfo.MemberType)
					{
						case MemberTypes.Field:
							var fieldInfo = (FieldInfo)memberInfo;
							memberName = fieldInfo.Name;
							memberType = fieldInfo.FieldType;
							memberValue = fieldInfo.GetValue(objectValue);
							break;
						case MemberTypes.Property:
							var propertyInfo = (PropertyInfo)memberInfo;
							memberName = propertyInfo.Name;
							memberType = propertyInfo.PropertyType;
							memberValue = propertyInfo.GetValue(objectValue);
							break;
						default:
							if (SaveToolboxPreferences.Instance.LoggingEnabled) Debug.LogWarning($"Cannot serialize member of type {memberInfo.MemberType}.");
							break;
					}

					// We don't need to null check here as it is done inside SerializeObject and also leaves an indication.
					jsonObjectNode.AddChild(await SerializeObjectAsync(memberType, memberValue, memberName));
				}

				if (SaveToolboxPreferences.Instance.LoggingEnabled && jsonObjectNode.Children.Count == 0) Debug.LogWarning($"Tried to serialize object but it was empty. Type was :{objectType}");
				return jsonObjectNode;
			}
			catch (Exception e)
			{
				Debug.LogError($"Failed Serialization with error: {e}");
			}

			throw new Exception("Failed to serialize json object.");
		}

		private async Task<JsonCompoundNode> SerializeCollectionAsync(Type objectType, object objectValue)
		{
			var isArray = objectType.IsArray;
			var isCollection = objectType.IsSerializableCollection();
			var arrayNode = new JsonArrayNode();

			if (isCollection)
			{
				var collection = (ICollection)objectValue;
				var isStack = objectType.IsStack();

				if (isStack)
				{
					var stackElements = new List<object>();
					// Reverse the stack by inserting to index 0 of a list, to adhere to a stacks FILO properties.
					foreach (var listObject in collection)
					{
						stackElements.Insert(0, listObject);
					}

					for (var index = 0; index < stackElements.Count; ++index)
					{
						var newChild = await SerializeObjectAsync(objectType.GetGenericArguments()[0], stackElements[index]);
						arrayNode.AddChild(newChild);
					}
				}
				else
				{
					foreach (var listObject in collection)
					{
						var newChild = await SerializeObjectAsync(objectType.GetGenericArguments()[0], listObject);
						arrayNode.AddChild(newChild);
					}
				}
			}

			if (isArray)
			{
				var array = (Array)objectValue;
				foreach (var arrayObject in array)
				{
					var newChild = await SerializeObjectAsync(objectType.GetElementType(), arrayObject);
					arrayNode.AddChild(newChild);
				}
			}

			return arrayNode;
		}

		private bool ShouldSerializeType(Type expectedType, Type actualType)
		{
			if (expectedType == null || expectedType != actualType)
			{
				return true;
			}

			return false;
		}

		private void SerializeTypeIfNecessary(Type expectedType, Type actualType, JsonBaseNode targetNode)
		{
			if (ShouldSerializeType(expectedType, actualType))
			{
				targetNode.Type = actualType.AssemblyQualifiedName;
			}
		}

		private JsonBaseNode SerializeObjectThroughReflection(Type objectType, object objectValue)
		{
			// Create a new object node.
			var jsonObjectNode = new JsonObjectNode();
			try
			{
				var membersInfo = GetFieldsAndProperties(objectType);
				foreach (var memberInfo in membersInfo)
				{
					var memberName = string.Empty;
					Type memberType = null;
					object memberValue = null;

					switch (memberInfo.MemberType)
					{
						case MemberTypes.Field:
							var fieldInfo = (FieldInfo)memberInfo;
							memberName = fieldInfo.Name;
							memberType = fieldInfo.FieldType;
							memberValue = fieldInfo.GetValue(objectValue);
							break;
						case MemberTypes.Property:
							var propertyInfo = (PropertyInfo)memberInfo;
							memberName = propertyInfo.Name;
							memberType = propertyInfo.PropertyType;
							memberValue = propertyInfo.GetValue(objectValue);
							break;
						default:
							if (SaveToolboxPreferences.Instance.LoggingEnabled) Debug.LogWarning($"Cannot serialize member of type {memberInfo.MemberType}.");
							break;
					}

					// We don't need to null check here as it is done inside SerializeObject and also leaves an indication.
					jsonObjectNode.AddChild(SerializeObject(memberType, memberValue, memberName));
				}

				if (SaveToolboxPreferences.Instance.LoggingEnabled && jsonObjectNode.Children.Count == 0) Debug.LogWarning($"Tried to serialize object but it was empty. Type was :{objectType}");
				return jsonObjectNode;
			}
			catch (Exception e)
			{
				Debug.LogError($"Failed Serialization with error: {e}");
			}

			throw new Exception("Failed to serialize json object.");
		}

		protected JsonCompoundNode SerializeCollection(Type objectType, object objectValue)
		{
			var isArray = objectType.IsArray;
			var isCollection = objectType.IsSerializableCollection();
			var arrayNode = new JsonArrayNode();

			if (isCollection)
			{
				var collection = (ICollection)objectValue;
				var isStack = objectType.IsStack();

				if (isStack)
				{
					var stackElements = new List<object>();
					// Reverse the stack by inserting to index 0 of a list, to adhere to a stacks FILO properties.
					foreach (var listObject in collection)
					{
						stackElements.Insert(0, listObject);
					}

					for (var index = 0; index < stackElements.Count; ++index)
					{
						var newChild = SerializeObject(objectType.GetGenericArguments()[0], stackElements[index]);
						arrayNode.AddChild(newChild);
					}
				}
				else
				{
					foreach (var listObject in collection)
					{
						var newChild = SerializeObject(objectType.GetGenericArguments()[0], listObject);
						arrayNode.AddChild(newChild);
					}
				}
			}

			if (isArray)
			{
				var array = (Array)objectValue;
				foreach (var arrayObject in array)
				{
					var newChild = SerializeObject(objectType.GetElementType(), arrayObject);
					arrayNode.AddChild(newChild);
				}
			}

			return arrayNode;
		}

		private T DeserializeObject<T>(JsonBaseNode targetNode)
		{
			var newObject = DeserializeObject(typeof(T), targetNode);
			return (T)newObject;
		}

		private object DeserializeObject(Type expectedType, JsonBaseNode targetNode)
		{
			var targetType = expectedType;

			// Handle if it is a null object.
			if (targetNode is JsonObjectNode objectNode && objectNode.Children.Count == 0)
			{
				return expectedType.IsValueType ? Activator.CreateInstance(expectedType) : default;
			}

			// If the type was serialized, target this once instead.
			var serializedCustomType = !string.IsNullOrEmpty(targetNode.Type);
			if (serializedCustomType)
			{
				targetType = StbSerializationUtilities.GetType(targetNode.Type);
			}

			// If the type is null it cannot be deserialized correctly, throw.
			if (targetType == null) throw new Exception("ERROR: Tried to deserialize a null type. Have assemblies changed?");

			// If it is a primitive type, the node should be a simple node.
			if (StbSerializationUtilities.IsPrimitiveType(targetType))
			{
				return DeserializePrimitive(targetType, targetNode);
			}
			// If the custom serializer has custom serializer functionality for this type, use that.
			if (CanSerialize(targetType) && targetNode is JsonCompoundNode compoundJsonNode)
			{
				return Deserialize(targetType, compoundJsonNode);
			}
			// Otherwise use reflection to find field and properties on the type that match data in the node.
			var isCollection = targetType.IsArray || targetType.IsSerializableCollection();

			if (isCollection)
			{
				return DeserializeCollection(targetType, targetNode);
			}

			// Use selection to deserialize the object.
			return DeserializeObjectUsingReflection(targetType, targetNode);
		}

		private object DeserializePrimitive(Type type, JsonBaseNode baseNode)
		{
			if (baseNode is JsonSimpleNode simpleNode)
			{
				if (type == typeof(bool)) return simpleNode.AsBool();
				if (type == typeof(string)) return simpleNode.AsString();
				if (type == typeof(char)) return simpleNode.AsChar();
				if (type == typeof(float)) return simpleNode.AsFloat();
				if (type == typeof(double)) return simpleNode.AsDouble();
				if (type == typeof(int)) return simpleNode.AsInt();
				if (type == typeof(uint)) return simpleNode.AsUint();
				if (type == typeof(long)) return simpleNode.AsLong();
				if (type == typeof(ulong)) return simpleNode.AsUlong();
				if (type == typeof(short)) return simpleNode.AsShort();
				if (type == typeof(ushort)) return simpleNode.AsUshort();
				if (type == typeof(byte)) return simpleNode.AsByte();
				if (type == typeof(sbyte)) return simpleNode.AsSbyte();
				if (type == typeof(decimal)) return simpleNode.AsDecimal();
			}

			if (baseNode is JsonPrimitiveArrayNode arrayNode)
			{
				if (type == typeof(bool[])) return arrayNode.AsBoolArray();
				if (type == typeof(string[])) return arrayNode.AsStringArray();
				if (type == typeof(char[])) return arrayNode.AsCharArray();
				if (type == typeof(float[])) return arrayNode.AsFloatArray();
				if (type == typeof(double[])) return arrayNode.AsDoubleArray();
				if (type == typeof(int[])) return arrayNode.AsIntArray();
				if (type == typeof(uint[])) return arrayNode.AsUIntArray();
				if (type == typeof(long[])) return arrayNode.AsLongArray();
				if (type == typeof(ulong[])) return arrayNode.AsULongArray();
				if (type == typeof(short[])) return arrayNode.AsShortArray();
				if (type == typeof(ushort[])) return arrayNode.AsUShortArray();
				if (type == typeof(byte[])) return arrayNode.AsByteArray();
				if (type == typeof(sbyte[])) return arrayNode.AsSByteArray();
				if (type == typeof(decimal[])) return arrayNode.AsDecimalArray();
			}

			throw new Exception($"Passed in type was not a primitive. {type}");
		}

		private object DeserializeObjectUsingReflection(Type targetType, JsonBaseNode targetNode)
		{
			try
			{
				if (targetType == typeof(object))
				{
					throw new Exception("Trying to load object that was serialized as type object -- something has gone wrong.");
				}

				var membersInfo = GetFieldsAndProperties(targetType);
				var newObjectInstance = Activator.CreateInstance(targetType);
				var objectNode = targetNode as JsonObjectNode;
				if (objectNode == null) throw new Exception("Tried to deserialize an object from non object node data.");

				// Now create instances of all the fields etc.
				foreach (var memberInfo in membersInfo)
				{
					switch (memberInfo.MemberType)
					{
						case MemberTypes.Field:
							var fieldInfo = (FieldInfo)memberInfo;
							var fieldType = fieldInfo.FieldType;
							fieldInfo.SetValue(newObjectInstance, DeserializeObject(fieldType, objectNode.GetChild(fieldInfo.Name)));
							break;
						case MemberTypes.Property:
							var propertyInfo = (PropertyInfo)memberInfo;
							var propertyType = propertyInfo.PropertyType;
							propertyInfo.SetValue(newObjectInstance, DeserializeObject(propertyType, objectNode.GetChild(propertyInfo.Name)));
							break;
					}
				}

				return newObjectInstance;
			}
			catch (Exception e)
			{
				Debug.LogError(e);
			}

			throw new Exception("Could not deserialize data.");
		}

		private object DeserializeCollection(Type collectionType, JsonBaseNode jsonBaseNode)
		{
			var isArray = collectionType.IsArray;

			var arrayNode = (JsonArrayNode)jsonBaseNode;

			if (isArray)
			{
				var arrayElementType = collectionType.GetElementType();
				if (arrayElementType == null) throw new Exception("Could not find correct array element type. Have assemblies changed?");

				var arrayInstance = Array.CreateInstance(arrayElementType, arrayNode.GetChildNodes().Count);

				var arrayElements = arrayNode.GetChildNodes();
				for (var index = 0; index < arrayElements.Count; index++)
				{
					var arrayElementNode = arrayElements[index];
					arrayInstance.SetValue(DeserializeObject(arrayElementType, arrayElementNode), index);
				}

				return arrayInstance;
			}

			var isList = collectionType.IsList();
			var isStack = collectionType.IsStack();
			var isQueue = collectionType.IsQueue();

			if (isList)
			{
				// Create list instance.
				var listElementType = collectionType.GetGenericArguments()[0];
				if (listElementType == null) throw new Exception("Could not find the correct list element type. Have assemblies changed?");

				var listInstance = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(listElementType));

				var listElements = arrayNode.GetChildNodes();

				// Populate list.
				for (var index = 0; index < listElements.Count; index++)
				{
					var arrayElementNode = listElements[index];
					listInstance.Add(DeserializeObject(listElementType, arrayElementNode));
				}

				// Return list instance.
				return listInstance;
			}

			if (isStack)
			{
				// Create list instance.
				var stackElementType = collectionType.GetGenericArguments()[0];
				var stackInstance = (ICollection)Activator.CreateInstance(typeof(Stack<>).MakeGenericType(stackElementType));
				var pushMethod = stackInstance.GetType().GetMethod("Push");

				var listElements = arrayNode.GetChildNodes();

				// Populate list.
				for (var index = 0; index < listElements.Count; index++)
				{
					pushMethod?.Invoke(stackInstance, new object[] { DeserializeObject(stackElementType, listElements[index]) });
				}

				// Return list instance.
				return stackInstance;
			}

			if (isQueue)
			{
				// Create list instance.
				var queueElementType = collectionType.GetGenericArguments()[0];
				var queueInstance = (ICollection)Activator.CreateInstance(typeof(Queue<>).MakeGenericType(queueElementType));
				var enqueueMethod = queueInstance.GetType().GetMethod("Enqueue");

				var listElements = arrayNode.GetChildNodes();

				// Populate list.
				for (var index = 0; index < listElements.Count; index++)
				{
					enqueueMethod?.Invoke(queueInstance, new object[] { DeserializeObject(queueElementType, listElements[index]) });
				}

				// Return list instance.
				return queueInstance;
			}

			throw new Exception("Tried to deserialize a collection that wasn't an array or a single argument list.");
		}

		private async Task<T> DeserializeObjectAsync<T>(JsonBaseNode targetNode)
		{
			return (T)await DeserializeObjectAsync(typeof(T), targetNode);;
		}

		private async Task<object> DeserializeObjectAsync(Type expectedType, JsonBaseNode targetNode)
		{
			var targetType = expectedType;

			// Handle if it is a null object.
			if (targetNode is JsonObjectNode objectNode && objectNode.Children.Count == 0)
			{
				return expectedType.IsValueType ? Activator.CreateInstance(expectedType) : default;
			}

			// If the type was serialized, target this once instead.
			var serializedCustomType = !string.IsNullOrEmpty(targetNode.Type);
			if (serializedCustomType)
			{
				targetType = StbSerializationUtilities.GetType(targetNode.Type);
			}

			// If the type is null it cannot be deserialized correctly, throw.
			if (targetType == null) throw new Exception("ERROR: Tried to deserialize a null type. Have assemblies changed?");

			await CheckFrameTime();

			// If it is a primitive type, the node should be a simple node.
			if (StbSerializationUtilities.IsPrimitiveType(targetType))
			{
				return DeserializePrimitive(targetType, targetNode);
			}
			// If the custom serializer has custom serializer functionality for this type, use that.
			if (CanSerialize(targetType) && targetNode is JsonCompoundNode compoundJsonNode)
			{
				return Deserialize(targetType, compoundJsonNode);
			}

			// Otherwise use reflection to find field and properties on the type that match data in the node.
			var isCollection = targetType.IsArray || targetType.IsSerializableCollection();

			if (isCollection)
			{
				return await DeserializeCollectionAsync(targetType, targetNode);
			}

			// Use selection to deserialize the object.
			return await DeserializeObjectUsingReflectionAsync(targetType, targetNode);
		}

		private async Task<object> DeserializeObjectUsingReflectionAsync(Type targetType, JsonBaseNode targetNode)
		{
			try
			{
				if (targetType == typeof(object))
				{
					throw new Exception("Trying to load object that was serialized as type object -- something has gone wrong.");
				}

				var membersInfo = GetFieldsAndProperties(targetType);
				var newObjectInstance = Activator.CreateInstance(targetType);
				var objectNode = targetNode as JsonObjectNode;
				if (objectNode == null) throw new Exception("Tried to deserialize an object from non object node data.");

				// Now create instances of all the fields etc.
				foreach (var memberInfo in membersInfo)
				{
					switch (memberInfo.MemberType)
					{
						case MemberTypes.Field:
							var fieldInfo = (FieldInfo)memberInfo;
							var fieldType = fieldInfo.FieldType;
							fieldInfo.SetValue(newObjectInstance, await DeserializeObjectAsync(fieldType, objectNode.GetChild(fieldInfo.Name)));
							break;
						case MemberTypes.Property:
							var propertyInfo = (PropertyInfo)memberInfo;
							var propertyType = propertyInfo.PropertyType;
							propertyInfo.SetValue(newObjectInstance, await DeserializeObjectAsync(propertyType, objectNode.GetChild(propertyInfo.Name)));
							break;
					}
				}

				return newObjectInstance;
			}
			catch (Exception e)
			{
				Debug.LogError(e);
			}

			throw new Exception("Could not deserialize data.");
		}

		private async Task<object> DeserializeCollectionAsync(Type collectionType, JsonBaseNode jsonBaseNode)
		{
			var isArray = collectionType.IsArray;
			var arrayNode = (JsonArrayNode)jsonBaseNode;

			if (isArray)
			{
				var arrayElementType = collectionType.GetElementType();
				if (arrayElementType == null) throw new Exception("Could not find correct array element type. Have assemblies changed?");

				var arrayInstance = Array.CreateInstance(arrayElementType, arrayNode.GetChildNodes().Count);

				var arrayElements = arrayNode.GetChildNodes();
				for (var index = 0; index < arrayElements.Count; index++)
				{
					var arrayElementNode = arrayElements[index];
					arrayInstance.SetValue(await DeserializeObjectAsync(arrayElementType, arrayElementNode), index);
				}

				return arrayInstance;
			}

			var isList = collectionType.IsList();
			var isStack = collectionType.IsStack();
			var isQueue = collectionType.IsQueue();

			if (isList)
			{
				// Create list instance.
				var listElementType = collectionType.GetGenericArguments()[0];
				if (listElementType == null) throw new Exception("Could not find the correct list element type. Have assemblies changed?");

				var listInstance = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(listElementType));

				var listElements = arrayNode.GetChildNodes();

				// Populate list.
				for (var index = 0; index < listElements.Count; index++)
				{
					var arrayElementNode = listElements[index];
					listInstance.Add(await DeserializeObjectAsync(listElementType, arrayElementNode));
				}

				// Return list instance.
				return listInstance;
			}

			if (isStack)
			{
				// Create list instance.
				var stackElementType = collectionType.GetGenericArguments()[0];
				var stackInstance = (ICollection)Activator.CreateInstance(typeof(Stack<>).MakeGenericType(stackElementType));
				var pushMethod = stackInstance.GetType().GetMethod("Push");

				var listElements = arrayNode.GetChildNodes();

				// Populate list.
				for (var index = 0; index < listElements.Count; index++)
				{
					pushMethod?.Invoke(stackInstance, new object[] { await DeserializeObjectAsync(stackElementType, listElements[index]) });
				}

				// Return list instance.
				return stackInstance;
			}

			if (isQueue)
			{
				// Create list instance.
				var queueElementType = collectionType.GetGenericArguments()[0];
				var queueInstance = (ICollection)Activator.CreateInstance(typeof(Queue<>).MakeGenericType(queueElementType));
				var enqueueMethod = queueInstance.GetType().GetMethod("Enqueue");

				var listElements = arrayNode.GetChildNodes();

				// Populate list.
				for (var index = 0; index < listElements.Count; index++)
				{
					enqueueMethod?.Invoke(queueInstance, new object[] { await DeserializeObjectAsync(queueElementType, listElements[index]) });
				}

				// Return list instance.
				return queueInstance;
			}


			throw new Exception("Tried to deserialize a collection that wasn't an array or a single argument list.");
		}

		/// <summary>
		/// Determines if a type can be serialized by this serializer. Can be extended.
		/// </summary>
		/// <param nodeName="type">The type to be checked if it can be serialized.</param>
		/// <returns>If it can be serialized.</returns>
		protected virtual bool CanSerialize(Type type)
		{
			return type == typeof(Vector2) ||
				type == typeof(Vector3) ||
				type == typeof(Vector4) ||
				type == typeof(Color) ||
				type == typeof(Quaternion) ||
				type == typeof(Vector2[]) ||
				type == typeof(Vector3[]) ||
				type == typeof(Vector4[]) ||
				type == typeof(Color[]) ||
				type == typeof(Quaternion[]) ||
				type == typeof(Texture2D) ||
				type.IsEnum ||
				type == typeof(StbByteWriter) ||
				typeof(ScriptableObject).IsAssignableFrom(type) ||
				type.IsGenericType && type.GetGenericTypeDefinition() == typeof(SaveEntityReference<>);
		}

		/// <summary>
		/// The serialization function for object instances. By default handles common unity objects like Vector3s and Quaternions.
		/// Can be extended for more functionality.
		/// </summary>
		/// <param nodeName="objectValue">The object to be serialized.</param>
		/// <returns>A JsonBaseNode which stores the serialized data.</returns>
		/// <exception cref="Exception">Could not serialize exception.</exception>
		protected virtual JsonBaseNode Serialize(object objectValue)
		{
			var objectNode = new JsonObjectNode();
			if (objectValue is Vector2 vector2)
			{
				objectNode.AddSimpleChild("X:", vector2.x);
				objectNode.AddSimpleChild("Y:", vector2.y);
				return objectNode;
			}

			if (objectValue is Vector3 vector3)
			{
				objectNode.AddSimpleChild("X:", vector3.x);
				objectNode.AddSimpleChild("Y:", vector3.y);
				objectNode.AddSimpleChild("Z:", vector3.z);
				return objectNode;
			}

			if (objectValue is Vector4 vector4)
			{
				objectNode.AddSimpleChild("X:", vector4.x);
				objectNode.AddSimpleChild("Y:", vector4.y);
				objectNode.AddSimpleChild("Z:", vector4.z);
				objectNode.AddSimpleChild("W:", vector4.w);
				return objectNode;
			}

			if (objectValue is Color color)
			{
				objectNode.AddSimpleChild("R:", color.r);
				objectNode.AddSimpleChild("G:", color.g);
				objectNode.AddSimpleChild("B:", color.b);
				objectNode.AddSimpleChild("A:", color.a);
				return objectNode;
			}

			if (objectValue is Quaternion quaternion)
			{
				objectNode.AddSimpleChild("X:", quaternion.x);
				objectNode.AddSimpleChild("Y:", quaternion.y);
				objectNode.AddSimpleChild("Z:", quaternion.z);
				objectNode.AddSimpleChild("W:", quaternion.w);
				return objectNode;
			}

			if (objectValue is Vector2[] vector2Array)
			{
				var vector2ArrayNode = new JsonArrayNode();
				for (var i = 0; i < vector2Array.Length; ++i)
				{
					vector2ArrayNode.AddChild(Serialize(vector2Array[i]));
				}

				return vector2ArrayNode;
			}

			if (objectValue is Vector3[] vector3Array)
			{
				var vector3ArrayNode = new JsonArrayNode();
				for (var i = 0; i < vector3Array.Length; ++i)
				{
					vector3ArrayNode.AddChild(Serialize(vector3Array[i]));
				}

				return vector3ArrayNode;
			}

			if (objectValue is Vector4[] vector4Array)
			{
				var vector4ArrayNode = new JsonArrayNode();
				for (var i = 0; i < vector4Array.Length; ++i)
				{
					vector4ArrayNode.AddChild(Serialize(vector4Array[i]));
				}

				return vector4ArrayNode;
			}

			if (objectValue is Color[] colorArray)
			{
				var colorArrayNode = new JsonArrayNode();
				for (var i = 0; i < colorArray.Length; ++i)
				{
					colorArrayNode.AddChild(Serialize(colorArray[i]));
				}

				return colorArrayNode;
			}

			if (objectValue is Quaternion[] quaternionArray)
			{
				var quaternionArrayNode = new JsonArrayNode();
				for (var i = 0; i < quaternionArray.Length; ++i)
				{
					quaternionArrayNode.AddChild(Serialize(quaternionArray[i]));
				}

				return quaternionArrayNode;
			}

			if (objectValue is Texture2D texture2D)
			{
				var textureArrayNode = SerializeCollection(typeof(byte[]), texture2D.EncodeToPNG());
				textureArrayNode.Name = "Bytes:";
				objectNode.AddChild(textureArrayNode);
				return objectNode;
			}

			if (objectValue is Enum targetEnum)
			{
				objectNode.AddSimpleChild("Enum:", Convert.ToInt32(targetEnum));
				return objectNode;
			}

			if (objectValue is StbByteWriter stbByteWriter)
			{
				var arrayNode = SerializeCollection(typeof(byte[]), stbByteWriter.GetByteArray());
				arrayNode.Name = "Bytes:";
				objectNode.AddChild(arrayNode);
				return objectNode;
			}

			if (typeof(ScriptableObject).IsAssignableFrom(objectValue.GetType()))
			{
				var scriptableObject = (ScriptableObject)objectValue;
				var isSaveable = ScriptableObjectDatabase.Instance.TryGetScriptableObjectGuid(scriptableObject, out var scriptableObjectAssetGuid);
				objectNode.AddSimpleChild("Is Saveable:", isSaveable);

				if (isSaveable)
				{
					objectNode.AddSimpleChild("Identifier:", scriptableObjectAssetGuid);
				}

				return objectNode;
			}

			if (objectValue.GetType().IsGenericType && objectValue.GetType().GetGenericTypeDefinition() == typeof(SaveEntityReference<>))
			{
				var objectType = objectValue.GetType();

				var referenceType = objectType.GetGenericArguments()[0];
				objectNode.AddSimpleChild("Generic Type:", referenceType.AssemblyQualifiedName);

				var identifierProperty = objectType.GetProperty("Identifier");
				var identifierString = (string)identifierProperty?.GetValue(objectValue);
				objectNode.AddSimpleChild("Target SaveIdentifier:", identifierString);

				return objectNode;
			}

			throw new Exception($"Could not serialize object of type: {objectValue.GetType()}");
		}

		private async Task CheckFrameTime()
		{
			currentFrameTime = Time.realtimeSinceStartup;
			var difference = currentFrameTime - startFrameTime;
			if (difference > 1f / SaveToolboxPreferences.Instance.LowestAcceptableLoadingFrameRate)
			{
				await Task.Yield();
				startFrameTime = Time.realtimeSinceStartup;
			}
		}

		/// <summary>
		/// Gets all the MemberInfos off of fields and properties of a specific type. Can be overridden to allow for different functionality.
		/// </summary>
		/// <param nodeName="objectType">The type to get the MemberInfos off of.</param>
		/// <returns>The list of MemberInfos.</returns>
		protected virtual List<MemberInfo> GetFieldsAndProperties(Type objectType)
		{
			return StbUtilities.GetAllSerializableFieldsAndProperties(objectType, serializationSettings);
		}

		public T Deserialize<T>(JsonCompoundNode jsonCompoundNode)
		{
			return (T)Deserialize(typeof(T), jsonCompoundNode);
		}

		/// <summary>
		/// The deserialize function used to define how objects instances of certain types should be deserialzied.
		/// Can be overridden for custom functionality and for extension to allow more custom deserialization.
		/// </summary>
		/// <param name="type">The type to be deserialized.</param>
		/// <param name="jsonCompoundNode">The Compound node that contains the data of what should be deserialized.</param>
		/// <returns>The deserialized object instance.</returns>
		/// <exception cref="Exception">Could not deserialize exception.</exception>
		protected virtual object Deserialize(Type type, JsonCompoundNode jsonCompoundNode)
		{
			if (type == typeof(Vector2))
			{
				var x = (JsonSimpleNode)jsonCompoundNode.GetChild("X:");
				var y = (JsonSimpleNode)jsonCompoundNode.GetChild("Y:");
				if (x.Value is int xInt && y.Value is int yInt)
				{
					return new Vector2(xInt, yInt);
				}

				return new Vector2(x.AsFloat(), y.AsFloat());
			}

			if (type == typeof(Vector3))
			{
				var x = (JsonSimpleNode)jsonCompoundNode.GetChild("X:");
				var y = (JsonSimpleNode)jsonCompoundNode.GetChild("Y:");
				var z = (JsonSimpleNode)jsonCompoundNode.GetChild("Z:");
				if (x.Value is int xInt && y.Value is int yInt && z.Value is int zInt)
				{
					return new Vector3(xInt, yInt, zInt);
				}

				return new Vector3(x.AsFloat(), y.AsFloat(), z.AsFloat());
			}

			if (type == typeof(Vector4))
			{
				var x = (JsonSimpleNode)jsonCompoundNode.GetChild("X:");
				var y = (JsonSimpleNode)jsonCompoundNode.GetChild("Y:");
				var z = (JsonSimpleNode)jsonCompoundNode.GetChild("Z:");
				var w = (JsonSimpleNode)jsonCompoundNode.GetChild("W:");

				if (x.Value is int xInt && y.Value is int yInt && z.Value is int zInt && w.Value is int wInt)
				{
					return new Vector4(xInt, yInt, zInt, wInt);
				}

				return new Vector4(x.AsFloat(), y.AsFloat(), z.AsFloat(), w.AsFloat());
			}

			if (type == typeof(Color))
			{
				var r = (JsonSimpleNode)jsonCompoundNode.GetChild("R:");
				var g = (JsonSimpleNode)jsonCompoundNode.GetChild("G:");
				var b = (JsonSimpleNode)jsonCompoundNode.GetChild("B:");
				var a = (JsonSimpleNode)jsonCompoundNode.GetChild("A:");
				return new Color(r.AsFloat(), g.AsFloat(), b.AsFloat(), a.AsFloat());
			}

			if (type == typeof(Quaternion))
			{
				var x = (JsonSimpleNode)jsonCompoundNode.GetChild("X:");
				var y = (JsonSimpleNode)jsonCompoundNode.GetChild("Y:");
				var z = (JsonSimpleNode)jsonCompoundNode.GetChild("Z:");
				var w = (JsonSimpleNode)jsonCompoundNode.GetChild("W:");

				if (x.Value is int xInt && y.Value is int yInt && z.Value is int zInt && w.Value is int wInt)
				{
					return new Quaternion(xInt, yInt, zInt, wInt);
				}

				return new Quaternion(x.AsFloat(), y.AsFloat(), z.AsFloat(), w.AsFloat());
			}

			if (type == typeof(Vector2[]))
			{
				var returnArray = new Vector2[jsonCompoundNode.Children.Count];
				for (var i = 0; i < returnArray.Length; i++)
				{
					returnArray[i] = Deserialize<Vector2>((JsonCompoundNode)jsonCompoundNode.Children[i]);
				}

				return returnArray;
			}

			if (type == typeof(Vector3[]))
			{
				var returnArray = new Vector3[jsonCompoundNode.Children.Count];
				for (var i = 0; i < returnArray.Length; i++)
				{
					returnArray[i] = Deserialize<Vector3>((JsonCompoundNode)jsonCompoundNode.Children[i]);
				}

				return returnArray;
			}

			if (type == typeof(Vector4[]))
			{
				var returnArray = new Vector4[jsonCompoundNode.Children.Count];
				for (var i = 0; i < returnArray.Length; i++)
				{
					returnArray[i] = Deserialize<Vector4>((JsonCompoundNode)jsonCompoundNode.Children[i]);
				}

				return returnArray;
			}

			if (type == typeof(Color[]))
			{
				var returnArray = new Color[jsonCompoundNode.Children.Count];
				for (var i = 0; i < returnArray.Length; i++)
				{
					returnArray[i] = Deserialize<Color>((JsonCompoundNode)jsonCompoundNode.Children[i]);
				}

				return returnArray;
			}

			if (type == typeof(Quaternion[]))
			{
				var returnArray = new Quaternion[jsonCompoundNode.Children.Count];
				for (var i = 0; i < returnArray.Length; i++)
				{
					returnArray[i] = Deserialize<Quaternion>((JsonCompoundNode)jsonCompoundNode.Children[i]);
				}

				return returnArray;
			}

			if (type == typeof(Texture2D))
			{
				var child = jsonCompoundNode.GetChild("Bytes:");
				var bytesNode = (JsonArrayNode)child;
				if (bytesNode.TryGetPrimitiveChildrenOfType<byte>(out var bytes))
				{
					var newTexture = new Texture2D(1, 1);
					newTexture.LoadImage(bytes.ToArray());
					return newTexture;
				}

				return null;
			}

			if (type.IsEnum)
			{
				var enumIntValue = (JsonSimpleNode)jsonCompoundNode.GetChild("Enum:");
				return Enum.ToObject(type, int.Parse((string)enumIntValue.Value));
			}

			if (type == typeof(StbByteWriter))
			{
				var child = jsonCompoundNode.GetChild("Bytes:");
				var bytesNode = (JsonArrayNode)child;
				if (bytesNode.TryGetPrimitiveChildrenOfType<byte>(out var bytes))
				{
					return new StbByteReader(bytes.ToArray());
				}
			}

			if (typeof(ScriptableObject).IsAssignableFrom(type))
			{
				var isSaveable = (JsonSimpleNode)jsonCompoundNode.GetChild("Is Saveable:");
				if (isSaveable.AsBool())
				{
					var identifier = (JsonSimpleNode)jsonCompoundNode.GetChild("Identifier:");
					if (ScriptableObjectDatabase.Instance.TryGetScriptableObject(identifier.AsString(), out var scriptableObject))
					{
						return scriptableObject;
					}
				}
				return null;
			}

			if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(SaveEntityReference<>))
			{
				var genericTypeNode = (JsonSimpleNode)jsonCompoundNode.GetChild("Generic Type:");
				var genericTypeString = (string)genericTypeNode.Value;

				var referenceNode = (JsonSimpleNode)jsonCompoundNode.GetChild("Target SaveIdentifier:");
				var identifierString = (string)referenceNode.Value;

				if (StbUtilities.TryGetISaveDataEntity(identifierString, out var iSaveDataEntity))
				{
					var referenceType = StbSerializationUtilities.GetType(genericTypeString);
					var instanceType = typeof(SaveEntityReference<>).MakeGenericType(referenceType);
					var entityReference = Activator.CreateInstance(instanceType);

					var entityReferenceProperty = type.GetProperty("EntityReference");
					if (iSaveDataEntity is MonoBehaviour saveEntityMonoBehaviour)
					{
						entityReferenceProperty?.SetValue(entityReference, saveEntityMonoBehaviour);
					}

					return entityReference;
				}

				Debug.LogError($"Could not find a instance of an ISaveDataEntity with the identifier: {identifierString}");
				return default;
			}


			throw new Exception($"Could not serialize object of type: {type}");
		}
	}
}