using SaveToolbox.Runtime.Core.MonoBehaviours;
using SaveToolbox.Runtime.Utils;
using UnityEngine;

namespace SaveToolbox.Runtime.BasicSaveableMonoBehaviours
{
	/// <summary>
	/// Caches the hierarchy of the transform for the object this is attached to.
	/// Will only work if parent also have a stbHierarchy component attached.
	/// </summary>
	[AddComponentMenu("SaveToolbox/SavingBehaviours/StbHierarchy")]
	public class StbHierarchy : SaveableMonoBehaviour
	{
		public StbHierarchy()
		{
			DeserializationPriority = 2;
		}

		public override object Serialize()
		{
			var hasParent = transform.parent != null;

			if (hasParent && transform.parent.TryGetComponent<StbHierarchy>(out var stbHierarchy))
			{
				return stbHierarchy.SaveIdentifier;
			}
			if (hasParent)
			{
				Debug.LogError("Parent of a hierarchy object did not have a hierarchy object on itself, this will break the hierarchy system.");
			}

			return string.Empty;
		}

		public override void Deserialize(object data)
		{
			var id = (string)data;
			if (id != null && TryGetHierarchyComponentOfId(id, out var stbHierarchy))
			{
				transform.SetParent(stbHierarchy.transform, true);
			}
		}

		private bool TryGetHierarchyComponentOfId(string id, out StbHierarchy stbHierarchy)
		{
			stbHierarchy = null;
			var allStbHierarchyComponents = StbUtilities.GetAllObjectsInAllScenes<StbHierarchy>();
			foreach (var stbHierarchyComponent in allStbHierarchyComponents)
			{
				if (stbHierarchyComponent.SaveIdentifier == id)
				{
					stbHierarchy = stbHierarchyComponent;
					return true;
				}
			}

			return false;
		}
	}
}
