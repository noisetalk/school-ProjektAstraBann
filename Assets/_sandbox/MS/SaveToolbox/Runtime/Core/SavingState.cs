using System;

namespace SaveToolbox.Runtime.Core
{
	/// <summary>
	/// Caches the current save state of the game save process.
	/// Also caches the progression as a normalized value between 0 - 1.
	/// </summary>
	public class SavingState
	{
		public SaveState SaveState { get; set; } = SaveState.None;
		public float CurrentStateProgression { get; set; }

		public float GetOverallProgression()
		{
			switch (SaveState)
			{
				case SaveState.Failed:
				case SaveState.None:
					return 0f;
				case SaveState.SavingLoadables:
					return CurrentStateProgression / 3f;
				case SaveState.SavingNonLoadables:
					return CurrentStateProgression / 3f + 0.33f;
				case SaveState.SavingNonMonoBehaviours:
					return CurrentStateProgression / 3f + 0.66f;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}
	}

	/// <summary>
	/// A save state.
	/// </summary>
	public enum SaveState
	{
		None,
		SavingLoadables,
		SavingNonLoadables,
		SavingNonMonoBehaviours,
		Failed
	}
}