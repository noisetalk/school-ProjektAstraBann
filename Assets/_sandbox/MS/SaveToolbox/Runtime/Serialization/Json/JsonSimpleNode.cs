using System.Text;
using System.Threading.Tasks;

namespace SaveToolbox.Runtime.Serialization.Json
{
	public class JsonSimpleNode : JsonBaseNode
	{
		public object Value { get; set; }

		public JsonSimpleNode(object value)
		{
			Value = value;
		}

		public JsonSimpleNode(string name, object value)
		{
			Name = name;
			Value = value;
		}

		public override void Render(StringBuilder stringBuilder, bool prettyPrint = true, int depth = 0, bool typeSerialization = true)
		{
			if (prettyPrint)
			{
				StbSerializationUtilities.ApplyDepth(stringBuilder, depth);
			}

			var isStringType = Value is string || Value is char;
			var isBoolType = Value is bool;

			// Appending rather than string concatenation is faster.
			if (!string.IsNullOrEmpty(Name))
			{
				stringBuilder.Append("\"");
				stringBuilder.Append(Name);
				stringBuilder.Append("\": ");
			}

			if (isStringType)
			{
				stringBuilder.Append("\"");
				stringBuilder.Append(Value);
				stringBuilder.Append("\"");
			}
			else if (isBoolType)
			{
				stringBuilder.Append(Value.ToString().ToLower());
			}
			else
			{
				stringBuilder.Append(Value);
			}

			if (typeSerialization && !string.IsNullOrEmpty(Type))
			{
				stringBuilder.Append(",");
				if (prettyPrint)
				{
					stringBuilder.Append("\n");
				}

				StbSerializationUtilities.ApplyDepth(stringBuilder, depth);
				stringBuilder.Append("\"StbTypeAssembly\": \"");
				stringBuilder.Append(Type);
				stringBuilder.Append("\"");
			}
		}

		public override async Task RenderAsync(StringBuilder stringBuilder, bool prettyPrint = true, int depth = 0, bool typeSerialization = true)
		{
			Render(stringBuilder, prettyPrint, depth, typeSerialization);
			await CheckFrameTime();
		}

		public override int GetUnderlyingNodeCount()
		{
			return 1;
		}
	}
}