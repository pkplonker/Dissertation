using System;
using Newtonsoft.Json;

namespace RealTimeUpdateRuntime
{
	[Serializable]
	public class PropertyChangeArgs
	{
		public string GameObjectPath = string.Empty;
		public string ComponentTypeName = string.Empty;
		public string PropertyPath = string.Empty;

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

		public PropertyChangeArgs Clone() =>
			new()
			{
				GameObjectPath = this.GameObjectPath,
				ComponentTypeName = this.ComponentTypeName,
				PropertyPath = this.PropertyPath,
				Value = this.Value,
				ValueType = this.ValueType,
			};
	}
}