using System;
using Newtonsoft.Json;
using UnityEngine;

namespace RealTimeUpdateRuntime
{
	public class CreateGameObjectChangeHandler : RTUCommandHandlerBase
	{
		public override string Tag { get; } = "ComponentChange";

		public override void Process(CommandHandlerArgs commandHandlerArgs, JsonSerializerSettings jsonSettings)
		{
			RTUProcessor.Enqueue(() =>
			{
				try
				{
					var args = JsonConvert.DeserializeObject<CreateGameObjectChangeArgs>(commandHandlerArgs.Payload,
						jsonSettings);
					var go = GameObject.Find(args.GameObjectPath);
					
				}
				catch (Exception e)
				{
					RTUDebug.Log($"Failed to add GameObject : {e.Message}");
				}
			});
		}
	}
}