using System;
using SaveToolbox.Runtime.Core.MonoBehaviours;
using UnityEngine;
using UnityEngine.UI;

namespace SaveToolbox.Runtime.BasicSaveableMonoBehaviours
{
	/// <summary>
	/// Saves data about the UI Slider that is referenced.
	/// </summary>
	[AddComponentMenu("SaveToolbox/SavingBehaviours/StbSlider")]
	public class StbSlider : SaveableMonoBehaviour
	{
		[SerializeField]
		private Slider slider;

		public override object Serialize()
		{
			if (slider == null)
			{
				if (!TryGetComponent(out slider)) throw new Exception($"Could not serialize object of type slider as there isn't one referenced or attached to the game object.");
			}
			var sliderValue = slider.value;
			return sliderValue;
		}

		public override void Deserialize(object data)
		{
			if (slider == null)
			{
				if (!TryGetComponent(out slider)) throw new Exception($"Could not deserialize object of type slider as there isn't one referenced or attached to the game object.");
			}
			var sliderValue = (float)data;
			slider.value = sliderValue;
		}
	}
}
