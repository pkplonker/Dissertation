using System;
using Newtonsoft.Json;
using UnityEngine;

namespace RealTimeUpdateRuntime
{
	public class ReparentGameObjectChangeHandler : RTUCommandHandlerBase
	{
		public override string Tag { get; } = ReparentGameObjectChangeArgs.MESSAGE_IDENTIFER;

		public override void Process(CommandHandlerArgs commandHandlerArgs, JsonSerializerSettings jsonSettings)
		{
			RTUProcessor.Enqueue(() =>
			{
				try
				{
					var args = JsonConvert.DeserializeObject<ReparentGameObjectChangeArgs>(commandHandlerArgs.Payload,
						jsonSettings);
					var go = GameObject.Find(args.GameObjectName);

					if (go == null)
					{
						throw new Exception("Unable to locate GameObject");
					}

					var parentGo = GameObject.Find(args.NewParentGameObjectName);
					
					go.transform.SetParent(parentGo?.transform);

					RTUDebug.Log($"Re-parent GameObject {go.name}");
				}
				catch (Exception e)
				{
					RTUDebug.Log($"Failed to re-parent GameObject : {e.Message}");
				}
			});
		}
	}
}