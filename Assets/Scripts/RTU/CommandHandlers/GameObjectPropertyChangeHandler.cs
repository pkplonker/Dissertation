using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Unity.VisualScripting;
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

					GameObject targetGO = GameObject.Find(args.ValuePath);
					member.SetValue(component, targetGO);
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