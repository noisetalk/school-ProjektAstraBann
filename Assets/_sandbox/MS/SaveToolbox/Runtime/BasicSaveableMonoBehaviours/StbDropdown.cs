using System;
using SaveToolbox.Runtime.Attributes;
using SaveToolbox.Runtime.Core.MonoBehaviours;
using UnityEngine;
using UnityEngine.UI;

namespace SaveToolbox.Runtime.BasicSaveableMonoBehaviours
{
	/// <summary>
	/// Saves data about the UI dropdown that is referenced.
	/// </summary>
	[AddComponentMenu("SaveToolbox/SavingBehaviours/StbComponentSaver")]
	public class StbDropdown : SaveableMonoBehaviour
	{
		/// <summary>
		/// The referenced dropdown.
		/// </summary>
		[SerializeField, StbSerialize]
		private Dropdown dropdown;

		public override object Serialize()
		{
			if (dropdown == null)
			{
				if (!TryGetComponent(out dropdown)) throw new Exception($"Could not serialize object of type dropdown as there isn't one referenced or attached to the game object.");
			}
			var dropdownValue = dropdown.value;
			return dropdownValue;
		}

		public override void Deserialize(object data)
		{
			if (dropdown == null)
			{
				if (!TryGetComponent(out dropdown)) throw new Exception($"Could not deserialize object of type dropdown as there isn't one referenced or attached to the game object.");
			}
			var dropdownValue = (int)data;
			dropdown.value = dropdownValue;
		}
	}
}
