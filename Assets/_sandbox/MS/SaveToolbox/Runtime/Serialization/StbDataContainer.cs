using System;
using System.Collections.Generic;
using SaveToolbox.Runtime.Attributes;
using UnityEngine;

namespace SaveToolbox.Runtime.Serialization
{
	[Serializable]
	public class StbDataContainer
	{
		[SerializeField, StbSerialize]
		private List<StbDataElement> serializableObjectsList = new List<StbDataElement>();

		public void AddKeyValueEntry(string key, object value)
		{
			foreach (var stbDataElement in serializableObjectsList)
			{
				if (stbDataElement.Key == key)
				{
					Debug.LogError("Could not add key value pair to StbDataContainer. Ensure a object of the same key does not already exist.");
					return;
				}
			}

			serializableObjectsList.Add(new StbDataElement(key, value));
		}

		public bool ContainsKey(string key)
		{
			foreach (var stbDataElement in serializableObjectsList)
			{
				if (stbDataElement.Key == key)
				{
					return true;
				}
			}

			return false;
		}

		public T GetValue<T>(string key)
		{
			foreach (var stbDataElement in serializableObjectsList)
			{
				if (stbDataElement.Key == key)
				{
					if (stbDataElement.Value is T castType)
					{
						return castType;
					}

					Debug.LogError($"Tried to get a value that was not of the correct type of: {typeof(T)}");
				}
			}

			return default;
		}

		public object GetValue(string key)
		{
			foreach (var stbDataElement in serializableObjectsList)
			{
				if (stbDataElement.Key == key)
				{
					return stbDataElement.Value;
				}
			}

			return null;
		}

		public bool TryGetValue(string key, out object value)
		{
			value = null;
			foreach (var stbDataElement in serializableObjectsList)
			{
				if (stbDataElement.Key == key)
				{
					value = stbDataElement.Value;
					return true;
				}
			}

			return false;
		}

		public bool TryGetValue<T>(string key, out T value)
		{
			value = default;
			foreach (var stbDataElement in serializableObjectsList)
			{
				if (stbDataElement.Key == key)
				{
					if (stbDataElement.Value is T castType)
					{
						value = castType;
						return true;
					}

					Debug.LogError($"Tried to get a value that was not of the correct type of: {typeof(T)}");
				}
			}

			return false;
		}
	}

	[Serializable]
	public class StbDataElement
	{
		[field: SerializeField, StbSerialize]
		public string Key { get; private set; }

		[field: SerializeField, StbSerialize]
		public object Value { get; private set; }

		public StbDataElement() {}

		public StbDataElement(string key, object value)
		{
			Key = key;
			Value = value;
		}
	}
}