using System;
using System.Text;
using System.Threading.Tasks;
using SaveToolbox.Runtime.Core.ScriptableObjects;
using UnityEngine;

namespace SaveToolbox.Runtime.Serialization.Json
{
	public abstract class JsonBaseNode
	{
		protected const string STB_ASSEMBLY_TYPE = "StbTypeAssembly";

		private static float currentFrameTime;
		private static float startFrameTime;

		public string Type { get; set; }
		public string Name { get; set; }

		/// <summary>
		/// Converts a node and it's enclosed data into the type that is passed in.
		/// </summary>
		/// <param name="type">The requested type.</param>
		/// <param name="baseNode">The node which contains the data to build the type.</param>
		/// <returns>An object of the specified type.</returns>
		/// <exception cref="Exception"></exception>
		public bool TryGetType(out Type type)
		{
			type = null;

			type = System.Type.GetType(Type);
			if (type == null)
			{
				StbSerializationUtilities.TryGetFormerType(Type, out type);
			}

			if (SaveToolboxPreferences.Instance.LoggingEnabled && type == null)
			{
				Debug.LogWarning($"Failed to find type.");
			}

			return type != null;
		}

		public abstract void Render(StringBuilder stringBuilder, bool prettyPrint = true, int depth = 0, bool typeSerialization = true);
		public abstract Task RenderAsync(StringBuilder stringBuilder, bool prettyPrint = true, int depth = 0, bool typeSerialization = true);
		public abstract int GetUnderlyingNodeCount();

		protected async Task CheckFrameTime()
		{
			currentFrameTime = Time.realtimeSinceStartup;
			var difference = currentFrameTime - startFrameTime;
			if (difference > 1f / SaveToolboxPreferences.Instance.LowestAcceptableLoadingFrameRate)
			{
				await Task.Yield();
				startFrameTime = Time.realtimeSinceStartup;
			}
		}
	}
}