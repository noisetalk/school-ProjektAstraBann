using System;
using SaveToolbox.Runtime.Attributes;
using SaveToolbox.Runtime.Core.MonoBehaviours;
using UnityEngine;

namespace SaveToolbox.Example.Scripts
{
	/// <summary>
	/// Shows an example of how the StbFormerlySerializedAs attribute works, uses the PreviouslySerializedAsExampleData struct.
	/// </summary>
	public class ExamplePreviouslySerializedAs : SaveableMonoBehaviour
	{
		[SerializeField]
		private float health = 5f;

		[SerializeField]
		private float damage = 19f;

		public override object Serialize()
		{
			return new PreviouslySerializedAsExampleData(health, damage);
		}

		public override void Deserialize(object data)
		{
			var cachedData = (PreviouslySerializedAsExampleData)data;
			health = cachedData.health;
			damage = cachedData.damage;
		}
	}

	/// <summary>
	/// Shows an example of how the StbFormerlySerializedAs attribute works. Has the old assembly namespace as a parameter on the attribute.
	/// </summary>
	[Serializable, StbFormerlySerializedAs("SaveToolbox.Runtime.Example.PreviousDataExample, SaveToolbox, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null")]
	public struct PreviouslySerializedAsExampleData
	{
		public float health;
		public float damage;

		public PreviouslySerializedAsExampleData(float health, float damage)
		{
			this.health = health;
			this.damage = damage;
		}
	}
}
