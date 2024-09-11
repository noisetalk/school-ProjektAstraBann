using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using SaveToolbox.Runtime.BasicSaveableMonoBehaviours;
using SaveToolbox.Runtime.Core;
using SaveToolbox.Runtime.Core.ScriptableObjects;
using SaveToolbox.Runtime.Utils;
using UnityEngine;

namespace SaveToolbox.Runtime.Serialization.Binary
{
	public class StbBinarySerializer
	{
		public StbSerializationSettings serializationSettings = new StbSerializationSettings();

		private float currentFrameTime;
		private float startFrameTime;

		/// <summary>
		/// Serializes an object into binary data which is represented as a byte array.
		/// </summary>
		/// <param name="serializableObject">The object to be serialized into binary data.</param>
		/// <returns>A byte array created from the data on the serializable object parameter.</returns>
		public byte[] ToBinary(object serializableObject)
		{
			var stbByteWriter = new StbByteWriter();
			// We pass in a null expected type so it serializes it.
			SerializeObject(null, serializableObject, stbByteWriter);
			return stbByteWriter.GetByteArray();
		}

		/// <summary>
		/// Serializes an object into binary data asynchronously which is represented as a byte array.
		/// </summary>
		/// <param name="serializableObject">The object to be serialized into binary data.</param>
		/// <returns>A byte array created from the data on the serializable object parameter.</returns>
		public async Task<byte[]> ToBinaryAsync(object serializableObject)
		{
			var stbByteWriter = new StbByteWriter();
			// We pass in a null expected type so it serializes it.
			await SerializeObjectAsync(null, serializableObject, stbByteWriter);
			return stbByteWriter.GetByteArray();
		}

		/// <summary>
		/// Creates an object of type T from the binary data that is passed in the parameter.
		/// </summary>
		/// <param name="uncompressedData">The binary byte array data to be used to create the object of type T.</param>
		/// <typeparam name="T">The object to be created type.</typeparam>
		/// <returns>An object of type T.</returns>
		public T FromBinary<T>(byte[] uncompressedData)
		{
			var currentStbByteWriter = new StbByteReader(uncompressedData);
			return DeserializeObject<T>(currentStbByteWriter);
		}

		/// <summary>
		/// Creates an object of type T asynchronously from the binary data that is passed in the parameter.
		/// </summary>
		/// <param name="uncompressedData">The binary byte array data to be used to create the object of type T.</param>
		/// <typeparam name="T">The object to be created type.</typeparam>
		/// <returns>An object of type T.</returns>
		public async Task<T> FromBinaryAsync<T>(byte[] uncompressedData)
		{
			var currentStbByteWriter = new StbByteReader(uncompressedData);
			return await DeserializeObjectAsync<T>(currentStbByteWriter);
		}

		private void SerializeObject(Type expectedType, object objectValue, StbByteWriter byteWriter)
		{
			// If the object is null, place a indicator byte in so when it is deserialized it knows to not look for a value.
			if (objectValue == null || objectValue.Equals(null))
			{
				byteWriter.Write((byte)0);
				return;
			}

			// We compare this against the expected type to determine if we should serialize the type.
			var actualType = objectValue.GetType();

			// Serialize it with a marker as well determining if you serialized the type data.
			SerializeTypeIfNecessary(expectedType, actualType, byteWriter);

			// If it is a primitive serialize it.
			if (StbSerializationUtilities.IsPrimitiveType(actualType))
			{
				SerializePrimitive(actualType, objectValue, byteWriter);
			}
			// If it is an object type with custom serialization, use that.
			else if (CanSerialize(actualType))
			{
				Serialize(objectValue, byteWriter);
			}
			else
			{
				var isCollection = actualType.IsSerializableCollection() || actualType.IsArray;

				// If it is a collection serialize it differently.
				if (isCollection)
				{
					SerializeCollection(actualType, objectValue, byteWriter);
				}
				// Otherwise serialize it by using selection.
				else
				{
					SerializeObjectThroughReflection(actualType, objectValue, byteWriter);
				}
			}
		}

