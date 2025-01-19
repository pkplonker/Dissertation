using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Object = UnityEngine.Object;

namespace RealTimeUpdateRuntime
{
	public abstract class BaseAssetUpdateChangeStrategy : IAssetUpdateChangeStrategy
	{
		public static string INSTANCE_STRING = "(Instance)";

		public virtual string EXTENSION { get; }

		protected string RemoveInstanceString(string name, string instanceString)
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

		public virtual void Update(string payload, JsonSerializerSettings jsonSettings)
		{
			var args = JsonConvert.DeserializeObject<AssetPropertyChangeEventArgs>(payload,
				jsonSettings);
			var elements = GetElements();
			var assetName = Path.GetFileNameWithoutExtension(args.Path);
			var matchingAssets = elements.WhereNotNull().Where(x =>
			{
				var name = RemoveInstanceString(x.name, IAssetUpdateChangeStrategy.INSTANCE_STRING);

				return name.TrimEnd(' ').Equals(assetName, StringComparison.InvariantCultureIgnoreCase);
			});

			if (matchingAssets.Any())
			{
				UpdateAsset(jsonSettings, matchingAssets, args);
			}
			else
			{
				RTUDebug.LogWarning($"No matches found for {assetName} - no change is being made");
			}
		}

		protected virtual void UpdateAsset(JsonSerializerSettings jsonSettings, IEnumerable<Object> matchingAssets,
			AssetPropertyChangeEventArgs args)
		{
			foreach (var mat in matchingAssets)
			{
				var members = MemberAdaptorUtils.GetMemberAdapters(mat.GetType());
				foreach (var change in args.Changes)
				{
					var member = members.FirstOrDefault(x =>
						x.Name.Equals(change.Key, StringComparison.InvariantCultureIgnoreCase));
					if (IsMemberNull(member,change.Key)) continue;
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
							value = ValueConverter.ConvertValue(member.MemberType, value);
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

		public abstract void MultiUpdate(string payload, JsonSerializerSettings jsonSettings);

		protected virtual bool IsMemberNull(IMemberAdapter member, string changeKey)
		{
			if (member == null)
			{
				RTUDebug.LogWarning($"No matches found for {changeKey} - no change is being made for this member");
				return true;
			}
			return false;
		}

		protected abstract List<UnityEngine.Object> GetElements();
	}
}