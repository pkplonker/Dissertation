using System;
using Newtonsoft.Json;
using UnityEngine;
using Object = UnityEngine.Object;

namespace RealTimeUpdateRuntime
{
	public class DestroyGameObjectChangeHandler : RTUCommandHandlerBase
	{
		public override string Tag { get; } = DestroyGameObjectChangeArgs.MESSAGE_IDENTIFER;

		public override void Process(CommandHandlerArgs commandHandlerArgs, JsonSerializerSettings jsonSettings)
		{
			RTUProcessor.Enqueue(() =>
			{
				try
				{
					var args = JsonConvert.DeserializeObject<DestroyGameObjectChangeArgs>(commandHandlerArgs.Payload,
						jsonSettings);
					var go = GameObject.Find(args.GameObjectName);

					if (go == null)
					{
						throw new Exception("unable to locate GameObject");
					}

					var goName = go.name;
					Object.Destroy(go);

					RTUDebug.Log($"Destroyed GameObject {goName}");
				}
				catch (Exception e)
				{
					RTUDebug.Log($"Failed to destroy GameObject : {e.Message}");
				}
			});
		}
	}
}