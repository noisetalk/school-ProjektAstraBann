using System;
using SaveToolbox.Runtime.Core;
using SaveToolbox.Runtime.Interfaces;
using UnityEngine;

namespace SaveToolbox.Example.Scripts
{
	/// <summary>
	/// An example of how a Non-MonoBehaviour instance can be saved as data through the game save system.
	/// A custom example of what the game settings may look like.
	/// Adds itself to the static ISaveDataEntity list in the SaveToolboxSystem on construction.
	/// </summary>
	[Serializable]
	public class ExampleNonMonoBehaviourOptionsData : ISaveDataEntity
	{
		public string SaveIdentifier { get; set; } = "OptionsData";
		public int DeserializationPriority { get; set; }
		public string LoadableObjectId { get; set; }

		public ExampleNonMonoBehaviourOptionsData()
		{
			SaveToolboxSystem.AddSaveGameDataEntity(this);
		}

		[SerializeField]
		private int resolutionIndex;
		public int ResolutionIndex => resolutionIndex;

		[SerializeField]
		private float masterVolume = 1f;
		public float MasterVolume => masterVolume;

		[SerializeField]
		private float effectsVolume = 1f;
		public float EffectsVolume => effectsVolume;

		[SerializeField]
		private float musicVolume = 1f;
		public float MusicVolume => musicVolume;

		[SerializeField]
		private int fullScreenMode;
		public int FullScreenMode => fullScreenMode;

		public void SetResolutionIndex(int resolutionIndex)
		{
			this.resolutionIndex = resolutionIndex;
		}

		public void SetMasterVolume(float masterVolume)
		{
			this.masterVolume = masterVolume;
		}

		public void SetEffectsVolume(float effectsVolume)
		{
			this.effectsVolume = effectsVolume;
		}

		public void SetMusicVolume(float musicVolume)
		{
			this.musicVolume = musicVolume;
		}

		public void SetFullscreenMode(int fullScreenMode)
		{
			this.fullScreenMode = fullScreenMode;
		}

		object ISaveDataEntity.Serialize()
		{
			return this;
		}

		void ISaveDataEntity.Deserialize(object data)
		{
			if (data is ExampleNonMonoBehaviourOptionsData optionsData)
			{
				resolutionIndex = optionsData.ResolutionIndex;
				masterVolume = optionsData.MasterVolume;
				effectsVolume = optionsData.EffectsVolume;
				musicVolume = optionsData.MusicVolume;
				fullScreenMode = optionsData.FullScreenMode;
			}
		}

		~ExampleNonMonoBehaviourOptionsData()
		{
			SaveToolboxSystem.RemoveSaveGameDataEntity(this);
		}
	}
}
