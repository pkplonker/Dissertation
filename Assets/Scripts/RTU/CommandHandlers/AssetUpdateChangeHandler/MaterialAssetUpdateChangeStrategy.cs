using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace RealTimeUpdateRuntime
{
	public class MaterialAssetUpdateChangeStrategy : BaseAssetUpdateChangeStrategy
	{
		public override string EXTENSION { get; } = "mat";

		public override void Update(string payload, JsonSerializerSettings jsonSettings)
		{
			base.Update(payload, jsonSettings);
			var args = JsonConvert.DeserializeObject<MaterialAssetPropertyChangeEventArgs>(payload,
				jsonSettings);

			var elements = GetElements();
			var assetName = Path.GetFileNameWithoutExtension(args.Path);
			var matchingAssets = elements.WhereNotNull().Where(x =>
			{
				var name = RemoveInstanceString(x.name, IAssetUpdateChangeStrategy.INSTANCE_STRING);

				return name.TrimEnd(' ').Equals(assetName, StringComparison.InvariantCultureIgnoreCase);
			}).OfType<Material>();

			if (matchingAssets.Any())
			{
				foreach (var mat in matchingAssets)
				{
					foreach (var propDict in args.ShaderProperties)
					{
						foreach (var change in propDict.Value)
						{
							try
							{
								var value = change.Value;
								// if (change.Value is JArray a)
								// {
								// 	value = a.ToObject(member?.MemberType);
								// }
								//
								// if (value is JObject)
								// {
								// 	value = JsonConvert.DeserializeObject(value.ToString(),
								// 		member.MemberType, jsonSettings);
								// }
								//
								// if (!member.MemberType.IsInstanceOfType(value))
								// {
								// 	value = ValueConverter.ConvertValue(member.MemberType, value);
								// }
								//
								// member?.SetValue(mat, value);
								RTUDebug.Log($"Updated asset {change.Key} on {args.Path}");
							}
							catch (Exception e)
							{
								RTUDebug.LogWarning(
									$"Failed setting {change.Key} on {args.Path}: {e.Message}");
							}
						}
						
					}
				}
			}
		}

		protected override List<UnityEngine.Object> GetElements()
		{
			var elements = UnityEngine.Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None)
				.Select(x => x.TryGetComponent<Renderer>(out var comp) ? comp : null).WhereNotNull()
				.SelectMany(x => x.materials).Cast<UnityEngine.Object>().ToList();
			return elements;
		}
	}
}