using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using RealTimeUpdateRuntime;
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

	public class TextureAssetChangePayloadStrategy : DefaultAssetChangePayloadStrategy
	{
		public override bool TryGenerateArgs(Clone originalClone, Clone currentClone,
			out AssetPropertyChangeEventArgs args)
		{
			// TODO needs updating to handle png change
			return base.TryGenerateArgs(originalClone, currentClone, out args);
		}
	}

	public class DefaultAssetChangePayloadStrategy : IAssetChangePayloadStrategy
	{
		public virtual bool TryGenerateArgs(Clone originalClone, Clone currentClone,
			out AssetPropertyChangeEventArgs args)
		{
			if (HasChange(originalClone, currentClone, out var changes))
			{
				UpdateAssetStoreWithLatest(currentClone);
				args = CreateArgs(changes);
				return true;
			}

			args = null;
			return false;
		}

		private void UpdateAssetStoreWithLatest(Clone currentClone)
		{
			RTUAssetStore.UpdateClone(currentClone);
		}

		protected virtual AssetPropertyChangeEventArgs CreateArgs(Dictionary<string, object> changes)
		{
			var args = new AssetPropertyChangeEventArgs()
			{
				Changes = changes,
			};
			return args;
		}

		protected virtual bool HasChange(Clone originalClone, Clone currentClone,
			out Dictionary<string, object> changes)
		{
			// working on the assumption that only the values change.
			changes = originalClone.Except(currentClone).ToDictionary(x => x.Key, x => x.Value);
			return changes.Any();
		}
	}

	public interface IAssetChangePayloadStrategy
	{
		bool TryGenerateArgs(Clone originalClone, Clone currentClone, out AssetPropertyChangeEventArgs args);
	}
}