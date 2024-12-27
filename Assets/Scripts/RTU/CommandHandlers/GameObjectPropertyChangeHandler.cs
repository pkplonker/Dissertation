using System;
using Newtonsoft.Json;
using UnityEngine;

namespace RealTimeUpdateRuntime
{
	public class GameObjectPropertyChangeHandler : PropertyRTUCommandHandlerBase
	{
		public override void Process(CommandHandlerArgs commandHandlerArgs, JsonSerializerSettings jsonSettings)
		{
			RTUProcessor.Enqueue(() =>
			{
				try
				{
					var args = ProcessInternal<GameObjectPropertyChangeArgs>(commandHandlerArgs, out var component,
						out var fieldName, out var member) as GameObjectPropertyChangeArgs;
					member.SetValue(component, GameObject.Find(args.ValuePath));
					RTUDebug.Log($"{fieldName} set to {args.ValuePath} successfully.");
				}
				catch (Exception e)
				{
					RTUDebug.Log($"Failed to set gameobject property: {e.Message}");
				}
			});
		}
	}
}