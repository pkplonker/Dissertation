using System;
using System.Collections.Generic;
using RealTimeUpdateRuntime;
using RTUEditor.AssetStore;

namespace RTUEditor.ObjectChange
{
	public class AssetChangePayloadStrategyFactory
	{
		private static IAssetChangePayloadStrategy defaultAssetChangePayloadStrategies =
			new DefaultAssetChangePayloadStrategy();

		private static Dictionary<string, IAssetChangePayloadStrategy> assetChangePayloadStrategies =
			new(StringComparer.InvariantCultureIgnoreCase)
			{
				{"PNG", new TextureAssetChangePayloadStrategy()},
			};

		public bool GeneratePayload(Clone existingClone, Clone newClone, string type, out IPayload payload)
		{
			payload = null;
			var strategy = GetStrategy(type);
			if (strategy != null)
			{
				if (strategy.TryGenerateArgs(existingClone, newClone, out var args))
				{
					payload = args;
					return true;
				}
			}

			RTUDebug.LogWarning($"Failed to generate payload for {newClone}");

			return false;
		}

		private IAssetChangePayloadStrategy GetStrategy(string type) =>
			assetChangePayloadStrategies.TryGetValue(type, out var strategy)
				? strategy
				: defaultAssetChangePayloadStrategies;
	}
}