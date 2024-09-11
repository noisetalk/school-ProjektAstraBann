using System;

namespace SaveToolbox.Runtime.Utils
{
	public static class AssetsSavedListener
	{
		public static event Action OnAssetsSaved;

		public static void NotifyAssetsSaved()
		{
			OnAssetsSaved?.Invoke();
		}
	}
}