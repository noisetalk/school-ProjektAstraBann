using System;
using System.Collections.Generic;
using SaveToolbox.Runtime.Core.MonoBehaviours;
using SaveToolbox.Runtime.Serialization;
using UnityEngine;

namespace SaveToolbox.Example.Scripts
{
	public class ExampleContainerSaving : SaveableMonoBehaviour
	{
		[SerializeField]
		private bool exampleBool;

		[SerializeField]
		private string exampleString;

		[SerializeField]
		private char exampleChar;

		[SerializeField]
		private float exampleFloat;

		[SerializeField]
		private double exampleDouble;

		[SerializeField]
		private int exampleInt;

		[SerializeField]
		private uint exampleUint;

		[SerializeField]
		private long exampleLong;

		[SerializeField]
		private ulong exampleUlong;

		[SerializeField]
		private short exampleShort;

		[SerializeField]
		private ushort exampleUshort;

		[SerializeField]
		private byte exampleByte;

		[SerializeField]
		private sbyte exampleSbyte;

		[SerializeField]
		private bool[] exampleBoolArray;

		[SerializeField]
		private string[] exampleStringArray;

		[SerializeField]
		private char[] exampleCharArray;

		[SerializeField]
		private float[] exampleFloatArray;

		[SerializeField]
		private double[] exampleDoubleArray;

		[SerializeField]
		private int[] exampleIntArray;

		[SerializeField]
		private uint[] exampleUintArray;

		[SerializeField]
		private long[] exampleLongArray;

		[SerializeField]
		private ulong[] exampleUlongArray;

		[SerializeField]
		private short[] exampleShortArray;

		[SerializeField]
		private ushort[] exampleUshortArray;

		[SerializeField]
		private byte[] exampleByteArray;

		[SerializeField]
		private sbyte[] exampleSbyteArray;

		[SerializeField]
		private CustomData customData;

		[SerializeField]
		private Stack<int> intStack = new Stack<int>();

		[SerializeField]
		private Stack<string> stringStack = new Stack<string>();

		[SerializeField]
		private Queue<int> intQueue = new Queue<int>();

		[SerializeField]
		private Queue<string> stringQueue = new Queue<string>();

		private decimal exampleDecimal = 1230998847384;
		private decimal[] exampleDecimalArray = {9876238, 03012312932132809, 097823812738};

		protected override void Awake()
		{
			intStack.Push(1);
			intStack.Push(2);
			intStack.Push(3);

			stringStack.Push("One Stack");
			stringStack.Push("Two Stack");
			stringStack.Push("Three Stack");

			intQueue.Enqueue(1);
			intQueue.Enqueue(2);
			intQueue.Enqueue(3);

			stringQueue.Enqueue("One Queue");
			stringQueue.Enqueue("Two Queue");
			stringQueue.Enqueue("Three Queue");
		}

		public override object Serialize()
		{
			var dataContainer = new StbDataContainer();
			dataContainer.AddKeyValueEntry("exampleBool", exampleBool);
			dataContainer.AddKeyValueEntry("exampleString", exampleString);
			dataContainer.AddKeyValueEntry("exampleChar", exampleChar);
			dataContainer.AddKeyValueEntry("exampleFloat", exampleFloat);
			dataContainer.AddKeyValueEntry("exampleDouble", exampleDouble);
			dataContainer.AddKeyValueEntry("exampleInt", exampleInt);
			dataContainer.AddKeyValueEntry("exampleUint", exampleUint);
			dataContainer.AddKeyValueEntry("exampleLong", exampleLong);
			dataContainer.AddKeyValueEntry("exampleUlong", exampleUlong);
			dataContainer.AddKeyValueEntry("exampleShort", exampleShort);
			dataContainer.AddKeyValueEntry("exampleUshort", exampleUshort);
			dataContainer.AddKeyValueEntry("exampleByte", exampleByte);
			dataContainer.AddKeyValueEntry("exampleSbyte", exampleSbyte);
			dataContainer.AddKeyValueEntry("exampleDecimal", exampleDecimal);

			dataContainer.AddKeyValueEntry("exampleBoolArray", exampleBoolArray);
			dataContainer.AddKeyValueEntry("exampleStringArray", exampleStringArray);
			dataContainer.AddKeyValueEntry("exampleCharArray", exampleCharArray);
			dataContainer.AddKeyValueEntry("exampleFloatArray", exampleFloatArray);
			dataContainer.AddKeyValueEntry("exampleDoubleArray", exampleDoubleArray);
			dataContainer.AddKeyValueEntry("exampleIntArray", exampleIntArray);
			dataContainer.AddKeyValueEntry("exampleUintArray", exampleUintArray);
			dataContainer.AddKeyValueEntry("exampleLongArray", exampleLongArray);
			dataContainer.AddKeyValueEntry("exampleUlongArray", exampleUlongArray);
			dataContainer.AddKeyValueEntry("exampleShortArray", exampleShortArray);
			dataContainer.AddKeyValueEntry("exampleUshortArray", exampleUshortArray);
			dataContainer.AddKeyValueEntry("exampleByteArray", exampleByteArray);
			dataContainer.AddKeyValueEntry("exampleSbyteArray", exampleSbyteArray);
			dataContainer.AddKeyValueEntry("exampleDecimalArray", exampleDecimalArray);

			dataContainer.AddKeyValueEntry("exampleIntStack", intStack);
			dataContainer.AddKeyValueEntry("exampleStringStack", stringStack);
			dataContainer.AddKeyValueEntry("exampleIntQueue", intQueue);
			dataContainer.AddKeyValueEntry("exampleStringQueue", stringQueue);

			dataContainer.AddKeyValueEntry("customData", customData);

			return dataContainer;
		}

