using System;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

namespace RealTimeUpdateRuntime
{
	public class CreateGameObjectChangeHandler : RTUCommandHandlerBase
	{
		public override string Tag { get; } = CreateGameObjectPayload.MESSAGE_IDENTIFER;

		public override void Process(CommandHandlerArgs commandHandlerArgs, JsonSerializerSettings jsonSettings)
		{
			RTUProcessor.Enqueue(() =>
			{
				try
				{
					var args = JsonConvert.DeserializeObject<CreateGameObjectPayload>(commandHandlerArgs.Payload,
						jsonSettings);
					var go = new GameObject();
					var pathElements =args.GameObjectPath.Split('/');
					go.name = pathElements.Last();
					if (pathElements.Count() > 1)
					{
						var parentPath = String.Join('/', pathElements);
						var parent = GameObject.Find(parentPath);
						if (parent == null)
						{
							throw new Exception("Unable to locate parent for object creation");
						}

						go.transform.parent = parent.transform;
					}
					RTUDebug.Log($"Added GameObject : {go.GetFullName()}");
				}
				catch (Exception e)
				{
					RTUDebug.LogWarning($"Failed to add GameObject : {e.Message}");
				}
			});
		}
	}
}