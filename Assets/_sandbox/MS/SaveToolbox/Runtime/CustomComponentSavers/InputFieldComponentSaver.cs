using UnityEngine.UI;

namespace SaveToolbox.Runtime.CustomComponentSavers
{
	public class InputFieldComponentSaver : AbstractComponentSaver<InputField>
	{
		public override object Serialize()
		{
			return Target.text;
		}

		public override void Deserialize(object data)
		{
			Target.text = (string)data;
		}
	}
}