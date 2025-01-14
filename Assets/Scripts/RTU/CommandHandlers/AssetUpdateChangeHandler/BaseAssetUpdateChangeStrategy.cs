using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RealTimeUpdateRuntime
{
	public abstract class BaseAssetUpdateChangeStrategy : IAssetUpdateChangeStrategy
	{
		public static string INSTANCE_STRING = "(Instance)";

		public virtual string EXTENSION { get; }

		private string RemoveInstanceString(string name, string instanceString)
		{
			while (true)
			{
				var index = name.IndexOf(instanceString, StringComparison.OrdinalIgnoreCase);
				if (index == -1)
					break;

				name = name.Remove(index, instanceString.Length).TrimEnd();
			}

			return name;
		}

		public void Update(JsonSerializerSettings jsonSettings, AssetPropertyChangeEventArgs args)
		{
			var elements = GetElements();
			var materialName = Path.GetFileNameWithoutExtension(args.Path);
			var matchingMats = elements.Where(x =>
			{
				var name = RemoveInstanceString(x.name, IAssetUpdateChangeStrategy.INSTANCE_STRING);

				return name.TrimEnd(' ').Equals(materialName, StringComparison.InvariantCultureIgnoreCase);
			});

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
								value = JsonConvert.DeserializeObject(value.ToString(),
									member.MemberType, jsonSettings);
							}

							if (!member.MemberType.IsInstanceOfType(value))
							{
								value = Convert.ChangeType(value, member.MemberType);
							}

							member?.SetValue(mat, value);
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
			else
			{
				RTUDebug.LogWarning($"No matches found for {materialName} - no change is being made");
			}
		}

		protected abstract List<UnityEngine.Object> GetElements();
	}
}