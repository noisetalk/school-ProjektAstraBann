using SaveToolbox.Runtime.BasicSaveableMonoBehaviours;
using SaveToolbox.Runtime.CustomComponentSavers;
using UnityEditor;

namespace SaveToolbox.Editor
{
	[CustomPropertyDrawer(typeof(AbstractComponentSaver<>))]
	public class AbstractComponentSaverPropertyDrawer : PropertyDrawer
	{

	}
}