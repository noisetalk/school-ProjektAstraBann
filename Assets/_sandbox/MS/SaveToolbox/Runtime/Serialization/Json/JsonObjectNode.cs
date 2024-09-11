using System.Text;
using System.Threading.Tasks;

namespace SaveToolbox.Runtime.Serialization.Json
{
	public class JsonObjectNode : JsonCompoundNode
	{
		public JsonBaseNode AddSimpleChild(string name, object value)
		{
			var simpleNode = new JsonSimpleNode(name, value);
			return AddChild(simpleNode);
		}

		public override void Render(StringBuilder stringBuilder, bool prettyPrint = true, int depth = 0, bool typeSerialization = true)
		{
			StbSerializationUtilities.ApplyDepth(stringBuilder, depth);
			if (!string.IsNullOrEmpty(Name))
			{
				stringBuilder.Append("\"");
				stringBuilder.Append(Name);
				stringBuilder.Append("\": ");
			}
			stringBuilder.Append("{");
			if (prettyPrint)
			{
				stringBuilder.Append("\n");
				++depth;
			}

			if (typeSerialization && !string.IsNullOrEmpty(Type))
			{
				if (prettyPrint)
				{
					StbSerializationUtilities.ApplyDepth(stringBuilder, depth);
				}
				stringBuilder.Append("\"");
				stringBuilder.Append(STB_ASSEMBLY_TYPE);
				stringBuilder.Append("\": \"");
				stringBuilder.Append(Type);
				stringBuilder.Append("\"");

				if (Children.Count > 0)
				{
					stringBuilder.Append(",");
				}
				if (prettyPrint)
				{
					stringBuilder.Append("\n");
				}
			}

			for (var i = 0; i < Children.Count; ++i)
			{
				var child = Children[i];
				child.Render(stringBuilder, prettyPrint, depth, typeSerialization);
				if (i < Children.Count - 1)
				{
					stringBuilder.Append(",");
				}
				if (prettyPrint)
				{
					stringBuilder.Append("\n");
				}
			}

			if (prettyPrint)
			{
				StbSerializationUtilities.ApplyDepth(stringBuilder, --depth);
			}

			stringBuilder.Append("}");
		}

		public override async Task RenderAsync(StringBuilder stringBuilder, bool prettyPrint = true, int depth = 0, bool typeSerialization = true)
		{
			Render(stringBuilder, prettyPrint, depth, typeSerialization);
			await CheckFrameTime();
		}
	}
}