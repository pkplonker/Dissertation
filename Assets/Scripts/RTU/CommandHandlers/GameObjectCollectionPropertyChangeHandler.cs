using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using UnityEngine;

namespace RealTimeUpdateRuntime
{
	public class GameObjectCollectionPropertyChangeHandler : CollectionPropertyChangeHandler
	{
		public override void Process(CommandHandlerArgs commandHandlerArgs, JsonSerializerSettings jsonSettings)
		{
			RTUProcessor.Enqueue(() =>
			{
				try
				{
					var args = ProcessInternal<GameObjectCollectionPropertyChangeArgs>(commandHandlerArgs,
						out var component,
						out var fieldName, out var member) as GameObjectCollectionPropertyChangeArgs;
					var collection =
						CreateInstanceFromMemberInfo(member, args.ValuePaths.Select(x => GameObject.Find(x)));

					member.SetValue(component, collection);
					RTUDebug.Log($"{fieldName} set to new collection of gameobjects successfully.");
				}
				catch (Exception e)
				{
					RTUDebug.Log($"Failed to set gameobject collection property: {e.Message}");
				}
			});
		}
	}
}