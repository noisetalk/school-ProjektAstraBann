using System;
using System.Collections.Generic;
using SaveToolbox.Runtime.Attributes;
using SaveToolbox.Runtime.Core.MonoBehaviours;
using SaveToolbox.Runtime.Utils;
using UnityEditor;
using UnityEngine;

namespace SaveToolbox.Runtime.Core.ScriptableObjects
{
	/// <summary>
	/// a singleton scriptable object that holds reference to all loadable objects in the project.
	/// When a loadable object is added to the database, it will have it's loadable object prefab id set to it's index in the list.
	/// This database is used to provide prefabs that should be loaded when a game save is loaded.
	/// </summary>
	[CreateAssetMenu(fileName = "LoadableObjectDatabase", menuName = "SaveToolbox/LoadableObjectDatabase", order = 1)]
	public class LoadableObjectDatabase : StbSingletonScriptableObject<LoadableObjectDatabase>, ISingletonScriptableObjectInstantiationHandler
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

		// We should not be able to edit this, needs a readonly attribute but that's not working currently.
		[SerializeField, ReadOnly]
		public List<LoadableObject> loadableObjects = new List<LoadableObject>();
#if UNITY_EDITOR
		[ContextMenu("Refresh")]
		public void RefreshDatabase()
		{
			// Get all prefabs.
			var allPrefabs = AssetDatabase.FindAssets($"t:prefab");

			for (var i = 0; i < allPrefabs.Length; i++)
			{
				var loadableObjectGuid = allPrefabs[i];
				var assetPath = AssetDatabase.GUIDToAssetPath(loadableObjectGuid);
				var loadableObjectPrefab = AssetDatabase.LoadAssetAtPath<LoadableObject>(assetPath);

				if (loadableObjectPrefab != null && !loadableObjects.Contains(loadableObjectPrefab))
				{
					loadableObjectPrefab.LoadableObjectId = loadableObjects.Count;
					loadableObjects.Add(loadableObjectPrefab);
					EditorUtility.SetDirty(loadableObjectPrefab);
				}
			}

			EditorUtility.SetDirty(this);
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
		}

		/// <summary>
		/// This will rebuild the prefab database fully. Useful if there have been any corruptions in the database
		/// or any of the prefabs.
		/// </summary>
		 [ContextMenu("Rebuild")]
		public void RebuildDatabase()
		{
			loadableObjects.Clear();
			RefreshDatabase();
		}
#endif

		/// <summary>
		/// Get a loadable object by the id, which is it's index in the list.
		/// </summary>
		/// <param name="id">The id of the loadable object, it's index in the list.</param>
		/// <returns>The loadable object of that id.</returns>
		public LoadableObject GetLoadableObjectById(int id)
		{
			return loadableObjects[id];
		}

		/// <summary>
		/// Try get a loadable object by the id, which is it's index in the list.
		/// </summary>
		/// <param name="id">The id of the loadable object, it's index in the list.</param>
		/// <param name="loadableObject">The loadable object of that id.</param>
		/// <returns>Whether or not it found the loadable object.</returns>
		public bool TryGetLoadableObjectById(int id, out LoadableObject loadableObject)
		{
			loadableObject = null;
			if (id < 0 || id > loadableObjects.Count - 1) return false;

			loadableObject = loadableObjects[id];
			return loadableObject != null;
		}

		void ISingletonScriptableObjectInstantiationHandler.HandleInstantiation()
		{
#if UNITY_EDITOR
			RefreshDatabase();
#endif
		}
	}
}
