using System;
using SaveToolbox.Runtime.Core.MonoBehaviours;
using UnityEngine;
using UnityEngine.UI;

namespace SaveToolbox.Runtime.BasicSaveableMonoBehaviours
{
	/// <summary>
	/// Saves data about the UI Toggle that is referenced. Saves the current state of it.
	/// </summary>
	[AddComponentMenu("SaveToolbox/SavingBehaviours/StbToggle")]
	public class StbToggle : SaveableMonoBehaviour
	{
		[field: SerializeField]
		private Toggle toggle;

		public override object Serialize()
		{
			if (toggle == null)
			{
				if (!TryGetComponent(out toggle)) throw new Exception($"Could not serialize object of type toggle as there isn't one referenced or attached to the game object.");
			}
			return toggle.isOn;
		}

		public override void Deserialize(object data)
		{
			if (toggle == null)
			{
				if (!TryGetComponent(out toggle)) throw new Exception($"Could not deserialize object of type toggle as there isn't one referenced or attached to the game object.");
			}
			toggle.isOn = (bool)data;
		}
	}
}