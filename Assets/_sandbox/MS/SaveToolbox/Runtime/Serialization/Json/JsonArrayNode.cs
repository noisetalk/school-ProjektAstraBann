using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaveToolbox.Runtime.Serialization.Json
{
	public class JsonArrayNode : JsonCompoundNode
	{
		public override void Render(StringBuilder stringBuilder, bool prettyPrint = true, int depth = 0, bool typeSerialization = true)
		{
			StbSerializationUtilities.ApplyDepth(stringBuilder, depth);
			stringBuilder.Append(!string.IsNullOrEmpty(Name) ? $"\"{Name}\": [" : "[");

			if (prettyPrint)
			{
				stringBuilder.Append("\n");
				depth++;
			}

			if (typeSerialization && !string.IsNullOrEmpty(Type))
			{
				if (prettyPrint)
				{
					StbSerializationUtilities.ApplyDepth(stringBuilder, depth);
					stringBuilder.Append("{\n");
					depth++;
					StbSerializationUtilities.ApplyDepth(stringBuilder, depth);
				}
				else
				{
					stringBuilder.Append("{");
				}

				stringBuilder.Append($"\"{STB_ASSEMBLY_TYPE}\": \"{Type}\"");

				if (prettyPrint)
				{
					stringBuilder.Append("\n");

					depth--;
					StbSerializationUtilities.ApplyDepth(stringBuilder, depth);
				}

				stringBuilder.Append("}");

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
					stringBuilder.Append(",\n");
				}
			}

			if (prettyPrint)
			{
				if (Children.Count > 0)
				{
					stringBuilder.Append("\n");
				}
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
	}
}