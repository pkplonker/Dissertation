using System;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

namespace RealTimeUpdateRuntime
{
	public class ComponentCollectionPropertyChangeHandler : CollectionPropertyChangeHandler
	{
		public override void Process(CommandHandlerArgs commandHandlerArgs, JsonSerializerSettings jsonSettings)
		{
			RTUProcessor.Enqueue(() =>
			{
				try
				{
					var args = ProcessInternal<ComponentCollectionPropertyChangeArgs>(commandHandlerArgs,
						out var component,
						out var fieldName, out var member) as ComponentCollectionPropertyChangeArgs;
					var collection =
						CreateInstanceFromMemberInfo(member, args.ValuePaths.Select(x =>
						{
							try
							{
								var go = GameObject.Find(x.TargetGOPath);
								return go.GetComponent(x.TargetComponentType);
							}
							catch
							{
								return null;
							}
						}));

					member.SetValue(component, collection);
					RTUDebug.Log($"{fieldName} set to new collection of gameobjects successfully.");
				}
				catch (Exception e)
				{
					RTUDebug.Log($"Failed to set component colletion property: {e.Message}");
				}
			});
		}
	}
}