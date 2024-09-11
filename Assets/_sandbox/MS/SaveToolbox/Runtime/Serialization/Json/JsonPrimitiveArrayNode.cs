using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaveToolbox.Runtime.Serialization.Json
{
	/// <summary>
	/// Only to be used for an array of primitives that are all the same types.
	/// </summary>
	internal class JsonPrimitiveArrayNode : JsonCompoundNode
	{
		// Ensure they are all the same types.
		public object[] PrimitiveChildren { get; set; } = new object[]{};

		public override void Render(StringBuilder stringBuilder, bool prettyPrint = true, int depth = 0, bool typeSerialization = true)
		{
			StbSerializationUtilities.ApplyDepth(stringBuilder, depth);

			stringBuilder.Append(!string.IsNullOrEmpty(Name) ? $"\"{Name}\": [" : "[");

			// Serialize the identifier that it is a primitive array type.
			if (prettyPrint)
			{
				stringBuilder.Append("\n");
				StbSerializationUtilities.ApplyDepth(stringBuilder, ++depth);
			}

			stringBuilder.Append("{");

			if (prettyPrint)
			{
				stringBuilder.Append("\n");
				StbSerializationUtilities.ApplyDepth(stringBuilder, ++depth);
			}

			// A way to determine if it is a primitive array node.
			stringBuilder.Append("\"IsStbPrimitiveArray\": true");

			if (prettyPrint)
			{
				stringBuilder.Append("\n");
				StbSerializationUtilities.ApplyDepth(stringBuilder, --depth);
			}

			stringBuilder.Append("}");

			//Serialize type.
			if (typeSerialization && !string.IsNullOrEmpty(Type))
			{
				stringBuilder.Append(",");

				if (prettyPrint)
				{
					stringBuilder.Append("\n");
					StbSerializationUtilities.ApplyDepth(stringBuilder, depth);
					stringBuilder.Append("{\n");
					StbSerializationUtilities.ApplyDepth(stringBuilder, ++depth);
				}
				else
				{
					stringBuilder.Append("{");
				}

				stringBuilder.Append("\"");
				stringBuilder.Append(STB_ASSEMBLY_TYPE);
				stringBuilder.Append("\": \"");
				stringBuilder.Append(Type);
				stringBuilder.Append("\"");

				if (prettyPrint)
				{
					stringBuilder.Append("\n");

					depth--;
					StbSerializationUtilities.ApplyDepth(stringBuilder, depth);
				}

				stringBuilder.Append("}");
			}

			if (PrimitiveChildren.Length > 0)
			{
				stringBuilder.Append(",");
				if (prettyPrint)
				{
					stringBuilder.Append("\n");
					StbSerializationUtilities.ApplyDepth(stringBuilder, depth);
				}
			}

			// As this should only be used for primitives of the same type, if the first element is a string then they all should be.
			var isStringType = PrimitiveChildren.Length > 0 && (PrimitiveChildren[0] is string || PrimitiveChildren[0] is char);
			var isBoolType = !isStringType && PrimitiveChildren.Length > 0 && PrimitiveChildren[0] is bool;
			for (var i = 0; i < PrimitiveChildren.Length; ++i)
			{
				var child = PrimitiveChildren[i];

				if (i < PrimitiveChildren.Length - 1)
				{
					// Appending rather than string concatenation is faster.
					if (isStringType)
					{
						stringBuilder.Append("\"");
						stringBuilder.Append(child);
						stringBuilder.Append("\",\n");
					}
					else if (isBoolType)
					{
						stringBuilder.Append(child.ToString().ToLower());
						stringBuilder.Append(",\n");
					}
					else
					{
						stringBuilder.Append(child);
						stringBuilder.Append(",\n");
					}
					StbSerializationUtilities.ApplyDepth(stringBuilder, depth);
				}
				else
				{
					if (isStringType)
					{
						stringBuilder.Append("\"");
						stringBuilder.Append(child);
						stringBuilder.Append("\"");
					}
					else if (isBoolType)
					{
						stringBuilder.Append(child.ToString().ToLower());
					}
					else
					{
						stringBuilder.Append(child);
					}
				}
			}

			if (prettyPrint)
			{
				stringBuilder.Append("\n");
				StbSerializationUtilities.ApplyDepth(stringBuilder, --depth);
			}

			stringBuilder.Append("]");
		}

		public override async Task RenderAsync(StringBuilder stringBuilder, bool prettyPrint = true, int depth = 0, bool typeSerialization = true)
		{
			Render(stringBuilder, prettyPrint, depth, typeSerialization);
			await CheckFrameTime();
		}

		public bool TryGetPrimitiveChildrenOfType<T>(out List<T> returnList)
		{
			returnList = new List<T>();
			var isPrimitive = StbSerializationUtilities.IsPrimitiveType(typeof(T));
			foreach (var jsonBaseNode in Children)
			{
				var simpleNode = jsonBaseNode as JsonSimpleNode;
				if (simpleNode == null) continue;

				if (isPrimitive)
				{
					var newValue = simpleNode.Value.ConvertPrimitive(typeof(T));
					if (newValue == null) continue;

					returnList.Add((T)newValue);
				}
			}

			return returnList.Count > 0;
		}

		public List<JsonBaseNode> GetChildNodes()
		{
			return Children.ToList();
		}

		public long[] AsLongArray()
		{
			var length = PrimitiveChildren.Length;
			var newArray = new long[length];

			Parallel.For(0, length, i => newArray[i] = PrimitiveChildren[i].AsLong());

			return newArray;
		}

		public ulong[] AsULongArray()
		{
			var length = PrimitiveChildren.Length;
			var newArray = new ulong[length];

			Parallel.For(0, length, i => newArray[i] = PrimitiveChildren[i].AsUlong());

			return newArray;
		}

		public int[] AsIntArray()
		{
			var length = PrimitiveChildren.Length;
			var newArray = new int[length];

			Parallel.For(0, length, i => newArray[i] = PrimitiveChildren[i].AsInt());

			return newArray;
		}

		public uint[] AsUIntArray()
		{
			var length = PrimitiveChildren.Length;
			var newArray = new uint[length];

			Parallel.For(0, length, i => newArray[i] = PrimitiveChildren[i].AsUint());

			return newArray;
		}

		public short[] AsShortArray()
		{
			var length = PrimitiveChildren.Length;
			var newArray = new short[length];

			Parallel.For(0, length, i => newArray[i] = PrimitiveChildren[i].AsShort());

			return newArray;
		}

		public ushort[] AsUShortArray()
		{
			var length = PrimitiveChildren.Length;
			var newArray = new ushort[length];

			Parallel.For(0, length, i => newArray[i] = PrimitiveChildren[i].AsUshort());

			return newArray;
		}

		public decimal[] AsDecimalArray()
		{
			var length = PrimitiveChildren.Length;
			var newArray = new decimal[length];

			Parallel.For(0, length, i => newArray[i] = PrimitiveChildren[i].AsDecimal());

			return newArray;
		}

		public bool[] AsBoolArray()
		{
			var length = PrimitiveChildren.Length;
			var newArray = new bool[length];

			Parallel.For(0, length, i => newArray[i] = PrimitiveChildren[i].AsBool());

			return newArray;
		}

		public string[] AsStringArray()
		{
			var length = PrimitiveChildren.Length;
			var newArray = new string[length];

			Parallel.For(0, length, i => newArray[i] = PrimitiveChildren[i].AsString());

			return newArray;
		}

		public byte[] AsByteArray()
		{
			var length = PrimitiveChildren.Length;
			var newArray = new byte[length];

			Parallel.For(0, length, i => newArray[i] = PrimitiveChildren[i].AsByte());

			return newArray;
		}

		public char[] AsCharArray()
		{
			var length = PrimitiveChildren.Length;
			var newArray = new char[length];

			Parallel.For(0, length, i => newArray[i] = PrimitiveChildren[i].AsChar());

			return newArray;
		}

		public float[] AsFloatArray()
		{
			var length = PrimitiveChildren.Length;
			var newArray = new float[length];

			Parallel.For(0, length, i => newArray[i] = PrimitiveChildren[i].AsFloat());

			return newArray;
		}

		public double[] AsDoubleArray()
		{
			var length = PrimitiveChildren.Length;
			var newArray = new double[length];

			Parallel.For(0, length, i => newArray[i] = PrimitiveChildren[i].AsDouble());

			return newArray;
		}

		public sbyte[] AsSByteArray()
		{
			var length = PrimitiveChildren.Length;
			var newArray = new sbyte[length];

			Parallel.For(0, length, i => newArray[i] = PrimitiveChildren[i].AsSbyte());

			return newArray;
		}

		public string[] AsString()
		{
			var length = PrimitiveChildren.Length;
			var newArray = new string[length];

			Parallel.For(0, length, i => newArray[i] = PrimitiveChildren[i].AsString());

			return newArray;
		}
	}
}