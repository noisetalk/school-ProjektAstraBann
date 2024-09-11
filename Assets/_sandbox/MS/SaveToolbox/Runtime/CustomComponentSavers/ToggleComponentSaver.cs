using UnityEngine.UI;

namespace SaveToolbox.Runtime.CustomComponentSavers
{
	public class ToggleComponentSaver : AbstractComponentSaver<Toggle>
	{
		public override object Serialize()
		{
			return Target.isOn;
		}

		public override void Deserialize(object data)
		{
			Target.isOn = (bool)data;
		}
	}
}