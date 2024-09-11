using System;
using SaveToolbox.Runtime.Core.MonoBehaviours;
using UnityEngine;
using UnityEngine.UI;

namespace SaveToolbox.Runtime.BasicSaveableMonoBehaviours
{
	[AddComponentMenu("SaveToolbox/SavingBehaviours/StbInputField")]
	public class StbInputField : SaveableMonoBehaviour
	{
		[SerializeField]
		private InputField inputField;

		public override object Serialize()
		{
			if (inputField == null)
			{
				if (!TryGetComponent(out inputField)) throw new Exception($"Could not serialize object of type textField as there isn't one referenced or attached to the game object.");
			}
			return inputField.text;
		}

		public override void Deserialize(object data)
		{
			if (inputField == null)
			{
				if (!TryGetComponent(out inputField)) throw new Exception($"Could not deserialize object of type textField as there isn't one referenced or attached to the game object.");
			}

			inputField.text = (string)data;
		}
	}
}