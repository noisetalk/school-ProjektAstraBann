using System.Collections.Generic;
using SaveToolbox.Runtime.BasicSaveableMonoBehaviours;
using SaveToolbox.Runtime.Utils;
using UnityEngine;

namespace SaveToolbox.Example.Scripts
{
	/// <summary>
	/// A MonoBehaviour class to show how field selection works and that it works with collection likes lists and arrays
	/// for the StbComponentSaver.
	/// </summary>
	public class CustomComponentTestBehaviour : MonoBehaviour
	{
		[SerializeField]
		private byte byteField;

		[SerializeField]
		private sbyte sByteField;

		[SerializeField]
		private bool boolField;

		[SerializeField]
		private string stringField;

		[SerializeField]
		private char charField;

		[SerializeField]
		private float floatField;

		[SerializeField]
		private double doubleField;

		[SerializeField]
		private decimal decimalField;

		[SerializeField]
		private int intField;

		[SerializeField]
		private uint uintField;

		[SerializeField]
		private long longField;

		[SerializeField]
		private ulong ulongField;

		[SerializeField]
		private short shortField;

		[SerializeField]
		private ushort ushortField;

		[SerializeField]
		private ExampleEnum privateEnumField;

		[SerializeField]
		private ExampleEnum1 publicEnumField;

		public Vector3 vector3Field;

		public Quaternion quaternionField;

		public Color colorField;

		[SerializeField]
		public ScriptableObject saveableScriptableObject;

		[SerializeField]
		private SaveEntityReference<StbTransform> stbTransformReference;

		[SerializeField]
		private byte[] byteArray;

		[SerializeField]
		private sbyte[] sByteArray;

		[SerializeField]
		private bool[] boolArray;

		[SerializeField]
		private string[] stringArray;

		[SerializeField]
		private char[] charArray;

		[SerializeField]
		private float[] floatArray;

		[SerializeField]
		private double[] doubleArray;

		[SerializeField]
		private decimal[] decimalArray;

		[SerializeField]
		private int[] intArray;

		[SerializeField]
		private uint[] uintArray;

		[SerializeField]
		private long[] longArray;

		[SerializeField]
		private ulong[] ulongArray;

		[SerializeField]
		private short[] shortArray;

		[SerializeField]
		private ushort[] ushortArray;

		[SerializeField]
		private ExampleEnum[] privateEnumArray;

		[SerializeField]
		private ExampleEnum1[] publicEnumArray;

		public Vector3[] vector3Array;

		public Quaternion[] quaternionArray;

		public Color[] colorArray;

		[SerializeField]
		public ScriptableObject[] saveableScriptableObjectArray;

		[SerializeField]
		private SaveEntityReference<StbTransform>[] stbTransformReferenceArray;

		[SerializeField]
		private List<byte> byteList;

		[SerializeField]
		private List<sbyte> sByteList;

		[SerializeField]
		private List<bool> boolList;

		[SerializeField]
		private List<string> stringList;

		[SerializeField]
		private List<char> charList;

		[SerializeField]
		private List<float> floatList;

		[SerializeField]
		private List<double> doubleList;

		[SerializeField]
		private List<decimal> decimalList;

		[SerializeField]
		private List<int> intList;

		[SerializeField]
		private List<uint> uintList;

		[SerializeField]
		private List<long> longList;

		[SerializeField]
		private List<ulong> ulongList;

		[SerializeField]
		private List<short> shortList;

		[SerializeField]
		private List<ushort> ushortList;

		[SerializeField]
		private List<ExampleEnum> privateEnumList;

		[SerializeField]
		private List<ExampleEnum1> publicEnumList;

		[SerializeField]
		private Stack<int> intStack = new Stack<int>();

		[SerializeField]
		private Stack<string> stringStack = new Stack<string>();

		[SerializeField]
		private Queue<int> intQueue = new Queue<int>();

		[SerializeField]
		private Queue<string> stringQueue = new Queue<string>();

		public List<Vector3> vector3List;

		public List<Quaternion> quaternionList;

		public List<Color> colorList;

		[SerializeField]
		public List<ScriptableObject> saveableScriptableObjectList;

		public float CyclicValue => CyclicValue;

		private enum ExampleEnum1
		{
			Enum1,
			Enum2,
			Enum3
		}

		private void Awake()
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

		[ContextMenu("Log stack and queue values")]
		private void LogQueueAndStackValues()
		{
			foreach (var i in intStack)
			{
				Debug.Log($"int stack: {i}");
			}

			foreach (var i in stringStack)
			{
				Debug.Log($"string stack: {i}");
			}

			foreach (var i in intQueue)
			{
				Debug.Log($"int queue: {i}");
			}

			foreach (var i in stringQueue)
			{
				Debug.Log($"string queue: {i}");
			}
		}

		[ContextMenu("Clear stack and queues")]
		private void ClearStackAndQueues()
		{
			intStack.Clear();
			stringStack.Clear();
			intQueue.Clear();
			stringQueue.Clear();
		}
	}

	public enum ExampleEnum
	{
		Enum4,
		Enum5,
		Enum6
	}
}
