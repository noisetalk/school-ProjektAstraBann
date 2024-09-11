using System.Collections.Generic;
using SaveToolbox.Runtime.Core.MonoBehaviours;
using UnityEngine;

namespace SaveToolbox.Example.Scripts
{
	public class ExampleScriptableObjectSaving : AutoSaveableMonoBehaviour
	{
		[SerializeField]
		private ExampleSaveableScriptableObject saveableScriptableObject;

		[SerializeField]
		private List<ExampleSaveableScriptableObject> saveableScriptableObjectsList;

		[SerializeField]
		private ExampleSaveableScriptableObject[] saveableScriptableObjectsArray;

		[SerializeField]
		private ScriptableObject nonSaveableScriptableObject;
	}
}
