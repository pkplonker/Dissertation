using System;
using System.Collections.Generic;
using Newtonsoft.Json;
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

		public bool GeneratePayload(Clone databaseClone, Clone currentClone, string type, out IPayload payload)
		{
			payload = null;
			var strategy = GetStrategy(type);
			if (strategy != null)
			{
				if (strategy.TryGenerateArgs(databaseClone, currentClone, out var args))
				{
					payload = args;
					return true;
				}
			}

			RTUDebug.LogWarning($"Failed to generate payload for {currentClone}");

			return false;
		}

		private IAssetChangePayloadStrategy GetStrategy(string type) =>
			assetChangePayloadStrategies.TryGetValue(type, out var strategy)
				? strategy
				: defaultAssetChangePayloadStrategies;
	}
}