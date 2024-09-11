using System;
using SaveToolbox.Runtime.Core.ScriptableObjects;
using SaveToolbox.Runtime.Interfaces;
using SaveToolbox.Runtime.Utils;
using UnityEngine;

namespace SaveToolbox.Runtime.Serialization.Binary
{
	public class StbByteReader
	{
		private byte[] byteData = Array.Empty<byte>();
		private int currentBytePosition;

		private int currentSize;

		public StbByteReader() {}

		public StbByteReader(byte[] bytes)
		{
			AssignByteData(bytes);
		}

		public void AssignByteData(byte[] bytes)
		{
			byteData = bytes;
		}

		public byte ReadByte()
		{
			var result = byteData[currentBytePosition];
			currentBytePosition++;
			return result;
		}

		public sbyte ReadSbyte()
		{
			return (sbyte)ReadByte();
		}

		public bool ReadBool()
		{
			var result = BitConverter.ToBoolean(byteData, currentBytePosition);
			currentBytePosition += 1;
			return result;
		}

		public string ReadString()
		{
			return new string(ReadCharArray());
		}

		public char ReadChar()
		{
			var result = BitConverter.ToChar(byteData, currentBytePosition);
			currentBytePosition += 2;
			return result;
		}

		public float ReadFloat()
		{
			var result = BitConverter.ToSingle(byteData, currentBytePosition);
			currentBytePosition += 4;
			return result;
		}

		public double ReadDouble()
		{
			var result = BitConverter.ToDouble(byteData, currentBytePosition);
			currentBytePosition += 8;
			return result;
		}

		public decimal ReadDecimal()
		{
			var intArray = ReadIntArray();
			return new decimal(intArray);
		}

		public int ReadInt()
		{
			try
			{
				var result = BitConverter.ToInt32(byteData, currentBytePosition);
				currentBytePosition += 4;
				return result;
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				throw;
			}

		}

		public uint ReadUint()
		{
			var result = BitConverter.ToUInt32(byteData, currentBytePosition);
			currentBytePosition += 4;
			return result;
		}

		public long ReadLong()
		{
			var result = BitConverter.ToInt64(byteData, currentBytePosition);
			currentBytePosition += 8;
			return result;
		}

		public ulong ReadUlong()
		{
			var result = BitConverter.ToUInt64(byteData, currentBytePosition);
			currentBytePosition += 8;
			return result;
		}

		public short ReadShort()
		{
			var result = BitConverter.ToInt16(byteData, currentBytePosition);
			currentBytePosition += 2;
			return result;
		}

		public ushort ReadUshort()
		{
			var result = BitConverter.ToUInt16(byteData, currentBytePosition);
			currentBytePosition += 2;
			return result;
		}

		public T ReadEnum<T>() where T : Enum
		{
			return (T)Enum.ToObject(typeof(T), ReadInt());
		}

		public Vector2 ReadVector2()
		{
			var vectorFloatArray = ReadFloatArray();
			return new Vector2(vectorFloatArray[0], vectorFloatArray[1]);
		}

		public Vector3 ReadVector3()
		{
			var vectorFloatArray = ReadFloatArray();
			return new Vector3(vectorFloatArray[0], vectorFloatArray[1], vectorFloatArray[2]);
		}

		public Vector4 ReadVector4()
		{
			var vectorFloatArray = ReadFloatArray();
			return new Vector4(vectorFloatArray[0], vectorFloatArray[1], vectorFloatArray[2], vectorFloatArray[3]);
		}

		public Quaternion ReadQuaternion()
		{
			var vectorFloatArray = ReadFloatArray();
			return new Quaternion(vectorFloatArray[0], vectorFloatArray[1], vectorFloatArray[2], vectorFloatArray[3]);
		}

		public Color ReadColor()
		{
			var vectorFloatArray = ReadFloatArray();
			return new Color(vectorFloatArray[0], vectorFloatArray[1], vectorFloatArray[2], vectorFloatArray[3]);
		}

