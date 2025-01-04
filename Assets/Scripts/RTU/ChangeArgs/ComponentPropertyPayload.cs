using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Object = UnityEngine.Object;

namespace RealTimeUpdateRuntime
{
	[CustomPropertyChangeArgs(typeof(object))] //default
	[Serializable]
	public class ComponentPropertyPayload : IPropertyPayload
	{
		public static string MESSAGE_IDENTIFER = "Property";
		public string GameObjectPath { get; set; } = string.Empty;
		public string ComponentTypeName { get; set; } = string.Empty;
		public string PropertyPath { get; set; } = string.Empty;

		public virtual List<string> GeneratePayload(JsonSerializerSettings JSONSettings) => new List<string>()
			{$"{MESSAGE_IDENTIFER}\n{JsonConvert.SerializeObject(this, Formatting.Indented, JSONSettings)}"};

		public string ValueTypeName { get; set; }
		public virtual object Value { get; set; }

		[JsonIgnore]
		public Type ValueType
		{
			get => string.IsNullOrEmpty(ValueTypeName) ? null : Type.GetType(ValueTypeName);
			set => ValueTypeName = value?.AssemblyQualifiedName;
		}

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
	}
}