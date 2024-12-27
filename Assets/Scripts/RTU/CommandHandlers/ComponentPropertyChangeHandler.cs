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
	public class ComponentPropertyChangeHandler : PropertyRTUCommandHandlerBase
	{
		public override void Process(CommandHandlerArgs commandHandlerArgs, JsonSerializerSettings jsonSettings)
		{
			RTUProcessor.Enqueue(() =>
			{
				try
				{
					var args = ProcessInternal<ComponentPropertyChangeArgs>(commandHandlerArgs, out var component,
						out var fieldName, out var member) as ComponentPropertyChangeArgs;

					GameObject targetGO = GameObject.Find(args.TargetGOPath);
					if (targetGO != null)
					{
						var comp = targetGO.GetComponent(args.TargetComponentType);
						if (comp != null)
						{
							member.SetValue(component, comp);
							RTUDebug.Log($"{fieldName} set to {args.TargetComponentType} successfully.");
						}
						else
						{
							RTUDebug.LogWarning($"Cannot locate component {args.TargetComponentType} on {args.TargetGOPath}");
						}
					}
					else
					{
						RTUDebug.LogWarning($"Cannot locate gameobject for component property change {args.TargetGOPath}");
					}
				}
				catch (Exception e)
				{
					RTUDebug.Log($"Failed to set gameobject property: {e.Message}");
				}
			});
		}

		
	}
}