using System;
using System.Collections.Generic;
using System.Linq;
using SaveToolbox.Runtime.Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace SaveToolbox.Runtime.CallbackHandlers
{
	/// <summary>
	/// A class that distributes the callbacks for saving and loading.
	/// </summary>
	public class StbCallbackDistributor
	{
		private static IEnumerable<Type> loadedCallbackTypes;
		private static IEnumerable<Type> beforeLoadedCallbackTypes;
		private static IEnumerable<Type> savedCallbackTypes;
		private static IEnumerable<Type> beforeSavedCallbackTypes;

		private readonly List<IStbSavedCallbackHandler> nonMonoBehaviourSaveCallbackHandlers = new List<IStbSavedCallbackHandler>();
		private readonly List<IStbBeforeSavedCallbackHandler> nonMonoBehaviourBeforeSaveCallbackHandlers = new List<IStbBeforeSavedCallbackHandler>();
		private readonly List<IStbLoadedCallbackHandler> nonMonoBehaviourLoadCallbackHandler = new List<IStbLoadedCallbackHandler>();
		private readonly List<IStbBeforeLoadedCallbackHandler> nonMonoBehaviourBeforeLoadCallbackHandler = new List<IStbBeforeLoadedCallbackHandler>();

		public void CacheCallbackTypes()
		{
			var currentAssemblyTypes = AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes());
			var assemblyTypes = currentAssemblyTypes.ToList();

			loadedCallbackTypes = assemblyTypes.Where(type => typeof(MonoBehaviour).IsAssignableFrom(type) && type.GetInterfaces().Contains(typeof(IStbLoadedCallbackHandler)) && !type.IsInterface);
			beforeLoadedCallbackTypes = assemblyTypes.Where(type => typeof(MonoBehaviour).IsAssignableFrom(type) && type.GetInterfaces().Contains(typeof(IStbBeforeLoadedCallbackHandler)) && !type.IsInterface);
			savedCallbackTypes = assemblyTypes.Where(type => typeof(MonoBehaviour).IsAssignableFrom(type) && type.GetInterfaces().Contains(typeof(IStbSavedCallbackHandler)) && !type.IsInterface);
			beforeSavedCallbackTypes = assemblyTypes.Where(type => typeof(MonoBehaviour).IsAssignableFrom(type) && type.GetInterfaces().Contains(typeof(IStbBeforeSavedCallbackHandler)) && !type.IsInterface);
		}

		/// <summary>
		/// When a game data is saved this should be called so it can distribute the callback.
		/// </summary>
		/// <param name="slotSaveData">The game save data that was saved.</param>
		/// <param name="allowInactiveMonoBehaviours">Should this distribute callbacks to inactive objects?</param>
		public void HandleSaved(SaveData slotSaveData, bool allowInactiveMonoBehaviours = true)
		{
			foreach (var savedCallbackType in savedCallbackTypes)
			{
				// Find all MonoBehaviours of this type.
#if STB_ABOVE_2021_3
				var savedCallbackObjects = Object.FindObjectsOfType(savedCallbackType, allowInactiveMonoBehaviours);
#else
				var savedCallbackObjects = FindObjectsOfType(savedCallbackType, allowInactiveMonoBehaviours);
#endif
				if (savedCallbackObjects == null || savedCallbackObjects.Length == 0) continue;

				foreach (var savedCallbackObject in savedCallbackObjects)
				{
					// Once found call the callbacks for loading on them all.
					var callbackHandler = savedCallbackObject as IStbSavedCallbackHandler;
					callbackHandler?.HandleDataSaved(slotSaveData);
				}

				// Any additional non MonoBehaviour callback handlers should also be added.
				foreach (var savedCallbackHandler in nonMonoBehaviourSaveCallbackHandlers)
				{
					savedCallbackHandler?.HandleDataSaved(slotSaveData);
				}
			}
		}

		/// <summary>
		/// When a game data is saved this should be called so it can distribute the callback to all relevant instances.
		/// </summary>
		/// <param name="allowInactiveMonoBehaviours">Should this distribute callbacks to inactive objects?</param>
		public void HandleBeforeSaved(bool allowInactiveMonoBehaviours = true)
		{
			foreach (var savedCallbackType in beforeSavedCallbackTypes)
			{
				// Find all MonoBehaviours of this type.
#if STB_ABOVE_2021_3
				var savedCallbackObjects = Object.FindObjectsOfType(savedCallbackType, allowInactiveMonoBehaviours);
#else
				var savedCallbackObjects = FindObjectsOfType(savedCallbackType, allowInactiveMonoBehaviours);
#endif
				if (savedCallbackObjects == null || savedCallbackObjects.Length == 0) continue;

				foreach (var savedCallbackObject in savedCallbackObjects)
				{
					// Once found call the callbacks for loading on them all.
					var callbackHandler = savedCallbackObject as IStbBeforeSavedCallbackHandler;
					callbackHandler?.HandleBeforeSaved();
				}

				// Any additional non MonoBehaviour callback handlers should also be added.
				foreach (var savedCallbackHandler in nonMonoBehaviourBeforeSaveCallbackHandlers)
				{
					savedCallbackHandler?.HandleBeforeSaved();
				}
			}
		}

		/// <summary>
		/// When a game save data is loaded this should be called so it can distribute the callback.
		/// </summary>
		/// <param name="slotSaveData">The game save data that was loaded.</param>
		/// <param name="allowInactiveMonoBehaviours">Should this distribute callbacks to inactive objects?</param>
		public void HandleLoaded(SaveData slotSaveData, bool allowInactiveMonoBehaviours = true)
		{
			foreach (var loadedCallbackType in loadedCallbackTypes)
			{
				// Find all MonoBehaviours of this type.
#if STB_ABOVE_2021_3
				var loadedCallbackObjects = Object.FindObjectsOfType(loadedCallbackType, allowInactiveMonoBehaviours);
#else
				var loadedCallbackObjects = FindObjectsOfType(loadedCallbackType, allowInactiveMonoBehaviours);
#endif

				if (loadedCallbackObjects == null || loadedCallbackObjects.Length == 0) continue;

				foreach (var loadedCallbackObject in loadedCallbackObjects)
				{
					// Once found call the callbacks for loading on them all.
					var callbackHandler = loadedCallbackObject as IStbLoadedCallbackHandler;
					callbackHandler?.HandleDataLoaded(slotSaveData);
				}
			}

			// Any additional non MonoBehaviour callback handlers should also be added.
			foreach (var loadedCallbackHandler in nonMonoBehaviourLoadCallbackHandler)
			{
				loadedCallbackHandler?.HandleDataLoaded(slotSaveData);
			}
		}

		/// <summary>
		/// When a game save data is loaded this should be called so it can distribute the callback to all relevant instances.
		/// </summary>
		/// <param name="allowInactiveMonoBehaviours">Should this distribute callbacks to inactive objects?</param>
		public void HandleBeforeLoaded(bool allowInactiveMonoBehaviours = true)
		{
			foreach (var loadedCallbackType in beforeLoadedCallbackTypes)
			{
				// Find all MonoBehaviours of this type.
#if STB_ABOVE_2021_3
				var loadedCallbackObjects = Object.FindObjectsOfType(loadedCallbackType, allowInactiveMonoBehaviours);
#else
				var loadedCallbackObjects = FindObjectsOfType(loadedCallbackType, allowInactiveMonoBehaviours);
#endif
				if (loadedCallbackObjects == null || loadedCallbackObjects.Length == 0) continue;

				foreach (var loadedCallbackObject in loadedCallbackObjects)
				{
					// Once found call the callbacks for loading on them all.
					var callbackHandler = loadedCallbackObject as IStbBeforeLoadedCallbackHandler;
					callbackHandler?.HandleBeforeDataLoaded();
				}
			}

			// Any additional non MonoBehaviour callback handlers should also be added.
			foreach (var loadedCallbackHandler in nonMonoBehaviourBeforeLoadCallbackHandler)
			{
				loadedCallbackHandler?.HandleBeforeDataLoaded();
			}
		}

		private Object[] FindObjectsOfType(Type type, bool allowInactiveMonoBehaviours)
		{
			var rootObjects = SceneManager.GetActiveScene().GetRootGameObjects();
			var returnObjects = new List<Object>();

			foreach (var rootObject in rootObjects)
			{
				var foundComponents = rootObject.GetComponentsInChildren(type, allowInactiveMonoBehaviours);
				foreach (var foundComponent in foundComponents)
				{
					returnObjects.Add(foundComponent);
				}
			}

			return returnObjects.ToArray();
		}

		/// <summary>
		/// A function to add Non-MonoBehaviour to the save callbacks list, so it will receive the save callbacks.
		/// </summary>
		/// <param name="savedCallbackHandler">The interface instance of the callback handler.</param>
		public void AddNonMonoBehaviourSaveCallbackHandler(IStbSavedCallbackHandler savedCallbackHandler)
		{
			nonMonoBehaviourSaveCallbackHandlers.Add(savedCallbackHandler);
		}

		/// <summary>
		/// A function to remove Non-MonoBehaviour to the save callbacks list, so it will no longer receive the save callbacks.
		/// </summary>
		/// <param name="savedCallbackHandler">The interface instance of the callback handler.</param>
		public void RemoveNonMonoBehaviourSaveCallbackHandler(IStbSavedCallbackHandler savedCallbackHandler)
		{
			nonMonoBehaviourSaveCallbackHandlers.Remove(savedCallbackHandler);
		}

		/// <summary>
		/// A function to add Non-MonoBehaviour to the before save callbacks list, so it will receive the save callbacks.
		/// </summary>
		/// <param name="savedCallbackHandler">The interface instance of the callback handler.</param>
		public void AddNonMonoBehaviourBeforeSaveCallbackHandler(IStbBeforeSavedCallbackHandler savedCallbackHandler)
		{
			nonMonoBehaviourBeforeSaveCallbackHandlers.Add(savedCallbackHandler);
		}

		/// <summary>
		/// A function to remove Non-MonoBehaviour to the before save callbacks list, so it will no longer receive the save callbacks.
		/// </summary>
		/// <param name="savedCallbackHandler">The interface instance of the callback handler.</param>
		public void RemoveNonMonoBehaviourBeforeSaveCallbackHandler(IStbBeforeSavedCallbackHandler savedCallbackHandler)
		{
			nonMonoBehaviourBeforeSaveCallbackHandlers.Remove(savedCallbackHandler);
		}

		/// <summary>
		/// A function to add Non-MonoBehaviour to the load callbacks list, so it will receive the load callbacks.
		/// </summary>
		/// <param name="loadedCallbackHandler">The interface instance of the callback handler.</param>
		public void AddNonMonoBehaviourLoadCallbackHandler(IStbLoadedCallbackHandler loadedCallbackHandler)
		{
			nonMonoBehaviourLoadCallbackHandler.Add(loadedCallbackHandler);
		}

		/// <summary>
		/// A function to remove Non-MonoBehaviour to the load callbacks list, so it will no longer receive the load callbacks.
		/// </summary>
		/// <param name="loadedCallbackHandler">The interface instance of the callback handler.</param>
		public void RemoveNonMonoBehaviourLoadCallbackHandler(IStbLoadedCallbackHandler loadedCallbackHandler)
		{
			nonMonoBehaviourLoadCallbackHandler.Remove(loadedCallbackHandler);
		}

		/// <summary>
		/// A function to add Non-MonoBehaviour to the load callbacks list, so it will receive the load callbacks.
		/// </summary>
		/// <param name="loadedCallbackHandler">The interface instance of the callback handler.</param>
		public void AddNonMonoBehaviourBeforeLoadCallbackHandler(IStbBeforeLoadedCallbackHandler loadedCallbackHandler)
		{
			nonMonoBehaviourBeforeLoadCallbackHandler.Add(loadedCallbackHandler);
		}

		/// <summary>
		/// A function to remove Non-MonoBehaviour to the load callbacks list, so it will no longer receive the load callbacks.
		/// </summary>
		/// <param name="loadedCallbackHandler">The interface instance of the callback handler.</param>
		public void RemoveNonMonoBehaviourBeforeLoadCallbackHandler(IStbBeforeLoadedCallbackHandler loadedCallbackHandler)
		{
			nonMonoBehaviourBeforeLoadCallbackHandler.Remove(loadedCallbackHandler);
		}
	}
}