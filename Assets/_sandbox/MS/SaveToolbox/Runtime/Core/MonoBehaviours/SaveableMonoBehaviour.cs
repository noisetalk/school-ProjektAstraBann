using System;
using System.Collections;
using SaveToolbox.Runtime.Attributes;
using SaveToolbox.Runtime.Interfaces;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace SaveToolbox.Runtime.Core.MonoBehaviours
{
	/// <summary>
	/// An abstract class that can be inherited from to allow for easy saving of MonoBehaviours.
	/// It has basic implementations of ISaveDataEntity.
	/// </summary>
	[ExecuteAlways]
	public abstract class SaveableMonoBehaviour : MonoBehaviour, ISaveDataEntity, ISaveEntityLifecycle, ILoadableObjectTarget, ISaveEntityDeserializationPriority
	{
		private const string PASTED_STACK_TRACE_VALUE = "UnityEditor.SceneHierarchy.PasteGO";
		private const string DUPLICATED_STACK_TRACE_VALUE = "UnityEditor.SceneHierarchy.DuplicateGO";
		private const string DRAGGED_FROM_ASSETS_VALUE = "UnityEditorInternal.InternalEditorUtility.HierarchyWindowDragByID";
		private const string INSTANTIATED_STACK_TRACE_VALUE = "UnityEngine.Object.Instantiate";
		private const string INSTANTIATED_PREFAB_STACK_TRACE_VALUE = "UnityEditor.PrefabUtility.InstantiatedPrefab";

		[field: SerializeField, ReadOnly]
		public string SaveIdentifier { get; set; } = Guid.NewGuid().ToString();

		public int DeserializationPriority { get; set; }

		public string LoadableObjectId { get; set; }

		public abstract object Serialize();
		public abstract void Deserialize(object data);

		protected virtual void Awake()
		{
			var environmentStackTrace = Environment.StackTrace;
			// Check the stack trace to figure out how the loadable object was created.
			if (environmentStackTrace.Contains(PASTED_STACK_TRACE_VALUE) ||
			    environmentStackTrace.Contains(DUPLICATED_STACK_TRACE_VALUE) ||
			    environmentStackTrace.Contains(INSTANTIATED_STACK_TRACE_VALUE) ||
			    environmentStackTrace.Contains(INSTANTIATED_PREFAB_STACK_TRACE_VALUE) ||
			    environmentStackTrace.Contains(DRAGGED_FROM_ASSETS_VALUE))
			{
#if UNITY_EDITOR
				// If it's not a prefab just refresh.
				if (PrefabUtility.GetPrefabAssetType(this) == PrefabAssetType.NotAPrefab)
				{
					RefreshIdentifier();
				}
				else
				{
					// Something strange happens with prefabs when duplicated, their changed data gets reverted if done in the first frame,
					// wait until the end or the next frame
					if (Application.isPlaying)
					{
						StartCoroutine(RegenerateIdentifierAtEndOfFrame());
					}
					else
					{
						StartCoroutine(RegenerateIdentifierAfterFrame());
					}
				}
#else
				RefreshIdentifier();
#endif
			}
		}

		private IEnumerator RegenerateIdentifierAtEndOfFrame()
		{
			yield return new WaitForEndOfFrame();
			RefreshIdentifier();
		}

		private IEnumerator RegenerateIdentifierAfterFrame()
		{
			yield return null;
			RefreshIdentifier();
		}

		/// <summary>
		/// This is to ensure that when we add the component to the gameobject it registers itself with the loadable object.
		/// This should also happen at runtime due to the class attribute "ExecuteAlways".
		/// </summary>
		protected virtual void Reset()
		{
			SaveIdentifier = Guid.NewGuid().ToString();

			var loadableObjects = transform.root.GetComponentsInChildren<LoadableObject>();
			foreach (var loadableObject in loadableObjects)
			{
				loadableObject.UpdateReferencedBehaviours();
			}
		}

		/// <summary>
		/// Creates a new SaveIdentifier. It will be generated as a Guid and cast to a string.
		/// </summary>
		[ContextMenu("Refresh SaveIdentifier (WARNING: Can break existing saves.)")]
		private void RefreshIdentifier()
		{
			SaveIdentifier = Guid.NewGuid().ToString();
		}

		public virtual void OnSaveCompleted() {}
		public virtual void OnLoadingSpawned() {}
		public virtual void OnLoadCompleted() {}
	}
}
