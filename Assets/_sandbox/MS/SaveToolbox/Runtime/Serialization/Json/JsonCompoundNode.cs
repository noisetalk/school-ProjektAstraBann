using System.Collections.Generic;
using System.Linq;

namespace SaveToolbox.Runtime.Serialization.Json
{
	public abstract class JsonCompoundNode : JsonBaseNode
	{
		public List<JsonBaseNode> Children { get; set; } = new List<JsonBaseNode>();

		public JsonBaseNode AddChild(JsonBaseNode node)
		{
			Children.Add(node);
			return node;
		}

		public bool TryGetChild(string childLabel, out JsonBaseNode childNode)
		{
			childNode = null;
			foreach (var child in Children)
			{
				if (string.Equals(child.Name, childLabel))
				{
					childNode = child;
					return true;
				}
			}

			return false;
		}

		public JsonBaseNode GetChild(string childLabel)
		{
			foreach (var child in Children)
			{
				if (string.Equals(child.Name, childLabel))
				{
					return child;
				}
			}

			return null;
		}

		public override int GetUnderlyingNodeCount()
		{
			return Children.Sum(child => child.GetUnderlyingNodeCount());
		}
	}
}