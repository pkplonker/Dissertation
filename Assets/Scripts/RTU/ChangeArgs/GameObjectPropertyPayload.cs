﻿using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace RealTimeUpdateRuntime
{
	[Serializable]
	public class GameObjectPropertyPayload : IPayload
	{
		public static string MESSAGE_IDENTIFER = "GameObjectProperty";
		public string GameObjectPath { get; set; } = string.Empty;
		public string MemberName { get; set; } = string.Empty;

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

		public int InstanceID { get; set; }

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