		public ScriptableObject ReadScriptableObject()
		{
			var savedAssetGuid = ReadBool();
			if (savedAssetGuid)
			{
				var saveeableScriptableObjectIdentifier = ReadString();
				if (ScriptableObjectDatabase.Instance.TryGetScriptableObject(saveeableScriptableObjectIdentifier, out var saveableScriptableObject))
				{
					return saveableScriptableObject;
				}
			}

			return null;
		}

		public SaveEntityReference<T> ReadSaveEntityReference<T>() where T : ISaveDataEntity
		{
			var identifierString = ReadString();
			if (StbUtilities.TryGetISaveDataEntity(identifierString, out var iSaveDataEntity))
			{
				var instanceType = typeof(SaveEntityReference<>).MakeGenericType(typeof(T));
				var entityReference = Activator.CreateInstance(instanceType);

				var entityReferenceProperty = instanceType.GetProperty("EntityReference");
				if (iSaveDataEntity is MonoBehaviour saveEntityMonoBehaviour)
				{
					entityReferenceProperty?.SetValue(entityReference, saveEntityMonoBehaviour);
				}

				return (SaveEntityReference<T>)entityReference;
			}

			Debug.LogError($"Could not find a instance of an ISaveDataEntity with the identifier: {identifierString}");
			return null;
		}

		public byte[] ReadByteArray()
		{
			return (byte[])ReadArray(typeof(byte), 1);
		}

		public sbyte[] ReadSbyteArray()
		{
			return (sbyte[])ReadArray(typeof(sbyte), 1);
		}

		public bool[] ReadBoolArray()
		{
			return (bool[])ReadArray(typeof(bool), 1);
		}

		public string[] ReadStringArray()
		{
			var stringArrayLength = ReadInt();
			var newStringArray = new string[stringArrayLength];

			for (var i = 0; i < stringArrayLength; i++)
			{
				newStringArray[i] = ReadString();
			}

			return newStringArray;
		}

		public char[] ReadCharArray()
		{
			return (char[])ReadArray(typeof(char), 2);
		}

		public float[] ReadFloatArray()
		{
			return (float[])ReadArray(typeof(float), 4);
		}

		public double[] ReadDoubleArray()
		{
			return (double[])ReadArray(typeof(double), 8);
		}

		public decimal[] ReadDecimalArray()
		{
			var decimalArrayLength = ReadInt();
			var newStringArray = new decimal[decimalArrayLength];

			for (var i = 0; i < decimalArrayLength; i++)
			{
				newStringArray[i] = ReadDecimal();
			}

			return newStringArray;
		}

		public int[] ReadIntArray()
		{
			return (int[])ReadArray(typeof(int), 4);
		}

		public uint[] ReadUintArray()
		{
			return (uint[])ReadArray(typeof(uint), 4);
		}

		public long[] ReadLongArray()
		{
			return (long[])ReadArray(typeof(long), 8);
		}

		public ulong[] ReadUlongArray()
		{
			return (ulong[])ReadArray(typeof(ulong), 8);
		}

		public short[] ReadShortArray()
		{
			return (short[])ReadArray(typeof(short), 2);
		}

		public ushort[] ReadUshortArray()
		{
			return (ushort[])ReadArray(typeof(ushort), 2);
		}

		public T[] ReadEnumArray<T>() where T : Enum
		{
			var intArray = ReadIntArray();
			var enumArray = new T[intArray.Length];
			for (var i = 0; i < enumArray.Length; i++)
			{
				enumArray[i] = (T)Enum.ToObject(typeof(T), intArray[i]);
			}

			return enumArray;
		}

		public Vector2[] ReadVector2Array()
		{
			var floatArray = ReadFloatArray();
			var returnArray = new Vector2[floatArray.Length / 2];
			for (var i = 0; i < returnArray.Length; i++)
			{
				returnArray[i] = new Vector2(floatArray[i * 2], floatArray[i * 2 + 1]);
			}
			return returnArray;
		}