		public override void Deserialize(object data)
		{
			var dataContainer = (StbDataContainer)data;
			exampleBool = dataContainer.GetValue<bool>("exampleBool");
			exampleString = dataContainer.GetValue<string>("exampleString");
			exampleChar = dataContainer.GetValue<char>("exampleChar");
			exampleFloat = dataContainer.GetValue<float>("exampleFloat");
			exampleDouble = dataContainer.GetValue<double>("exampleDouble");
			exampleInt = dataContainer.GetValue<int>("exampleInt");
			exampleUint = dataContainer.GetValue<uint>("exampleUint");
			exampleLong = dataContainer.GetValue<long>("exampleLong");
			exampleUlong = dataContainer.GetValue<ulong>("exampleUlong");
			exampleShort = dataContainer.GetValue<short>("exampleShort");
			exampleUshort = dataContainer.GetValue<ushort>("exampleUshort");
			exampleByte = dataContainer.GetValue<byte>("exampleByte");
			exampleSbyte = dataContainer.GetValue<sbyte>("exampleSbyte");
			exampleDecimal = dataContainer.GetValue<decimal>("exampleDecimal");
			Debug.Log("Data Container, Loaded Decimal:" + exampleDecimal);

			exampleBoolArray = dataContainer.GetValue<bool[]>("exampleBoolArray");
			exampleStringArray = dataContainer.GetValue<string[]>("exampleStringArray");
			exampleCharArray = dataContainer.GetValue<char[]>("exampleCharArray");
			exampleFloatArray = dataContainer.GetValue<float[]>("exampleFloatArray");
			exampleDoubleArray = dataContainer.GetValue<double[]>("exampleDoubleArray");
			exampleIntArray = dataContainer.GetValue<int[]>("exampleIntArray");
			exampleUintArray = dataContainer.GetValue<uint[]>("exampleUintArray");
			exampleLongArray = dataContainer.GetValue<long[]>("exampleLongArray");
			exampleUlongArray = dataContainer.GetValue<ulong[]>("exampleUlongArray");
			exampleShortArray = dataContainer.GetValue<short[]>("exampleShortArray");
			exampleUshortArray = dataContainer.GetValue<ushort[]>("exampleUshortArray");
			exampleByteArray = dataContainer.GetValue<byte[]>("exampleByteArray");
			exampleSbyteArray = dataContainer.GetValue<sbyte[]>("exampleSbyteArray");
			exampleDecimalArray = dataContainer.GetValue<decimal[]>("exampleDecimalArray");
			foreach (var targetDecimal in exampleDecimalArray)
			{
				Debug.Log("Data Container, Loaded Decimal Array Entry:" + targetDecimal);
			}

			customData = dataContainer.GetValue<CustomData>("customData");

			intStack = dataContainer.GetValue<Stack<int>>("exampleIntStack");
			stringStack = dataContainer.GetValue<Stack<string>>("exampleStringStack");
			intQueue = dataContainer.GetValue<Queue<int>>("exampleIntQueue");
			stringQueue = dataContainer.GetValue<Queue<string>>("exampleStringQueue");
		}

		[Serializable]
		public class CustomData
		{
			[SerializeField]
			private string customDataString;

			[SerializeField]
			private Vector3 customVector3;

			[SerializeField]
			private Quaternion customDataQuaternion;
		}
	}
}
