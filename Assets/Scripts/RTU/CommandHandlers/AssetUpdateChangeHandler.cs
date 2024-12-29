using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace RealTimeUpdateRuntime
{
	public class AssetUpdateChangeHandler : RTUCommandHandlerBase
	{
		public override string Tag { get; } = "assetUpdate";

		public override void Process(CommandHandlerArgs commandHandlerArgs, JsonSerializerSettings jsonSettings)
		{
			RTUProcessor.Enqueue(() =>
			{
				try
				{
					var args = JsonConvert.DeserializeObject<AssetPropertyChangeEventArgs>(commandHandlerArgs.Payload, jsonSettings);
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
							if (matchingMats.Count() == 1)
							{
								var mat = matchingMats.First();
								var members = MemberAdaptorUtils.GetMemberAdapters(mat.GetType());
								foreach (var change in args.Changes)
								{
									var member = members.FirstOrDefault(x =>
										x.Name.Equals(change.Key, StringComparison.InvariantCultureIgnoreCase));
									try
									{
										var value = change.Value;
										if(change.Value is JArray a )
										{
											value = a.ToObject(member?.MemberType);
										}
										member?.SetValue(mat, value);
									}
									catch (Exception e)
									{
										RTUDebug.LogWarning($"Failed setting {change.Key} on {args.Path}: {e.Message}");
									}
								}
							}
							else if (!matchingMats.Any())
							{
								RTUDebug.LogWarning($"No matches found for {materialName} - no change is being made");
							}
							else
							{
								RTUDebug.LogWarning(
									$"Multiply possible matches found for {materialName} - no change is being made");
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


		private bool ModifyStruct(object structValue, string subFieldName, string newValue, out object newStruct)
		{
			Type structType = structValue.GetType();

			FieldInfo subFieldInfo = structType.GetField(subFieldName, BindingFlags.Public | BindingFlags.Instance);
			newStruct = structType;
			if (subFieldInfo == null) return false;
			object convertedValue = Convert.ChangeType(newValue, subFieldInfo.FieldType);
			newStruct = structValue;
			subFieldInfo.SetValue(newStruct, convertedValue);
			return true;
		}

		private static object ConvertValue(Type targetType, object value)
		{
			try
			{
				return Convert.ChangeType(value, targetType);
			}
			catch (InvalidCastException)
			{
				throw new InvalidCastException(
					$"Cannot convert value '{value}' of type {targetType} to {targetType}.");
			}
			catch (FormatException)
			{
				throw new FormatException($"Invalid format for value '{value}' when converting to {targetType}.");
			}
			catch (Exception ex)
			{
				throw new Exception($"Failed to convert value '{value}' to {targetType}: {ex.Message}");
			}
		}
	}
}