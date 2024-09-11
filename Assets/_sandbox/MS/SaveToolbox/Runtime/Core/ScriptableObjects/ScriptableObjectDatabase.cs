using System;
using System.Collections.Generic;
using SaveToolbox.Runtime.Utils;
using UnityEngine;

namespace SaveToolbox.Runtime.Core.ScriptableObjects
{
	/// <summary>
	/// a singleton scriptable object that holds reference to all scriptable objects in the project.
	/// </summary>
	[CreateAssetMenu(fileName = "ScriptableObjectDatabase", menuName = "SaveToolbox/ScriptableObjectDatabase", order = 1)]
	public class ScriptableObjectDatabase : StbSingletonScriptableObject<ScriptableObjectDatabase>
	{
		private const string ASSET_PATH = "Assets/Resources";

		public override string AssetPath
		{
			get
			{
				var scriptDirectoryParentName = ASSET_PATH;
				if (string.IsNullOrEmpty(scriptDirectoryParentName))
				{
					throw new Exception("Could not retrieve asset path.");
				}

				return scriptDirectoryParentName;
			}
		}

		[SerializeField]
		public List<ScriptableObjectDatabaseEntry> scriptableObjectEntries = new List<ScriptableObjectDatabaseEntry>();

		public ScriptableObject GetScriptableObject(string assetGuid)
		{
			foreach (var scriptableObjectEntry in scriptableObjectEntries)
			{
				if (scriptableObjectEntry.ScriptableObjectAssetGuid == assetGuid)
				{
					return scriptableObjectEntry.ScriptableObject;
				}
			}

			return null;
		}

		public bool TryGetScriptableObject(string assetGuid, out ScriptableObject scriptableObject)
		{
			scriptableObject = null;
			foreach (var scriptableObjectEntry in scriptableObjectEntries)
			{
				if (scriptableObjectEntry.ScriptableObjectAssetGuid == assetGuid)
				{
					scriptableObject = scriptableObjectEntry.ScriptableObject;
					return true;
				}
			}

			return false;
		}

		public string GetScriptableObjectGuid(ScriptableObject scriptableObject)
		{
			foreach (var scriptableObjectEntry in scriptableObjectEntries)
			{
				if (scriptableObjectEntry.ScriptableObject == scriptableObject)
				{
					return scriptableObjectEntry.ScriptableObjectAssetGuid;
				}
			}

			return string.Empty;
		}

		public bool TryGetScriptableObjectGuid(ScriptableObject scriptableObject, out string scriptableObjectAssetGuid)
		{
			scriptableObjectAssetGuid = string.Empty;

			foreach (var scriptableObjectEntry in scriptableObjectEntries)
			{
				if (scriptableObjectEntry.ScriptableObject == scriptableObject)
				{
					scriptableObjectAssetGuid = scriptableObjectEntry.ScriptableObjectAssetGuid;
					return true;
				}
			}

			return false;
		}
	}
}