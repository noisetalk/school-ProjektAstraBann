using UnityEngine;
#if STB_HAS_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#else
using UnityEngine.EventSystems;
#endif

namespace SaveToolbox.Example.Scripts.SpaceShooterExample
{
	/// <summary>
	/// This is is an event system selector. Dependant if the unity project has the new input system
	/// will determine which event system the scene will use.
	/// </summary>
	[AddComponentMenu("SaveToolbox/Example/EventSystemSelector")]
	public class EventSystemSelector : MonoBehaviour
	{
		private void Awake()
		{
#if STB_HAS_INPUT_SYSTEM
			gameObject.AddComponent<InputSystemUIInputModule>();
#else
			gameObject.AddComponent<StandaloneInputModule>();
#endif
		}
	}
}
