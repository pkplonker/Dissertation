using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace RealTimeUpdateRuntime
{
	public class AssetUpdateChangeHandler : RTUCommandHandlerBase
	{
		private Dictionary<string, IAssetUpdateChangeStrategy> converters = new();
		public override string Tag { get; } = AssetPropertyChangeEventArgs.MESSAGE_IDENTIFER;

		public AssetUpdateChangeHandler()
		{
			converters =
				TypeRepository.GetTypes()
					.Where(x => x.IsSubclassOf(typeof(BaseAssetUpdateChangeStrategy)) && !x.IsAbstract)
					.Select(converterType => (IAssetUpdateChangeStrategy) Activator.CreateInstance(converterType,
						new object[] { }))
					.ToDictionary(x => x.EXTENSION, x => x, StringComparer.InvariantCultureIgnoreCase);
		}

		public override void Process(CommandHandlerArgs commandHandlerArgs, JsonSerializerSettings jsonSettings)
		{
			RTUProcessor.Enqueue(() =>
			{
				try
				{
					var args = JsonConvert.DeserializeObject<AssetPropertyChangeEventArgs>(commandHandlerArgs.Payload,
						jsonSettings);
					if (converters.TryGetValue(args.Type, out var converter))
					{
						converter.Update(commandHandlerArgs.Payload, jsonSettings);
					}
					else
					{
						RTUDebug.LogWarning($"Asset Type not yet supported {args.Type}");
					}
				}
				catch (Exception e)
				{
					RTUDebug.Log($"Failed to set property: {e.Message}");
				}
			});
		}
	}
}