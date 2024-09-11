using System;
using SaveToolbox.Runtime.Interfaces;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SaveToolbox.Runtime.Utils
{
	[Serializable]
	public class SaveEntityReference<T> : IEquatable<T>, IEquatable<SaveEntityReference<T>> where T : ISaveDataEntity
	{
		[field: SerializeField]
		public MonoBehaviour EntityReference { get; private set; }

		public string Identifier => Reference != null ? Reference.SaveIdentifier : string.Empty;
		public T Reference => EntityReference != null ? (T)(EntityReference as ISaveDataEntity) : default;

		// Equality operators.
		public static bool operator ==(SaveEntityReference<T> reference1, T reference2) => reference1.Equals(reference2);
		public static bool operator ==(T reference1, SaveEntityReference<T> reference2) => reference2 == reference1;
		public static bool operator ==(SaveEntityReference<T> reference1, Object reference2) => reference1.Equals(reference2);
		public static bool operator ==(Object reference1, SaveEntityReference<T> reference2) => reference2 == reference1;

		public static bool operator !=(SaveEntityReference<T> reference1, T reference2) => !(reference1 == reference2);
		public static bool operator !=(T reference1, SaveEntityReference<T> reference2) => reference2 != reference1;
		public static bool operator !=(SaveEntityReference<T> reference1, Object reference2) => !(reference1 == reference2);
		public static bool operator !=(Object reference1, SaveEntityReference<T> reference2) => reference2 != reference1;

		public override int GetHashCode()
		{
			return EntityReference != null ? EntityReference.GetHashCode() : 0;
		}

		public override bool Equals(object obj)
		{
			return obj is SaveEntityReference<T> otherReference && Equals(otherReference);
		}

		public bool Equals(SaveEntityReference<T> otherReference)
		{
			return otherReference != null && Equals(EntityReference, otherReference.EntityReference);
		}

		public bool Equals(Object other)
		{
			return EntityReference == other;
		}

		public bool Equals(T other)
		{
			return Equals(Reference, other);
		}
	}
}