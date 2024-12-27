using System;
using Newtonsoft.Json;

namespace RealTimeUpdateRuntime
{
	[Serializable]
	public class GameObjectPropertyChangeArgs : IPropertyChangeArgs
	{
		public string GameObjectPath { get; set; } = string.Empty;
		public string ComponentTypeName { get; set; } = string.Empty;
		public string PropertyPath { get; set; } = string.Empty;

		public string ValuePath { get; set; }

		public object GetDeserializedValue(JsonSerializerSettings settings)
		{
			return null;
		}

		public string GeneratePayload(JsonSerializerSettings JSONSettings) =>
			$"GameObjectproperty,\n{JsonConvert.SerializeObject(this, Formatting.Indented, JSONSettings)}";
	}
}