using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using Object = UnityEngine.Object;

namespace RealTimeUpdateRuntime
{
	[CustomPropertyChangeArgs(typeof(Material))]
	[Serializable]
	public class MaterialPropertyChangeArgs : PropertyChangeArgs
	{
		public override List<string> GeneratePayload(JsonSerializerSettings JSONSettings)
		{
			var x = base.GeneratePayload(JSONSettings);

			return x;
		}

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