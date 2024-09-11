using System;
using System.IO;
using SaveToolbox.Runtime.Core.ScriptableObjects;
using UnityEditor;
using UnityEngine;

namespace SaveToolbox.Runtime.Utils
{
	/// <summary>
	/// A scriptable object that is also a singleton. Should only ever be 1 in a project. Will auto create itself
	/// on get of an instance. Uses resources folder to function.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public abstract class StbSingletonScriptableObject<T> : ScriptableObject, IAssetPathProvider where T : ScriptableObject
	{
		public abstract string AssetPath { get; }

		private static T instance;
		public static T Instance
		{
			get
			{
				if (instance == null)
				{
					if (Application.isPlaying)
					{
						var path = typeof(T).Name;
						var singletonObject = Resources.Load<T>(path);
						if (singletonObject == null) throw new Exception($"Could not find singleton object of type: {typeof(T)}");

						instance = singletonObject;
					}
#if UNITY_EDITOR
					else
					{
						var path = typeof(T).Name;
						var singletonObject = Resources.Load<T>(path);
						if (singletonObject != null)
						{
							instance = singletonObject;
						}
						else
						{
							// if it's null create one.
							Debug.Log($"Could not find singleton Scriptable Object of type: {typeof(T)}, attempting to create one.");

							var scriptableObject = CreateInstance<T>();
							var assetPathProvider = scriptableObject as IAssetPathProvider;
							if (assetPathProvider == null)
							{
								throw new Exception("Created instance is not a IAssetPathProvider");
							}

							var assetSavePath = $"{assetPathProvider.AssetPath}/{typeof(T).Name}.Asset";
							if (!Directory.Exists(assetPathProvider.AssetPath))
							{
								Directory.CreateDirectory(assetPathProvider.AssetPath);
							}

							// Check if anything is in the expected path first.
							var hasDeletedAssetAtTargetPath = AssetDatabase.DeleteAsset(assetSavePath);
							if (hasDeletedAssetAtTargetPath)
							{
								Debug.Log($"Found existing asset at path: {assetSavePath}, attempting to delete it. This could be because of a broken asset of type {typeof(T)}");
							}

							AssetDatabase.CreateAsset(scriptableObject, assetSavePath);
							instance = scriptableObject;
							if (scriptableObject != null && scriptableObject is ISingletonScriptableObjectInstantiationHandler scriptableObjectInstantiationHandler)
							{
								scriptableObjectInstantiationHandler.HandleInstantiation();
							}

							Debug.Log($"Could not find singleton scriptable object of type {typeof(T)}. Successfully created one in a resources folder at path {assetSavePath}.");
							AssetDatabase.SaveAssets();
							AssetDatabase.Refresh();
						}
					}
#endif
				}
				return instance;
			}
		}
	}

	public interface IAssetPathProvider
	{
		string AssetPath { get; }
	}

	public interface ISingletonScriptableObjectInstantiationHandler
	{
		void HandleInstantiation();
	}
}