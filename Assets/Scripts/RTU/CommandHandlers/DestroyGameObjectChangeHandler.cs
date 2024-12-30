using System;
using System.Collections;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace RealTimeUpdateRuntime
{
	public class DestroyGameObjectChangeHandler : RTUCommandHandlerBase
	{
		public override string Tag { get; } = "ComponentChange";

		public override void Process(CommandHandlerArgs commandHandlerArgs, JsonSerializerSettings jsonSettings)
		{
			RTUProcessor.Enqueue(() =>
			{
				try
				{
					var args = JsonConvert.DeserializeObject<DestroyGameObjectChangeArgs>(commandHandlerArgs.Payload,
						jsonSettings);
					var go = GameObject.Find(args.ParentGameObjectPath);
				}
				catch (Exception e)
				{
					RTUDebug.Log($"Failed to destroy GameObject : {e.Message}");
				}
			});
		}
	}
}