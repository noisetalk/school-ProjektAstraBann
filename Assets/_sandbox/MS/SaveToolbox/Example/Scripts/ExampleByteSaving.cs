using SaveToolbox.Runtime.Core.MonoBehaviours;
using SaveToolbox.Runtime.Serialization.Binary;
using UnityEngine;

namespace SaveToolbox.Example.Scripts
{
	public class ExampleByteSaving : SaveableMonoBehaviour
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
		private sbyte[] exampleSByteArray;

		private readonly decimal exampleDecimal = 1203903129123;
		private readonly decimal[] exampleDecimalArray = {129038903, 030932132809, 01923901238123};

		public override object Serialize()
		{
			var byteWriter = new StbByteWriter();
			byteWriter.Write(exampleBool);
			byteWriter.Write(exampleString);
			byteWriter.Write(exampleChar);
			byteWriter.Write(exampleFloat);
			byteWriter.Write(exampleDouble);
			byteWriter.Write(exampleInt);
			byteWriter.Write(exampleUint);
			byteWriter.Write(exampleLong);
			byteWriter.Write(exampleUlong);
			byteWriter.Write(exampleShort);
			byteWriter.Write(exampleUshort);
			byteWriter.Write(exampleByte);
			byteWriter.Write(exampleSbyte);
			byteWriter.Write(exampleDecimal);

			byteWriter.Write(exampleBoolArray);
			byteWriter.Write(exampleStringArray);
			byteWriter.Write(exampleCharArray);
			byteWriter.Write(exampleFloatArray);
			byteWriter.Write(exampleDoubleArray);
			byteWriter.Write(exampleIntArray);
			byteWriter.Write(exampleUintArray);
			byteWriter.Write(exampleLongArray);
			byteWriter.Write(exampleUlongArray);
			byteWriter.Write(exampleShortArray);
			byteWriter.Write(exampleUshortArray);
			byteWriter.Write(exampleByteArray);
			byteWriter.Write(exampleSByteArray);
			byteWriter.Write(exampleDecimalArray);

			return byteWriter.GetByteArray();
		}

		public override void Deserialize(object data)
		{
			var byteData = (byte[])data;
			var byteReader = new StbByteReader(byteData);
			exampleBool = byteReader.ReadBool();
			exampleString = byteReader.ReadString();
			exampleChar = byteReader.ReadChar();
			exampleFloat = byteReader.ReadFloat();
			exampleDouble = byteReader.ReadDouble();
			exampleInt = byteReader.ReadInt();
			exampleUint = byteReader.ReadUint();
			exampleLong = byteReader.ReadLong();
			exampleUlong = byteReader.ReadUlong();
			exampleShort = byteReader.ReadShort();
			exampleUshort = byteReader.ReadUshort();
			exampleByte = byteReader.ReadByte();
			exampleSbyte = byteReader.ReadSbyte();

			Debug.Log("Byte Reader, Loaded Decimal:" + byteReader.ReadDecimal());

			exampleBoolArray = byteReader.ReadBoolArray();
			exampleStringArray = byteReader.ReadStringArray();
			exampleCharArray = byteReader.ReadCharArray();
			exampleFloatArray = byteReader.ReadFloatArray();
			exampleDoubleArray = byteReader.ReadDoubleArray();
			exampleIntArray = byteReader.ReadIntArray();
			exampleUintArray = byteReader.ReadUintArray();
			exampleLongArray = byteReader.ReadLongArray();
			exampleUlongArray = byteReader.ReadUlongArray();
			exampleShortArray = byteReader.ReadShortArray();
			exampleUshortArray = byteReader.ReadUshortArray();
			exampleByteArray = byteReader.ReadByteArray();
			exampleSByteArray = byteReader.ReadSbyteArray();

			var newDecimalArray = byteReader.ReadDecimalArray();
			foreach (var b in newDecimalArray)
			{
				Debug.Log("Byte Reader, Loaded Decimal Array Entry:" + b);
			}
		}
	}
}