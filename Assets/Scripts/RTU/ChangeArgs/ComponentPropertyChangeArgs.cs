using System;
using Newtonsoft.Json;

namespace RealTimeUpdateRuntime
{
	[Serializable]
	public class ComponentPropertyChangeArgs : IPropertyChangeArgs
	{
		public string GameObjectPath { get; set; } = string.Empty;
		public string ComponentTypeName { get; set; } = string.Empty;
		public string PropertyPath { get; set; } = string.Empty;
		public string TargetGOPath;

		public Type TargetComponentTypeName { get; set; }

		public string GeneratePayload(JsonSerializerSettings JSONSettings) =>
			$"ComponentProperty,\n{JsonConvert.SerializeObject(this, Formatting.Indented, JSONSettings)}";
	}
}