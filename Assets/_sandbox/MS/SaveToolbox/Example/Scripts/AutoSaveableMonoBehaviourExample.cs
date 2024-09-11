using System;
using SaveToolbox.Runtime.Core.MonoBehaviours;
using UnityEngine;

namespace SaveToolbox.Example.Scripts
{
	public class AutoSaveableMonoBehaviourExample : AutoSaveableMonoBehaviour
	{
		[SerializeField]
		private CustomSaveableMonoBehaviourData customSaveData;
	}

	[Serializable]
	public class CustomSaveableMonoBehaviourData
	{
		[SerializeField]
		private int exampleInt;

		[SerializeField]
		private Vector2 exampleVector2;

		[SerializeField]
		private Vector3 exampleVector3;

		[SerializeField]
		private Vector4 exampleVector4;
	}
}