		public Vector3[] ReadVector3Array()
		{
			var floatArray = ReadFloatArray();
			var returnArray = new Vector3[floatArray.Length / 3];
			for (var i = 0; i < returnArray.Length; i++)
			{
				returnArray[i] = new Vector3(floatArray[i * 3], floatArray[i * 3 + 1], floatArray[i * 3 + 2]);
			}
			return returnArray;
		}

		public Vector4[] ReadVector4Array()
		{
			var floatArray = ReadFloatArray();
			var returnArray = new Vector4[floatArray.Length / 4];
			for (var i = 0; i < returnArray.Length; i++)
			{
				returnArray[i] = new Vector4(floatArray[i * 4], floatArray[i * 4 + 1], floatArray[i * 4 + 2], floatArray[i * 4 + 3]);
			}
			return returnArray;
		}

		public Quaternion[] ReadQuaternionArray()
		{
			var floatArray = ReadFloatArray();
			var returnArray = new Quaternion[floatArray.Length / 4];
			for (var i = 0; i < returnArray.Length; i++)
			{
				returnArray[i] = new Quaternion(floatArray[i * 4], floatArray[i * 4 + 1], floatArray[i * 4 + 2], floatArray[i * 4 + 3]);
			}
			return returnArray;
		}

		public Color[] ReadColorArray()
		{
			var floatArray = ReadFloatArray();
			var returnArray = new Color[floatArray.Length / 4];
			for (var i = 0; i < returnArray.Length; i++)
			{
				returnArray[i] = new Color(floatArray[i * 4], floatArray[i * 4 + 1], floatArray[i * 4 + 2], floatArray[i * 4 + 3]);
			}
			return returnArray;
		}

		public ScriptableObject[] ReadSaveableScriptableObjectArray()
		{
			var saveableScriptableObjectIdentifiers = ReadStringArray();
			var returnArray = new ScriptableObject[saveableScriptableObjectIdentifiers.Length];
			for (var i = 0; i < saveableScriptableObjectIdentifiers.Length; i++)
			{
				var identifier = saveableScriptableObjectIdentifiers[i];
				if (ScriptableObjectDatabase.Instance.TryGetScriptableObject(identifier, out var saveableScriptableObject))
				{
					returnArray[i] = saveableScriptableObject;
				}
			}

			return returnArray;
		}

		public SaveEntityReference<T>[] ReadSaveEntityReferenceArray<T>() where T : ISaveDataEntity
		{
			var identifierArray = ReadStringArray();
			var referenceArray = new SaveEntityReference<T>[identifierArray.Length];
			for (var i = 0; i < identifierArray.Length; i++)
			{
				var identifier = identifierArray[i];
				if (StbUtilities.TryGetISaveDataEntity(identifier, out var iSaveDataEntity))
				{
					var instanceType = typeof(SaveEntityReference<>).MakeGenericType(typeof(T));
					var entityReference = Activator.CreateInstance(instanceType);

					var entityReferenceProperty = instanceType.GetProperty("EntityReference");
					if (iSaveDataEntity is MonoBehaviour saveEntityMonoBehaviour)
					{
						entityReferenceProperty?.SetValue(entityReference, saveEntityMonoBehaviour);
					}

					referenceArray[i] = (SaveEntityReference<T>)entityReference;
				}
			}

			return referenceArray;
		}

		private Array ReadArray(Type arrayElementType, int elementTypeSize)
		{
			var arrayLength = ReadInt();
			var newArrayInstance = Array.CreateInstance(arrayElementType, arrayLength);

			var totalArrayByteSize = elementTypeSize * arrayLength;
			Buffer.BlockCopy(byteData, currentBytePosition, newArrayInstance, 0, totalArrayByteSize);
			currentBytePosition += totalArrayByteSize;

			return newArrayInstance;
		}

		public byte[] GetByteArray()
		{
			return byteData;
		}
	}
}