using System;
using SaveToolbox.Runtime.Core;
using UnityEngine;

namespace SaveToolbox.Example.Scripts.MetaDataExample
{
	[Serializable]
	public class ExampleMetaSaveData : SaveMetaData
	{
		[SerializeField]
		private int slotIndex;
		public int SlotIndex => slotIndex;

		[SerializeField]
		private Texture2D imageTexture;
		public Texture2D ImageTexture => imageTexture;

		[SerializeField]
		private string saveTime;
		public string SaveTime => saveTime;

		public ExampleMetaSaveData() {}

		public ExampleMetaSaveData(Texture2D imageTexture, string saveTime, int slotIndex, double saveVersion = 0.1, object metaData = null) : base(saveVersion, metaData)
		{
			this.slotIndex = slotIndex;
			this.imageTexture = imageTexture;
			this.saveTime = saveTime;
		}
	}
}