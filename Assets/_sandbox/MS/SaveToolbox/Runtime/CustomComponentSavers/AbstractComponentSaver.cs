using System;
using SaveToolbox.Runtime.Attributes;
using SaveToolbox.Runtime.Interfaces;
using UnityEngine;

namespace SaveToolbox.Runtime.CustomComponentSavers
{
	/// <summary>
	/// A generic class that is used to apply custom savers to the Custom Component saver.
	/// Does not inherit from component or MonoBehaviour and is drawn using a custom system in the Save Toolbox.
	/// </summary>
	/// <typeparam name="T">The type of object this saver will be saving data about.</typeparam>
	[Serializable]
	public abstract class AbstractComponentSaver<T> : ISaveDataEntity, ISaveEntityDeserializationPriority where T : Component
	{
		/// <summary>
		/// A way to identify the custom saver among the other custom savers.
		/// </summary>
		[field: SerializeField, StbSerialize]
		public string SaveIdentifier { get; set; } = Guid.NewGuid().ToString();

		/// <summary>
		/// The component which is targeted and will have data of itself serialized.
		/// </summary>
		[field: SerializeField, StbSerialize]
		public T Target { get; protected set; }

		/// <summary>
		/// The priority of deserialization of this object. When higher will be deserialized first when in a list of other component savers.
		/// </summary>
		public virtual int DeserializationPriority { get; set; } = 0;

		public abstract object Serialize();
		public abstract void Deserialize(object data);
	}
}