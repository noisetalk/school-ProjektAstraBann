using UnityEngine.UI;

namespace SaveToolbox.Runtime.CustomComponentSavers
{
	public class DropdownComponentSaver : AbstractComponentSaver<Dropdown>
	{
		public override object Serialize()
		{
			var dropdownValue = Target.value;
			return dropdownValue;
		}

		public override void Deserialize(object data)
		{
			var dropdownValue = (int)data;
			Target.value = dropdownValue;
		}
	}
}