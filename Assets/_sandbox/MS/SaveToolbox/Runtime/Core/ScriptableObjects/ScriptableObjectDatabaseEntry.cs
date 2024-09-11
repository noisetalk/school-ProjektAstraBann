using System;
using Unity.Collections;
using UnityEditor;
using UnityEngine;

namespace SaveToolbox.Runtime.Core.ScriptableObjects
{
	[Serializable]
	public class ScriptableObjectDatabaseEntry
	{
		[SerializeField]
		private ScriptableObject scriptableObject;
		public ScriptableObject ScriptableObject
		{
			get => scriptableObject;
			set
			{
				scriptableObject = value;
#if UNITY_EDITOR
				if (scriptableObject != null && AssetDatabase.TryGetGUIDAndLocalFileIdentifier(scriptableObject, out var guid, out long localFileIdentifier))
				{
					scriptableObjectAssetGuid = guid;
				}
				else // If it is null or something has gone wrong.
				{
					scriptableObjectAssetGuid = string.Empty;
				}
#endif
			}
		}

		[field: SerializeField, ReadOnly]
		private string scriptableObjectAssetGuid;
		public string ScriptableObjectAssetGuid => scriptableObjectAssetGuid;
	}
}