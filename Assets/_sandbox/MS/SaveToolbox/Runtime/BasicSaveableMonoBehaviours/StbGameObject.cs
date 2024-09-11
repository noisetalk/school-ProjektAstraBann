using System;
using SaveToolbox.Runtime.Attributes;
using SaveToolbox.Runtime.Core.MonoBehaviours;
using UnityEngine;

namespace SaveToolbox.Runtime.BasicSaveableMonoBehaviours
{
	/// <summary>
	/// Saves the states of the gameobject.
	/// </summary>
	[AddComponentMenu("SaveToolbox/SavingBehaviours/StbGameObject")]
	public class StbGameObject : SaveableMonoBehaviour
	{
		[SerializeField]
		private bool loadIsActive = true;

		[SerializeField]
		private bool loadName = true;

		[SerializeField]
		private bool loadTag = true;

		[SerializeField]
		private bool loadLayer = true;

		public StbGameObject()
		{
			DeserializationPriority = 1;
		}

		public override object Serialize()
		{
			return new GameObjectSaveData(loadIsActive, gameObject.activeSelf, loadName, name, loadTag, tag, loadLayer, gameObject.layer);
		}

		public override void Deserialize(object data)
		{
			var gameObjectSaveData = (GameObjectSaveData)data;
			if (gameObjectSaveData.LoadIsActive)
			{
				gameObject.SetActive(gameObjectSaveData.IsActive);
			}

			if (gameObjectSaveData.LoadName)
			{
				gameObject.name = gameObjectSaveData.Name;
			}

			if (gameObjectSaveData.LoadTag)
			{
				gameObject.tag = gameObjectSaveData.Tag;
			}

			if (gameObjectSaveData.LoadLayer)
			{
				gameObject.layer = gameObjectSaveData.Layer;
			}
		}
	}

	[Serializable]
	public struct GameObjectSaveData
	{
		[SerializeField, StbSerialize]
		private bool loadIsActive;
		public bool LoadIsActive => loadIsActive;

		[SerializeField, StbSerialize]
		private bool isActive;
		public bool IsActive => isActive;

		[SerializeField, StbSerialize]
		private bool loadName;
		public bool LoadName => loadName;

		[SerializeField, StbSerialize]
		private string name;
		public string Name => name;

		[SerializeField, StbSerialize]
		private bool loadTag;
		public bool LoadTag => loadTag;

		[SerializeField, StbSerialize]
		private string tag;
		public string Tag => tag;

		[SerializeField, StbSerialize]
		private bool loadLayer;
		public bool LoadLayer => loadLayer;

		[SerializeField, StbSerialize]
		private int layer;
		public int Layer => layer;

		public GameObjectSaveData(bool loadIsActive, bool isActive, bool loadName, string name, bool loadTag, string tag, bool loadLayer, int layer)
		{
			this.loadIsActive = loadIsActive;
			this.isActive = isActive;
			this.loadName = loadName;
			this.name = name;
			this.loadTag = loadTag;
			this.tag = tag;
			this.loadLayer = loadLayer;
			this.layer = layer;
		}

	}
}
