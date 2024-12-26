using System;
using Newtonsoft.Json;

namespace RealTimeUpdateRuntime
{
	[Serializable]
	public class PropertyChangeArgs : IPropertyChangeArgs
	{
		private IPropertyChangeArgs propertyChangeArgsImplementation;
		public string GameObjectPath { get; set; } = string.Empty;
		public string ComponentTypeName { get; set; } = string.Empty;
		public string PropertyPath { get; set; } = string.Empty;
		public string GeneratePayload(JsonSerializerSettings JSONSettings) =>
			$"property,\n{JsonConvert.SerializeObject(this, Formatting.Indented, JSONSettings)}";

		public string ValueTypeName { get; set; }
		public object Value { get; set; }

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