using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace RealTimeUpdateRuntime
{
	public class AssetUpdateChangeHandler : RTUCommandHandlerBase
	{
		public override string Tag { get; } = AssetPropertyChangeEventArgs.MESSAGE_IDENTIFER;

		public override void Process(CommandHandlerArgs commandHandlerArgs, JsonSerializerSettings jsonSettings)
		{
			RTUProcessor.Enqueue(() =>
			{
				try
				{
					var args = JsonConvert.DeserializeObject<AssetPropertyChangeEventArgs>(commandHandlerArgs.Payload,
						jsonSettings);
					switch (args.Type.ToLower())
					{
						case "mat":
							var mats = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None)
								.Select(x => x.TryGetComponent<Renderer>(out var comp) ? comp : null).WhereNotNull()
								.SelectMany(x => x.materials).ToList();
							var instanceString = "(Instance)";
							var materialName = Path.GetFileNameWithoutExtension(args.Path);
							var ex = mats.Select(x =>
								x.name.Remove(x.name.IndexOf(instanceString, StringComparison.OrdinalIgnoreCase),
									instanceString.Length).TrimEnd(' '));
							var matchingMats = mats.Where(x =>
								x.name.Remove(x.name.IndexOf(instanceString, StringComparison.OrdinalIgnoreCase),
									instanceString.Length).TrimEnd(' ').Equals(materialName));
							if (matchingMats.Any())
							{
								foreach (var mat in matchingMats)
								{
									var members = MemberAdaptorUtils.GetMemberAdapters(mat.GetType());
									foreach (var change in args.Changes)
									{
										var member = members.FirstOrDefault(x =>
											x.Name.Equals(change.Key, StringComparison.InvariantCultureIgnoreCase));
										try
										{
											var value = change.Value;
											if (change.Value is JArray a)
											{
												value = a.ToObject(member?.MemberType);
											}

											if (value is JObject)
											{
												value = JsonConvert.DeserializeObject(value.ToString(), member.MemberType, jsonSettings);
											}

											member?.SetValue(mat, value);
										}
										catch (Exception e)
										{
											RTUDebug.LogWarning(
												$"Failed setting {change.Key} on {args.Path}: {e.Message}");
										}
									}
								}
							}
							else
							{
								RTUDebug.LogWarning($"No matches found for {materialName} - no change is being made");
							}

							break;
						default:
							RTUDebug.LogWarning($"Asset Type not yet supported {args.Type.ToLower()}");
							break;
					}
				}
				catch (Exception e)
				{
					RTUDebug.Log($"Failed to set property: {e.Message}");
				}
			});
		}
	}
}