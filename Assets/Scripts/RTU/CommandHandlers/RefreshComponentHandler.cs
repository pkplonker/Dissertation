using System;
using System.Collections;
using Newtonsoft.Json;
using UnityEngine;
using Object = UnityEngine.Object;

namespace RealTimeUpdateRuntime
{
	public class RefreshComponentHandler : RTUCommandHandlerBase
	{
		public override string Tag { get; } = RefreshComponentChangeArgs.MESSAGE_IDENTIFER;

		public override void Process(CommandHandlerArgs commandHandlerArgs, JsonSerializerSettings jsonSettings)
		{
			RTUProcessor.Enqueue(() =>
			{
				try
				{
					var args = JsonConvert.DeserializeObject<RefreshComponentChangeArgs>(commandHandlerArgs.Payload,
						jsonSettings);
					var go = GameObject.Find(args.GameObjectPath);
					var componentType = args.ComponentTypeName.GetTypeIncludingUnity();
					var adaptors = MemberAdaptorUtils.GetMemberAdaptersAsDict(componentType);
					if (go.TryGetComponent(componentType, out var component))
					{
						foreach ((var name, var value) in args.GetDeserializedMembers(jsonSettings))
						{
							if (adaptors.TryGetValue(name, out var adaptor))
							{
								try
								{

									adaptor.SetValue(component, value);
								}
								catch (ArgumentException e) { }
								catch (Exception e)
								{
									RTUDebug.LogWarning(
										$"Failed to update property to refresh structure {go.name} : {component.name} : {name}");
								}
							}
							else
							{
								RTUDebug.LogWarning(
									$"Failed to locate property to refresh structure {go.name} : {component.name} : {name}");
							}
						}
					}
					else
					{
						throw new Exception("Failed to get component to refresh structure");
					}

					RTUDebug.Log($"Refreshed component structure");
				}
				catch (Exception e)
				{
					RTUDebug.LogWarning($"Failed to refresh structure: {e.Message} : {e?.InnerException}");
				}
			});
		}
	}
}