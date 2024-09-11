using System;
using SaveToolbox.Runtime.Core.ScriptableObjects;
using UnityEditor;
using UnityEngine;

namespace SaveToolbox.Runtime.Core
{
#if UNITY_EDITOR
	/// <summary>
	/// A script to ensure the scriptable object singletons for the package are create in a resource folder.
	/// Checks if they exist and if not creates them.
	/// </summary>
	[InitializeOnLoad]
	public class ScriptLibraryInitializer
	{
		static ScriptLibraryInitializer()
		{
			// Subscribe to callback to initialize scriptable objects, because we can't do it until domain is fully reloaded.
			EditorApplication.delayCall += HandleEditorDelayCall;
		}

		private static void HandleEditorDelayCall()
		{
			// Get the instances to auto-create the files.
			try
			{
				var saveSettings = SaveToolboxPreferences.Instance;
			}
			catch (Exception e)
			{
				Debug.LogError(e);
			}

			try
			{
				var loadableObjectDatabase = LoadableObjectDatabase.Instance;
			}
			catch (Exception e)
			{
				Debug.LogError(e);
			}

			try
			{
				var scriptableObjectDatabase = ScriptableObjectDatabase.Instance;
			}
			catch (Exception e)
			{
				Debug.LogError(e);
			}
		}
	}
#endif
}
