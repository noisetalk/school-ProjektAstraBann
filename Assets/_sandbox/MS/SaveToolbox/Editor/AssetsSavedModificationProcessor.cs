using SaveToolbox.Runtime.Utils;
using UnityEditor;

namespace SaveToolbox.Editor
{
	public class AssetsSavedModificationProcessor : AssetModificationProcessor
	{
		private static string[] OnWillSaveAssets(string[] assetPaths)
		{
			AssetsSavedListener.NotifyAssetsSaved();
			return assetPaths;
		}
	}
}