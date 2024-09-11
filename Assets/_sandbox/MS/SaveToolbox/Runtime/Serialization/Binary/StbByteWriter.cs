using System;
using SaveToolbox.Runtime.Core.ScriptableObjects;
using SaveToolbox.Runtime.Interfaces;
using SaveToolbox.Runtime.Utils;
using UnityEngine;

namespace SaveToolbox.Runtime.Serialization.Binary
{
	public class StbByteWriter
	{
		private const int DEFAULT_SIZE = 32;

		private byte[] byteData;
		private int currentBytePosition;

		private readonly bool shouldResize;
		private int currentSize;

		public StbByteWriter(bool shouldResize = true, int initialSize = DEFAULT_SIZE)
		{
			this.shouldResize = shouldResize;
			byteData = new byte[currentSize = initialSize];
		}

		public void Write(byte value)
		{
			if (shouldResize) CheckResize(1);

			byteData[currentBytePosition++] = value;
		}

		public void Write(sbyte value)
		{
			if (shouldResize) CheckResize(1);

			WriteValueToBytes(BitConverter.GetBytes(value), 1);
		}

		public void Write(bool value)
		{
			if (shouldResize) CheckResize(1);

			WriteValueToBytes(BitConverter.GetBytes(value), 1);
		}

		public void Write(string value)
		{
			Write(value.ToCharArray(0, value.Length));
		}

		public void Write(char value)
		{
			if (shouldResize) CheckResize(2);

			WriteValueToBytes(BitConverter.GetBytes(value), 2);
		}

		public void Write(float value)
		{
			if (shouldResize) CheckResize(4);

			WriteValueToBytes(BitConverter.GetBytes(value), 4);
		}

		public void Write(double value)
		{
			if (shouldResize) CheckResize(8);

			WriteValueToBytes(BitConverter.GetBytes(value), 8);
		}

		public void Write(decimal value)
		{
			// Get bits returns a decimal as an int array of size of 4.
			Write(decimal.GetBits(value));
		}

		public void Write(int value)
		{
			if (shouldResize) CheckResize(4);

			WriteValueToBytes(BitConverter.GetBytes(value), 4);
		}

		public void Write(uint value)
		{
			if (shouldResize) CheckResize(4);

			WriteValueToBytes(BitConverter.GetBytes(value), 4);
		}

		public void Write(long value)
		{
			if (shouldResize) CheckResize(8);

			WriteValueToBytes(BitConverter.GetBytes(value), 8);
		}

		public void Write(ulong value)
		{
			if (shouldResize) CheckResize(8);

			WriteValueToBytes(BitConverter.GetBytes(value), 8);
		}

		public void Write(short value)
		{
			if (shouldResize) CheckResize(2);

			WriteValueToBytes(BitConverter.GetBytes(value), 2);
		}

		public void Write(ushort value)
		{
			if (shouldResize) CheckResize(2);

			WriteValueToBytes(BitConverter.GetBytes(value), 2);
		}

		public void Write(Enum value)
		{
			Write(Convert.ToInt32(value));
		}

		public void Write(Vector2 value)
		{
			var vectorFloatArray = new float[2];
			vectorFloatArray[0] = value.x;
			vectorFloatArray[1] = value.y;

			Write(vectorFloatArray);
		}

		public void Write(Vector3 value)
		{
			var vectorFloatArray = new float[3];
			vectorFloatArray[0] = value.x;
			vectorFloatArray[1] = value.y;
			vectorFloatArray[2] = value.z;

			Write(vectorFloatArray);
		}

		public void Write(Vector4 value)
		{
			var vectorFloatArray = new float[4];
			vectorFloatArray[0] = value.x;
			vectorFloatArray[1] = value.y;
			vectorFloatArray[2] = value.z;
			vectorFloatArray[3] = value.w;

			Write(vectorFloatArray);
		}

		public void Write(Quaternion value)
		{
			var quaternionFloatArray = new float[4];
			quaternionFloatArray[0] = value.x;
			quaternionFloatArray[1] = value.y;
			quaternionFloatArray[2] = value.z;
			quaternionFloatArray[3] = value.w;

			Write(quaternionFloatArray);
		}

		public void Write(Color value)
		{
			var colorFloatArray = new float[4];
			colorFloatArray[0] = value.r;
			colorFloatArray[1] = value.g;
			colorFloatArray[2] = value.b;
			colorFloatArray[3] = value.a;

			Write(colorFloatArray);
		}

		public void Write(ScriptableObject value)
		{
			var isSaveable = ScriptableObjectDatabase.Instance.TryGetScriptableObjectGuid(value, out var scriptableObjectAssetGuid);
			Write(isSaveable);
			if (isSaveable)
			{
				Write(scriptableObjectAssetGuid);
			}
		}

		public void Write<T>(SaveEntityReference<T> saveEntityReference) where T : ISaveDataEntity
		{
			Write(saveEntityReference.Identifier);
		}

		public void Write(byte[] value)
		{
			WriteArray(value, 1 * value.Length);
		}

		public void Write(sbyte[] value)
		{
			WriteArray(value, 1 * value.Length);
		}

		public void Write(bool[] value)
		{
			WriteArray(value, 1 * value.Length);
		}

