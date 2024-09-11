using UnityEngine;
using UnityEngine.UI;

namespace SaveToolbox.Example.Scripts
{
	/// <summary>
	/// An example loading bar, to show how loading screens can work. Shows current progress and state.
	/// </summary>
	[AddComponentMenu("SaveToolbox/Example/ExampleLoadingBar")]
	public class ExampleLoadingBar : MonoBehaviour
	{
		[SerializeField]
		private Text loadingTextPercentage;

		[SerializeField]
		private Transform loadingBarScaler;

		public void UpdateValue(float normalizedValue)
		{
			loadingTextPercentage.text = (normalizedValue * 100).ToString("F0") + "%";
			loadingBarScaler.localScale = new Vector3(normalizedValue, 1f, 1f);
		}
	}
}
