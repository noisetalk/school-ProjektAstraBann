using System;

namespace SaveToolbox.Runtime.Core
{
	/// <summary>
	/// Caches the current loadstate of the game save load process.
	/// Also caches the progression as a normalized value between 0 - 1.
	/// </summary>
	public class LoadingState
	{
		public LoadState LoadState { get; set; } = LoadState.None;
		public float CurrentStateProgression { get; set; }

		public float GetOverallProgression()
		{
			switch (LoadState)
			{
				case LoadState.Failed:
				case LoadState.None:
					return 0f;
				case LoadState.LoadingObjects:
					return CurrentStateProgression / 2f;
				case LoadState.ApplyingData:
					return CurrentStateProgression / 2f + 0.5f;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}
	}

	/// <summary>
	/// A load state.
	/// </summary>
	public enum LoadState
	{
		None,
		LoadingObjects,
		ApplyingData,
		Failed
	}
}