using System.Collections.Generic;
using SaveToolbox.Runtime.BasicSaveableMonoBehaviours;
using SaveToolbox.Runtime.CallbackHandlers;
using SaveToolbox.Runtime.Core;
using SaveToolbox.Runtime.Core.MonoBehaviours;
using SaveToolbox.Runtime.Utils;
using UnityEngine;

namespace SaveToolbox.Example.Scripts
{
	public class ExampleReferenceSaving : AutoSaveableMonoBehaviour, IStbLoadedCallbackHandler
	{
		/// <summary>
		/// This will automatically reference again on loading.
		/// </summary>
		[SerializeField]
		private SaveEntityReference<StbTransform> saveTransform;

		[SerializeField]
		private SaveEntityReference<StbGameObject>[] stbGameObjectsArray;

		[SerializeField]
		private List<SaveEntityReference<StbGameObject>> stbGameObjectsList;

		/// <summary>
		/// Logs in the console if the fields references were loaded by logging out their length.
		/// </summary>
		/// <param name="slotSaveData">The slot save data that was just loaded.</param>
		void IStbLoadedCallbackHandler.HandleDataLoaded(SaveData slotSaveData)
		{
			Debug.Log("Save transform loaded value: " + (saveTransform.Reference != null));
			Debug.Log("Save game objects array length: " + stbGameObjectsArray.Length);
			Debug.Log("Save game objects list count: " + stbGameObjectsList.Count);
		}
	}
}