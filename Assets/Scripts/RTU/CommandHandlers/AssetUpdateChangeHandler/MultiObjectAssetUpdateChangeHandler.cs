using System;
using Newtonsoft.Json;
using UnityEngine;
using Object = UnityEngine.Object;

namespace RealTimeUpdateRuntime
{
	public class MultiObjectAssetUpdateChangeHandler : AssetUpdateChangeHandler
	{
		public override string Tag { get; } = MultiObjectAssetChangeEventArgs.MESSAGE_IDENTIFER;

		public override void Process(CommandHandlerArgs commandHandlerArgs, JsonSerializerSettings jsonSettings)
		{
			RTUProcessor.Enqueue(() =>
			{
				try
				{
					var args = JsonConvert.DeserializeObject<MultiObjectAssetChangeEventArgs>(commandHandlerArgs.Payload,
						jsonSettings);
					var value = args.GetDeserializedValue(jsonSettings);
					var typeExtension = GetExtensionFromType(value);
					if (converters.TryGetValue(typeExtension, out var converter))
					{
						converter.MultiUpdate(commandHandlerArgs.Payload, jsonSettings);
					}
					else
					{
						RTUDebug.LogWarning($"Asset Type not yet supported {args.ValueTypeName}");
					}
				}
				catch (Exception e)
				{
					RTUDebug.Log($"Failed to set property: {e.Message}");
				}
			});
		}

		private static string GetExtensionFromType(object value)
		{
			return value switch
			{
				Material => "mat",
				PhysicMaterial => "physicmaterial",
				Texture => "png",
				_ => null
			};
		}
	}
}