		private async Task SerializeObjectAsync(Type expectedType, object objectValue, StbByteWriter byteWriter)
		{
			// If the object is null, place a indicator byte in so when it is deserialized it knows to not look for a value.
			if (objectValue == null || objectValue.Equals(null))
			{
				byteWriter.Write((byte)0);
				await CheckFrameTime();
				return;
			}

			// We compare this against the expected type to determine if we should serialize the type.
			var actualType = objectValue.GetType();

			// Serialize it with a marker as well determining if you serialized the type data.
			SerializeTypeIfNecessary(expectedType, actualType, byteWriter);

			// If it is a primitive serialize it.
			if (StbSerializationUtilities.IsPrimitiveType(actualType))
			{
				SerializePrimitive(actualType, objectValue, byteWriter);
			}
			// If it is an object type with custom serialization, use that.
			else if (CanSerialize(actualType))
			{
				Serialize(objectValue, byteWriter);
			}
			else
			{
				var isCollection = actualType.IsSerializableCollection() || actualType.IsArray;

				// If it is a collection serialize it differently.
				if (isCollection)
				{
					await SerializeCollectionAsync(actualType, objectValue, byteWriter);
				}
				// Otherwise serialize it by using selection.
				else
				{
					await SerializeObjectThroughReflectionAsync(actualType, objectValue, byteWriter);
				}
			}

			await CheckFrameTime();
		}

		private void SerializeObjectThroughReflection(Type objectType, object objectValue, StbByteWriter byteWriter)
		{
			try
			{
				var membersInfo = GetFieldsAndProperties(objectType);
				foreach (var memberInfo in membersInfo)
				{
					Type memberType = null;
					object memberValue = null;

					switch (memberInfo.MemberType)
					{
						case MemberTypes.Field:
							var fieldInfo = (FieldInfo)memberInfo;
							memberValue = fieldInfo.GetValue(objectValue);
							memberType = fieldInfo.FieldType;
							break;
						case MemberTypes.Property:
							var propertyInfo = (PropertyInfo)memberInfo;
							memberValue = propertyInfo.GetValue(objectValue);
							memberType = propertyInfo.PropertyType;
							break;
						default:
							if (SaveToolboxPreferences.Instance.LoggingEnabled) Debug.LogWarning($"Cannot serialize member of expectedType {memberInfo.MemberType}.");
							break;
					}

					// We don't need to null check here as it is done inside SerializeObject and also leaves an indication.
					SerializeObject(memberType, memberValue, byteWriter);
				}
			}
			catch (Exception e)
			{
				Debug.LogError($"Failed Serialization with error: {e}");
			}
		}

		private async Task SerializeObjectThroughReflectionAsync(Type objectType, object objectValue, StbByteWriter byteWriter)
		{
			try
			{
				var membersInfo = GetFieldsAndProperties(objectType);
				foreach (var memberInfo in membersInfo)
				{
					Type memberType = null;
					object memberValue = null;

					switch (memberInfo.MemberType)
					{
						case MemberTypes.Field:
							var fieldInfo = (FieldInfo)memberInfo;
							memberValue = fieldInfo.GetValue(objectValue);
							memberType = fieldInfo.FieldType;
							break;
						case MemberTypes.Property:
							var propertyInfo = (PropertyInfo)memberInfo;
							memberValue = propertyInfo.GetValue(objectValue);
							memberType = propertyInfo.PropertyType;
							break;
						default:
							if (SaveToolboxPreferences.Instance.LoggingEnabled) Debug.LogWarning($"Cannot serialize member of expectedType {memberInfo.MemberType}.");
							break;
					}

					// We don't need to null check here as it is done inside SerializeObject and also leaves an indication.
					await SerializeObjectAsync(memberType, memberValue, byteWriter);
				}
			}
			catch (Exception e)
			{
				Debug.LogError($"Failed Serialization with error: {e}");
			}
		}

