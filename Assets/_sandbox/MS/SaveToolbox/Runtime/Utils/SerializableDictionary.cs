using System;
using System.Collections.Generic;
using SaveToolbox.Runtime.Attributes;
using UnityEngine;

namespace SaveToolbox.Runtime.Utils
{
	/// <summary>
	/// A basic serializable dictionary.
	/// </summary>
	/// <typeparam name="TKey">The type of the key.</typeparam>
	/// <typeparam name="TValue">The type of the value.</typeparam>
	[Serializable]
	public class SerializableDictionary<TKey, TValue>
	{
		[field: SerializeField, StbSerialize]
		public List<TKey> Keys { get; private set; } = new List<TKey>();

		[field: SerializeField, StbSerialize]
		public List<TValue> Values { get; private set; } = new List<TValue>();

		public int Count => Keys.Count;

		public void Add(TKey key, TValue value)
		{
			Keys.Add(key);
			Values.Add(value);
		}

		public bool TryRemove(TKey key)
		{
			if (ContainsKey(key))
			{
				var index = Keys.IndexOf(key);
				Keys.RemoveAt(index);
				Values.RemoveAt(index);
				return true;
			}

			return false;
		}

		public bool ContainsKey(TKey key)
		{
			return Keys.Contains(key);
		}

		public void Clear()
		{
			Keys.Clear();
			Values.Clear();
		}
	}
}
