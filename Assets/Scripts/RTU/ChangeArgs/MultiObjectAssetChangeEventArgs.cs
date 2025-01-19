using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Object = UnityEngine.Object;

namespace RealTimeUpdateRuntime
{
	[Serializable]
	public class MultiObjectAssetChangeEventArgs : IPayload
	{
		public static string MESSAGE_IDENTIFER = "MultiObjectAssetUpdate";

		public List<MultiObjectAssetChange> ImpactedAssets { get; set; }

		[JsonIgnore]
		public int ID { get; set; }

		[JsonIgnore]
		public Type ValueType
		{
			get => string.IsNullOrEmpty(ValueTypeName) ? null : Type.GetType(ValueTypeName);
			set => ValueTypeName = value?.AssemblyQualifiedName;
		}
		public string ValueTypeName { get; set; }
		public virtual object Value { get; set; }

		public object GetDeserializedValue(JsonSerializerSettings settings)
		{
			if (Value == null || string.IsNullOrEmpty(ValueTypeName))
				return null;

			Type targetType = Type.GetType(ValueTypeName);

			if (targetType.IsPrimitive || targetType == typeof(string) || targetType == typeof(decimal))
			{
				try
				{
					return Convert.ChangeType(Value, targetType);
				}
				catch { }
			}

			return JsonConvert.DeserializeObject(Value.ToString(), targetType, settings);
		}

		public virtual List<string> GeneratePayload(JsonSerializerSettings JSONSettings) => new()
			{$"{MESSAGE_IDENTIFER}\n{JsonConvert.SerializeObject(this, Formatting.Indented, JSONSettings)}"};
	}

	public class MultiObjectAssetChange
	{
		public string PropName { get; set; }
		public string ComponentName { get; set; }
		public string GameObject { get; set; }
	}
}