		private void SerializeTypeIfNecessary(Type expectedType, Type actualType, StbByteWriter byteWriter)
		{
			if (ShouldSerializeType(expectedType, actualType))
			{
				byteWriter.Write((byte)1);
				byteWriter.Write(actualType.AssemblyQualifiedName);
				return;
			}

			byteWriter.Write((byte)2);
		}

		private bool ShouldSerializeType(Type expectedType, Type actualType)
		{
			if (expectedType == null || expectedType != actualType)
			{
				return true;
			}

			return false;
		}

		private void SerializeCollection(Type objectType, object objectValue, StbByteWriter byteWriter)
		{
			var isArray = objectType.IsArray;
			var isCollection = objectType.IsSerializableCollection();

			// serialize the length and the expectedType.
			if (isCollection)
			{
				var collection = (ICollection)objectValue;
				var isStack = objectType.IsStack();

				// Serialize the list count.
				byteWriter.Write(collection.Count);

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
						SerializeObject(objectType.GetGenericArguments()[0], stackElements[index], byteWriter);
					}
				}
				else
				{
					foreach (var listObject in collection)
					{
						SerializeObject(objectType.GetGenericArguments()[0], listObject, byteWriter);
					}
				}
			}

			if (isArray)
			{
				var array = (Array)objectValue;
				// Serialize the array length.
				byteWriter.Write(array.Length);

				foreach (var arrayObject in array)
				{
					var value = arrayObject;
					SerializeObject(objectType.GetElementType(), value, byteWriter);
				}
			}
		}

		private async Task SerializeCollectionAsync(Type objectType, object objectValue, StbByteWriter byteWriter)
		{
			var isArray = objectType.IsArray;
			var isCollection = objectType.IsSerializableCollection();
			// Handle if it is a list, IEnumerable or an array.
			// serialize the length and the expectedType.
			if (isCollection)
			{
				var collection = (ICollection)objectValue;
				var isStack = objectType.IsStack();

				// Serialize the list count.
				byteWriter.Write(collection.Count);

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
						await SerializeObjectAsync(objectType.GetGenericArguments()[0], stackElements[index], byteWriter);
					}
				}
				else
				{
					foreach (var listObject in collection)
					{
						await SerializeObjectAsync(objectType.GetGenericArguments()[0], listObject, byteWriter);
					}
				}
			}

			if (isArray)
			{
				var array = (Array)objectValue;
				// Serialize the array length.
				byteWriter.Write(array.Length);

				foreach (var arrayObject in array)
				{
					var value = arrayObject;
					await SerializeObjectAsync(objectType.GetElementType(), value, byteWriter);
				}
			}
		}

		private T DeserializeObject<T>(StbByteReader byteReader)
		{
			var newObject = DeserializeObject(typeof(T), byteReader);
			return (T)newObject;
		}

		private object DeserializeObject(Type expectedType, StbByteReader byteReader)
		{
			// Read the indicator byte so we know if it is null, otherwise
			var indicatorByte = byteReader.ReadByte();

			// If it was a null value return a default value.
			if (indicatorByte == 0)
			{
				return expectedType.IsValueType ? Activator.CreateInstance(expectedType) : default;
			}

			// Set the target type to the expected type.
			var targetType = expectedType;

			// If the indicator byte is 1 this means the actual type has been serialized. Use this instead.
			if (indicatorByte == 1)
			{
				var typeAssemblyName = byteReader.ReadString();
				targetType = StbSerializationUtilities.GetType(typeAssemblyName);
			}

			if (targetType == null) throw new Exception("ERROR: Tried to deserialize a null type, have assemblies changed?");

			// Try deserialize the type if it is a primitive.
			if (StbSerializationUtilities.IsPrimitiveType(targetType))
			{
				return DeserializePrimitive(targetType, byteReader);
			}

			// Try deserialize an object that has custom deserialization.
			if (CanSerialize(targetType))
			{
				return Deserialize(targetType, byteReader);
			}

			// Deserialize the object if it is a collection.
			var isCollection = targetType.IsSerializableCollection() || targetType.IsArray;
			if (isCollection)
			{
				return DeserializeCollection(targetType, byteReader);
			}

			// Use selection to deserialize the object.
			return DeserializeObjectUsingReflection(targetType, byteReader);
		}

		private async Task<T> DeserializeObjectAsync<T>(StbByteReader byteReader)
		{
			return (T)await DeserializeObjectAsync(typeof(T), byteReader);
		}

		private async Task<object> DeserializeObjectAsync(Type expectedType, StbByteReader byteReader)
		{
			// Read the indicator byte so we know if it is null, otherwise
			var indicatorByte = byteReader.ReadByte();

			// If it was a null value return a default value.
			if (indicatorByte == 0)
			{
				await CheckFrameTime();
				return expectedType.IsValueType ? Activator.CreateInstance(expectedType) : default;
			}

			// Set the target type to the expected type.
			var targetType = expectedType;

			// If the indicator byte is 1 this means the actual type has been serialized. Use this instead.
			if (indicatorByte == 1)
			{
				var typeAssemblyName = byteReader.ReadString();
				targetType = StbSerializationUtilities.GetType(typeAssemblyName);
			}

			if (targetType == null) throw new Exception("ERROR: Tried to deserialize a null type, have assemblies changed?");

			await CheckFrameTime();

			// Try deserialize the type if it is a primitive.
			if (StbSerializationUtilities.IsPrimitiveType(targetType))
			{
				return DeserializePrimitive(targetType, byteReader);
			}

			// Try deserialize an object that has custom deserialization.
			if (CanSerialize(targetType))
			{
				return Deserialize(targetType, byteReader);
			}

			// Deserialize the object if it is a collection.
			var isCollection = targetType.IsArray || targetType.IsSerializableCollection();
			if (isCollection)
			{
				return await DeserializeCollectionAsync(targetType, byteReader);
			}

			// Use selection to deserialize the object.
			return await DeserializeObjectUsingReflectionAsync(targetType, byteReader);
		}

		private async Task<object> DeserializeObjectUsingReflectionAsync(Type targetType, StbByteReader byteReader)
		{
			try
			{
				if (targetType == typeof(object))
				{
					throw new Exception("Trying to load object that was serialized as type object -- something has gone wrong.");
				}

				var membersInfo = GetFieldsAndProperties(targetType);
				var newObjectInstance = Activator.CreateInstance(targetType);

				// Now create instances of all the fields etc.
				foreach (var memberInfo in membersInfo)
				{
					switch (memberInfo.MemberType)
					{
						case MemberTypes.Field:
							var fieldInfo = (FieldInfo)memberInfo;
							var fieldType = fieldInfo.FieldType;
							fieldInfo.SetValue(newObjectInstance, await DeserializeObjectAsync(fieldType, byteReader));
							break;
						case MemberTypes.Property:
							var propertyInfo = (PropertyInfo)memberInfo;
							var propertyType = propertyInfo.PropertyType;
							propertyInfo.SetValue(newObjectInstance, await DeserializeObjectAsync(propertyType, byteReader));
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

		private async Task<object> DeserializeCollectionAsync(Type collectionType, StbByteReader byteReader)
		{
			var length = byteReader.ReadInt();
			var isArray = collectionType.IsArray;

			if (isArray)
			{
				// Create array instance.
				var arrayElementType = collectionType.GetElementType();
				if (arrayElementType == null) throw new Exception("Could not find correct array element type. Have assemblies changed?");

				var arrayInstance = Array.CreateInstance(arrayElementType, length);

				// Populate array.
				for (var i = 0; i < length; i++)
				{
					arrayInstance.SetValue(await DeserializeObjectAsync(arrayElementType, byteReader), i);
				}

				// Return array instance.
				return arrayInstance;
			}

			var isSerializableCollection = collectionType.IsSerializableCollection();
			if (isSerializableCollection)
			{
				var isList = collectionType.IsList();
				var isStack = collectionType.IsStack();
				var isQueue = collectionType.IsQueue();

				if (isList)
				{
					// Create list instance.
					var listElementType = collectionType.GetGenericArguments()[0];
					var listInstance = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(listElementType));

					// Populate list.
					for (var i = 0; i < length; i++)
					{
						listInstance.Add(await DeserializeObjectAsync(listElementType, byteReader));
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

					// Populate list.
					for (var i = 0; i < length; i++)
					{
						pushMethod?.Invoke(stackInstance, new object[] { await DeserializeObjectAsync(stackElementType, byteReader) });
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

					// Populate list.
					for (var i = 0; i < length; i++)
					{
						enqueueMethod?.Invoke(queueInstance, new object[] { await DeserializeObjectAsync(queueElementType, byteReader) });
					}

					// Return list instance.
					return queueInstance;
				}
			}

			throw new Exception("Tried to deserialize a collection that wasn't an array or a single argument list.");
		}

		private object DeserializeObjectUsingReflection(Type targetType, StbByteReader byteReader)
		{
			try
			{
				if (targetType == typeof(object))
				{
					throw new Exception("Trying to load object that was serialized as type object -- something has gone wrong.");
				}

				var membersInfo = GetFieldsAndProperties(targetType);
				var newObjectInstance = Activator.CreateInstance(targetType);

				// Now create instances of all the fields etc.
				foreach (var memberInfo in membersInfo)
				{
					switch (memberInfo.MemberType)
					{
						case MemberTypes.Field:
							var fieldInfo = (FieldInfo)memberInfo;
							var fieldType = fieldInfo.FieldType;
							fieldInfo.SetValue(newObjectInstance, DeserializeObject(fieldType, byteReader));
							break;
						case MemberTypes.Property:
							var propertyInfo = (PropertyInfo)memberInfo;
							var propertyType = propertyInfo.PropertyType;
							propertyInfo.SetValue(newObjectInstance, DeserializeObject(propertyType, byteReader));
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

		private object DeserializeCollection(Type collectionType, StbByteReader byteReader)
		{
			var length = byteReader.ReadInt();
			var isArray = collectionType.IsArray;

			if (isArray)
			{
				// Create array instance.
				var arrayElementType = collectionType.GetElementType();
				if (arrayElementType == null) throw new Exception("Could not find correct array element type. Have assemblies changed?");

				var arrayInstance = Array.CreateInstance(arrayElementType, length);

				// Populate array.
				for (var i = 0; i < length; i++)
				{
					arrayInstance.SetValue(DeserializeObject(arrayElementType, byteReader), i);
				}

				// Return array instance.
				return arrayInstance;
			}

			var isSerializableCollection = collectionType.IsSerializableCollection();
			if (isSerializableCollection)
			{
				var isList = collectionType.IsList();
				var isStack = collectionType.IsStack();
				var isQueue = collectionType.IsQueue();

				if (isList)
				{
					// Create list instance.
					var listElementType = collectionType.GetGenericArguments()[0];
					var listInstance = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(listElementType));

					// Populate list.
					for (var i = 0; i < length; i++)
					{
						listInstance.Add(DeserializeObject(listElementType, byteReader));
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

					// Populate list.
					for (var i = 0; i < length; i++)
					{
						pushMethod?.Invoke(stackInstance, new object[] { DeserializeObject(stackElementType, byteReader) });
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

					// Populate list.
					for (var i = 0; i < length; i++)
					{
						enqueueMethod?.Invoke(queueInstance, new object[] { DeserializeObject(queueElementType, byteReader) });
					}

					// Return list instance.
					return queueInstance;
				}
			}

			throw new Exception("Tried to deserialize a collection that wasn't an array or a single argument list.");
		}

		private void SerializePrimitive(Type type, object objectValue, StbByteWriter byteWriter)
		{
			if (type == typeof(bool))
			{
				byteWriter.Write((bool)objectValue);
			}
			else if (type == typeof(string))
			{
				byteWriter.Write((string)objectValue);
			}
			else if (type == typeof(char))
			{
				byteWriter.Write((char)objectValue);
			}
			else if (type == typeof(float))
			{
				byteWriter.Write((float)objectValue);
			}
			else if (type == typeof(double))
			{
				byteWriter.Write((double)objectValue);
			}
			else if (type == typeof(decimal))
			{
				byteWriter.Write((decimal)objectValue);
			}
			else if (type == typeof(int))
			{
				byteWriter.Write((int)objectValue);
			}
			else if (type == typeof(uint))
			{
				byteWriter.Write((uint)objectValue);
			}
			else if (type == typeof(long))
			{
				byteWriter.Write((long)objectValue);
			}
			else if (type == typeof(ulong))
			{
				byteWriter.Write((ulong)objectValue);
			}
			else if (type == typeof(short))
			{
				byteWriter.Write((short)objectValue);
			}
			else if (type == typeof(ushort))
			{
				byteWriter.Write((ushort)objectValue);
			}
			else if (type == typeof(byte))
			{
				byteWriter.Write((byte)objectValue);
			}
			else if (type == typeof(sbyte))
			{
				byteWriter.Write((sbyte)objectValue);
			}
			else if (type == typeof(bool[]))
			{
				byteWriter.Write((bool[])objectValue);
			}
			else if (type == typeof(string[]))
			{
				byteWriter.Write((string[])objectValue);
			}
			else if (type == typeof(char[]))
			{
				byteWriter.Write((char[])objectValue);
			}
			else if (type == typeof(float[]))
			{
				byteWriter.Write((float[])objectValue);
			}
			else if (type == typeof(double[]))
			{
				byteWriter.Write((double[])objectValue);
			}
			else if (type == typeof(decimal[]))
			{
				byteWriter.Write((decimal[])objectValue);
			}
			else if (type == typeof(int[]))
			{
				byteWriter.Write((int[])objectValue);
			}
			else if (type == typeof(uint[]))
			{
				byteWriter.Write((uint[])objectValue);
			}
			else if (type == typeof(long[]))
			{
				byteWriter.Write((long[])objectValue);
			}
			else if (type == typeof(ulong[]))
			{
				byteWriter.Write((ulong[])objectValue);
			}
			else if (type == typeof(short[]))
			{
				byteWriter.Write((short[])objectValue);
			}
			else if (type == typeof(ushort[]))
			{
				byteWriter.Write((ushort[])objectValue);
			}
			else if (type == typeof(byte[]))
			{
				byteWriter.Write((byte[])objectValue);
			}
			else if (type == typeof(sbyte[]))
			{
				byteWriter.Write((sbyte[])objectValue);
			}
		}

		private object DeserializePrimitive(Type type, StbByteReader byteReader)
		{
			if (type == typeof(bool)) return byteReader.ReadBool();
			if (type == typeof(string)) return byteReader.ReadString();
			if (type == typeof(char)) return byteReader.ReadChar();
			if (type == typeof(float)) return byteReader.ReadFloat();
			if (type == typeof(double)) return byteReader.ReadDouble();
			if (type == typeof(decimal)) return byteReader.ReadDecimal();
			if (type == typeof(int)) return byteReader.ReadInt();
			if (type == typeof(uint)) return byteReader.ReadUint();
			if (type == typeof(long)) return byteReader.ReadLong();
			if (type == typeof(ulong)) return byteReader.ReadUlong();
			if (type == typeof(short)) return byteReader.ReadShort();
			if (type == typeof(ushort)) return byteReader.ReadUshort();
			if (type == typeof(byte)) return byteReader.ReadByte();
			if (type == typeof(sbyte)) return byteReader.ReadSbyte();

			// Deserialize arrays.
			if (type == typeof(bool[])) return byteReader.ReadBoolArray();
			if (type == typeof(string[])) return byteReader.ReadStringArray();
			if (type == typeof(char[])) return byteReader.ReadCharArray();
			if (type == typeof(float[])) return byteReader.ReadFloatArray();
			if (type == typeof(double[])) return byteReader.ReadDoubleArray();
			if (type == typeof(decimal[])) return byteReader.ReadDecimalArray();
			if (type == typeof(int[])) return byteReader.ReadIntArray();
			if (type == typeof(uint[])) return byteReader.ReadUintArray();
			if (type == typeof(long[])) return byteReader.ReadLongArray();
			if (type == typeof(ulong[])) return byteReader.ReadUlongArray();
			if (type == typeof(short[])) return byteReader.ReadShortArray();
			if (type == typeof(ushort[])) return byteReader.ReadUshortArray();
			if (type == typeof(byte[])) return byteReader.ReadByteArray();
			if (type == typeof(sbyte[])) return byteReader.ReadSbyteArray();

			throw new Exception("Passed in type was not a primitive.");
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
		/// Override this to add extra functionality on what can or cannot be serialized with custom functionality.
		/// </summary>
		/// <param name="type">The type to be checked if it can be serialized.</param>
		/// <returns>Whether or not this type can be serialized.</returns>
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
		/// Override this to add extra functionality on how the object of a specific type can be serialized.
		/// </summary>
		/// <param name="objectValue">The current object value.</param>
		/// <param name="byteWriter">The byte writer to allow for binary serialization.</param>
		/// <exception cref="Exception">Failed to serialize exception.</exception>
		protected virtual void Serialize(object objectValue, StbByteWriter byteWriter)
		{
			if (objectValue is Vector2 vector2)
			{
				byteWriter.Write(vector2);
				return;
			}

			if (objectValue is Vector3 vector3)
			{
				byteWriter.Write(vector3);
				return;
			}

			if (objectValue is Vector4 vector4)
			{
				byteWriter.Write(vector4);
				return;
			}

			if (objectValue is Color color)
			{
				byteWriter.Write(color);
				return;
			}

			if (objectValue is Quaternion quaternion)
			{
				byteWriter.Write(quaternion);
				return;
			}

			if (objectValue is Vector2[] vector2Array)
			{
				byteWriter.Write(vector2Array);
				return;
			}

			if (objectValue is Vector3[] vector3Array)
			{
				byteWriter.Write(vector3Array);
				return;
			}

			if (objectValue is Vector4[] vector4Array)
			{
				byteWriter.Write(vector4Array);
				return;
			}

			if (objectValue is Color[] colorArray)
			{
				byteWriter.Write(colorArray);
				return;
			}

			if (objectValue is Quaternion[] quaternionArray)
			{
				byteWriter.Write(quaternionArray);
				return;
			}

			if (objectValue is Texture2D texture2D)
			{
				var byteArray = texture2D.EncodeToPNG();
				byteWriter.Write(byteArray);
				return;
			}

			if (objectValue is Enum targetEnum)
			{
				byteWriter.Write(targetEnum.GetType().AssemblyQualifiedName);
				byteWriter.Write(Convert.ToInt32(targetEnum));
				return;
			}

			if (objectValue is StbByteWriter stbByteWriter)
			{
				byteWriter.Write(stbByteWriter.GetByteArray());
				return;
			}

			if (typeof(ScriptableObject).IsAssignableFrom(objectValue.GetType()))
			{
				byteWriter.Write((ScriptableObject)objectValue);
				return;
			}

			if (objectValue.GetType().IsGenericType && objectValue.GetType().GetGenericTypeDefinition() == typeof(SaveEntityReference<>))
			{
				var objectType = objectValue.GetType();

				var referenceType = objectType.GetGenericArguments()[0];
				byteWriter.Write(referenceType.AssemblyQualifiedName);

				var identifierProperty = objectType.GetProperty("Identifier");
				var identifierString = (string)identifierProperty?.GetValue(objectValue);
				byteWriter.Write(identifierString);
				return;
			}

			throw new Exception($"Could not serialize object of type: {objectValue.GetType()}");
		}

		/// <summary>
		/// Override this to add extra functionality on how the object of a specific type can be deserialized.
		/// </summary>
		/// <param name="type">The type of object that is expected to be deserialized.</param>
		/// <param name="byteReader">The byte reader to assist in deserializing the object.
		/// Should contain the byte array that relates to the data that is needed for deserialization of the object.</param>
		/// <returns>An object instance of the type that was defined in the parameters.</returns>
		/// <exception cref="Exception">Failed to deserialize exception.</exception>
		protected virtual object Deserialize(Type type, StbByteReader byteReader)
		{
			if (type == typeof(Vector2))
			{
				return byteReader.ReadVector2();
			}

			if (type == typeof(Vector3))
			{
				return byteReader.ReadVector3();
			}

			if (type == typeof(Vector4))
			{
				return byteReader.ReadVector4();
			}

			if (type == typeof(Color))
			{
				return byteReader.ReadColor();
			}

			if (type == typeof(Quaternion))
			{
				return byteReader.ReadQuaternion();
			}

			if (type == typeof(Vector2[]))
			{
				return byteReader.ReadVector2Array();
			}

			if (type == typeof(Vector3[]))
			{
				return byteReader.ReadVector3Array();
			}

			if (type == typeof(Vector4[]))
			{
				return byteReader.ReadVector4Array();
			}

			if (type == typeof(Color[]))
			{
				return byteReader.ReadColorArray();
			}

			if (type == typeof(Quaternion[]))
			{
				return byteReader.ReadQuaternionArray();
			}

			if (type == typeof(Texture2D))
			{
				var newTexture = new Texture2D(1, 1);
				var byteArray = byteReader.ReadByteArray();
				newTexture.LoadImage(byteArray);
				return newTexture;
			}

			if (type.IsEnum)
			{
				var enumType = StbSerializationUtilities.GetType(byteReader.ReadString());
				if (enumType == null) throw new Exception("Could not deserialize enum. Have assemblies changed?");

				return Enum.ToObject(enumType, byteReader.ReadInt());
			}

			if (type == typeof(StbByteWriter))
			{
				var byteArray = byteReader.ReadByteArray();
				return new StbByteReader(byteArray);
			}

			if (typeof(ScriptableObject).IsAssignableFrom(type))
			{
				return byteReader.ReadScriptableObject();
			}

			if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(SaveEntityReference<>))
			{
				var referenceTypeString = byteReader.ReadString();
				var identifierString = byteReader.ReadString();
				if (StbUtilities.TryGetISaveDataEntity(identifierString, out var iSaveDataEntity))
				{
					var referenceType = StbSerializationUtilities.GetType(referenceTypeString);
					var instanceType = typeof(SaveEntityReference<>).MakeGenericType(referenceType);
					var entityReference = Activator.CreateInstance(instanceType);

					var entityReferenceProperty = instanceType.GetProperty("EntityReference");
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

		/// <summary>
		/// A function that can be overridden that defines which memberinfos will be selected when serializing an
		/// object through reflection.
		/// </summary>
		/// <param name="objectType">The objects type.</param>
		/// <returns>A list of MemberInfos that should be serialized for an object of this type.</returns>
		protected virtual List<MemberInfo> GetFieldsAndProperties(Type objectType)
		{
			return StbUtilities.GetAllSerializableFieldsAndProperties(objectType, serializationSettings);
		}
	}
}