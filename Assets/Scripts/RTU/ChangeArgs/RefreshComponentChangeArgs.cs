using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace RealTimeUpdateRuntime
{
	[Serializable]
	public class RefreshComponentChangeArgs : IChangeArgs
	{
		public string GameObjectPath { get; set; } = string.Empty;
		public string ComponentTypeName { get; set; } = string.Empty;
		public Dictionary<string, object> Members { get; set; }

		public string GeneratePayload(JsonSerializerSettings JSONSettings) =>
			$"RefreshComponent\n{JsonConvert.SerializeObject(this, Formatting.Indented, JSONSettings)}";

		public Dictionary<string, object> GetDeserializedMembers(JsonSerializerSettings settings)
		{
			if (Members == null) return null;
			var adaptors = MemberAdaptorUtils.GetMemberAdaptersAsDict(Type.GetType(ComponentTypeName));
			var result = new Dictionary<string, object>();
			foreach (var (name, value) in Members)
			{
				if (adaptors.TryGetValue(name, out var adaptor))
				{
					var targetType = adaptor.MemberType;
					if (targetType.IsPrimitive || targetType == typeof(string) || targetType == typeof(decimal))
					{
						try
						{
							result.Add(name, Convert.ChangeType(value, targetType));
						}
						catch { }
					}
					else
					{
						result.Add(name, JsonConvert.DeserializeObject(value.ToString(), targetType, settings));
					}
				}
			}

			return result;
		}
	}
}