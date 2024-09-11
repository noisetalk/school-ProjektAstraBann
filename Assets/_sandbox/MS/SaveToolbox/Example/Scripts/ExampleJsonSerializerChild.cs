using System;
using SaveToolbox.Example.Scripts.UnitTesting;
using SaveToolbox.Runtime.Serialization.Binary;
using SaveToolbox.Runtime.Serialization.Json;
using UnityEngine;

namespace SaveToolbox.Example.Scripts
{
	/// <summary>
	/// An example of a custom JsonSerializer. Implements a way to serialize a spriterenderer.
	/// Serializes the sprite name only to find the relevant sprite renderer. Not very practical but a good
	/// example for what is possible.
	/// </summary>
	public class ExampleJsonSerializerChild : StbJsonSerializer
	{
		protected override bool CanSerialize(Type type)
		{
			return type == typeof(SpriteRenderer) || type == typeof(ExampleSerializableMonoBehaviour) || base.CanSerialize(type);
		}

		protected override JsonBaseNode Serialize(object objectValue)
		{
			var objectNode = new JsonObjectNode();
			switch (objectValue)
			{
				case SpriteRenderer spriteRenderer:
				{
					objectNode.AddSimpleChild("Sprite name:", spriteRenderer.sprite.name);
					return objectNode;
				}
				case ExampleSerializableMonoBehaviour exampleSerializableMonoBehaviour:
				{
					objectNode.AddSimpleChild("stringName:", exampleSerializableMonoBehaviour.exampleSerializableString);
					return objectNode;
				}
			}

			base.Serialize(objectValue);
			return objectNode;
		}

		protected override object Deserialize(Type type, JsonCompoundNode jsonCompoundNode)
		{
			if (type == typeof(SpriteRenderer))
			{
				var spriteName = (JsonSimpleNode)jsonCompoundNode.GetChild("Sprite name:");
				if (spriteName.Value is string)
				{
					return new SpriteRenderer();
				}
			}

			if (type == typeof(ExampleSerializableMonoBehaviour))
			{
				var stringNameNode = (JsonSimpleNode)jsonCompoundNode.GetChild("stringName:");
				var newGameObject = new GameObject();
				newGameObject.AddComponent<ExampleSerializableMonoBehaviour>();
				var exampleSerializableMonoBehaviour = newGameObject.GetComponent<ExampleSerializableMonoBehaviour>();
				exampleSerializableMonoBehaviour.exampleSerializableString = (string)stringNameNode.Value;
				return exampleSerializableMonoBehaviour;
			}

			return base.Deserialize(type, jsonCompoundNode);
		}
	}
}