		public void Write(string[] value)
		{
			// Cannot use the traditional block copy like other primitives, so have to do this instead.
			var arrayLength = value.Length;
			Write(arrayLength);

			for (var i = 0; i < value.Length; ++i)
			{
				Write(value[i]);
			}
		}

		public void Write(char[] value)
		{
			WriteArray(value, 2 * value.Length);
		}

		public void Write(float[] value)
		{
			WriteArray(value, 4 * value.Length);
		}

		public void Write(double[] value)
		{
			WriteArray(value, 8 * value.Length);
		}

		public void Write(decimal[] value)
		{
			// Cannot use the traditional block copy like other primitives, so have to do this instead.
			var arrayLength = value.Length;
			Write(arrayLength);

			for (var i = 0; i < value.Length; ++i)
			{
				Write(value[i]);
			}
		}

		public void Write(int[] value)
		{
			WriteArray(value, 4 * value.Length);
		}

		public void Write(uint[] value)
		{
			WriteArray(value, 4 * value.Length);
		}

		public void Write(long[] value)
		{
			WriteArray(value, 8 * value.Length);
		}

		public void Write(ulong[] value)
		{
			WriteArray(value, 8 * value.Length);
		}

		public void Write(short[] value)
		{
			WriteArray(value, 2 * value.Length);
		}

		public void Write(ushort[] value)
		{
			WriteArray(value, 2 * value.Length);
		}

		public void Write(Enum[] value)
		{
			var intArray = new int[value.Length];
			for (var i = 0; i < intArray.Length; i++)
			{
				intArray[i] = Convert.ToInt32(value[i]);
			}
			Write(intArray);
		}

		public void Write(Vector2[] value)
		{
			var newFloatArray = new float[value.Length * 2];
			for (var i = 0; i < value.Length; ++i)
			{
				newFloatArray[i * 2] = value[i].x;
				newFloatArray[i * 2 + 1] = value[i].y;
			}

			Write(newFloatArray);
		}

		public void Write(Vector3[] value)
		{
			var newFloatArray = new float[value.Length * 3];
			for (var i = 0; i < value.Length; ++i)
			{
				newFloatArray[i * 3] = value[i].x;
				newFloatArray[i * 3 + 1] = value[i].y;
				newFloatArray[i * 3 + 2] = value[i].z;
			}

			Write(newFloatArray);
		}

		public void Write(Vector4[] value)
		{
			var newFloatArray = new float[value.Length * 4];
			for (var i = 0; i < value.Length; ++i)
			{
				newFloatArray[i * 4] = value[i].x;
				newFloatArray[i * 4 + 1] = value[i].y;
				newFloatArray[i * 4 + 2] = value[i].z;
				newFloatArray[i * 4 + 3] = value[i].w;
			}

			Write(newFloatArray);
		}

		public void Write(Quaternion[] value)
		{
			var newFloatArray = new float[value.Length * 4];
			for (var i = 0; i < value.Length; ++i)
			{
				newFloatArray[i * 4] = value[i].x;
				newFloatArray[i * 4 + 1] = value[i].y;
				newFloatArray[i * 4 + 2] = value[i].z;
				newFloatArray[i * 4 + 3] = value[i].w;
			}

			Write(newFloatArray);
		}

		public void Write(Color[] value)
		{
			var newFloatArray = new float[value.Length * 4];
			for (var i = 0; i < value.Length; ++i)
			{
				newFloatArray[i * 4] = value[i].r;
				newFloatArray[i * 4 + 1] = value[i].g;
				newFloatArray[i * 4 + 2] = value[i].b;
				newFloatArray[i * 4 + 3] = value[i].a;
			}

			Write(newFloatArray);
		}

		public void Write<T>(SaveEntityReference<T>[] value) where T : ISaveDataEntity
		{
			var newStringArray = new string[value.Length];
			for (var i = 0; i < value.Length; ++i)
			{
				newStringArray[i] = value[i].Identifier;
			}

			Write(newStringArray);
		}

		public void Write(ScriptableObject[] value)
		{
			var newStringArray = new string[value.Length];
			for (var i = 0; i < value.Length; ++i)
			{
				newStringArray[i] = ScriptableObjectDatabase.Instance.GetScriptableObjectGuid(value[i]);
			}

			Write(newStringArray);
		}

		private void WriteArray(Array targetArray, int arrayMemorySize)
		{
			// If it's null we still want to write a length of 0 in so that it doesn't break it when trying to read.
			var isNull = targetArray == null;
			var arrayLength = isNull ? 0 : targetArray.Length;
			Write(arrayLength);
			if (isNull) return;

			if (shouldResize) CheckResize(arrayMemorySize);

			Buffer.BlockCopy(targetArray, 0, byteData, currentBytePosition, arrayMemorySize);
			currentBytePosition += arrayMemorySize;
		}

		private void WriteValueToBytes(byte[] newData, int size)
		{
			for (var i = 0; i < size; i++)
			{
				byteData[currentBytePosition++] = newData[i];
			}
		}

		private void CheckResize(int byteSize)
		{
			var newSize = currentBytePosition + byteSize;
			if (newSize > currentSize)
			{
				// Double it to resize it.
				currentSize *= 2;
				Array.Resize(ref byteData, Math.Max(newSize, currentSize));
			}
		}

		public byte[] GetByteArray()
		{
			return byteData;
		}
	}
}