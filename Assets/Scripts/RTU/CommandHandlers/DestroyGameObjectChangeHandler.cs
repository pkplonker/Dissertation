using System;
using Newtonsoft.Json;
using UnityEngine;

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
					var go = GameObject.Find(args.ParentGameObjectPath);
					if (go == null)
					{
						throw new Exception("unable to locate gameobject");
					}
					//Object.Destroy(go);
					string gameObjectName = string.Empty;
					RTUDebug.Log("Destroyed gameobject {gameObjectName}");
				}
				catch (Exception e)
				{
					RTUDebug.Log($"Failed to destroy GameObject : {e.Message}");
				}
			});
		}
	}
}