using UnityEngine.UI;

namespace SaveToolbox.Runtime.CustomComponentSavers
{
	public class SliderComponentSaver : AbstractComponentSaver<Slider>
	{
		public override object Serialize()
		{
			var sliderValue = Target.value;
			return sliderValue;
		}

		public override void Deserialize(object data)
		{
			var sliderValue = (float)data;
			Target.value = sliderValue;
		}
	}
}