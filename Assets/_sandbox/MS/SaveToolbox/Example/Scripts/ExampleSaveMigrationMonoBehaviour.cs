using SaveToolbox.Runtime.Core;
using UnityEngine;

namespace SaveToolbox.Example.Scripts
{
	/// <summary>
	/// An example of how a custom migrator can be implemented into the SaveToolboxSystem.
	/// </summary>
	public class ExampleSaveMigrationMonoBehaviour : MonoBehaviour
	{
		private void Awake()
		{
			SaveToolboxSystem.saveMigrator = new ExampleSaveMigrator();
		}
	}
}
