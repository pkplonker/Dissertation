using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using RTUEditor.AssetStore;
using UnityEngine;

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

		public bool GeneratePayload(Clone databaseClone, Clone currentClone, string type, out string payload)
		{
			payload = null;
			var strategy = GetStrategy(type);
			if (strategy != null)
			{
				if (strategy.TryGenerateArgs(databaseClone, currentClone, out var args))
				{
					string argsData = null;
					try
					{
						argsData = JsonConvert.SerializeObject(args);
					}
					catch (Exception e)
					{
						Debug.LogError($"Failed to serialize {argsData} due to : {e.Message}");
					}

					payload = $"assetUpdate,\n{argsData}";
					return true;
				}
			}

			Debug.LogWarning($"Failed to generate payload for {currentClone}");

			return false;
		}

		private IAssetChangePayloadStrategy GetStrategy(string type) =>
			assetChangePayloadStrategies.TryGetValue(type, out var strategy)
				? strategy
				: defaultAssetChangePayloadStrategies;
	}
}