using System;
using System.Collections;
using Newtonsoft.Json;
using UnityEngine;
using Object = UnityEngine.Object;

namespace RealTimeUpdateRuntime
{
	public class GameObjectStructureChangeHandler : RTUCommandHandlerBase
	{
		public override string Tag { get; } = GameObjectStructureChangeArgs.MESSAGE_IDENTIFER;

		public override void Process(CommandHandlerArgs commandHandlerArgs, JsonSerializerSettings jsonSettings)
		{
			RTUProcessor.Enqueue(() =>
			{
				try
				{
					var args = JsonConvert.DeserializeObject<GameObjectStructureChangeArgs>(commandHandlerArgs.Payload,
						jsonSettings);
					var go = GameObject.Find(args.GameObjectPath);
					var componentType = args.ComponentTypeName.GetTypeIncludingUnity();
					if (args.ComponentTypeName.Equals("UnityEngine.Transform",
						    StringComparison.InvariantCultureIgnoreCase))
					{
						// ignore transform as all GameObjects inherently have a transform.
						return;
					}

					if (componentType == null)
					{
						throw new Exception("Failed to determine type for GameObject structure change");
					}

					if (args.IsAdd)
					{
						var c = go.AddComponent(componentType);
						if (c != null)
							RTUDebug.Log($"Added {args.ComponentTypeName} to {go.name}");
						else
						{
							throw new Exception($"Failed to add component {componentType}");
						}
					}
					else
					{
						if (go.TryGetComponent(componentType, out var component))
						{
							Object.Destroy(component);
							RTUDebug.Log($"Removed {args.ComponentTypeName} from {go.name}");
						}
					}
				}
				catch (Exception e)
				{
					RTUDebug.Log($"Failed to update GameObject structure: {e.Message} : {e?.InnerException}");
				}
			});
		}
	}
}