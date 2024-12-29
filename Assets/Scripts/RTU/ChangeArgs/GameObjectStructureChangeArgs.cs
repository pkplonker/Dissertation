using System;
using Newtonsoft.Json;

namespace RealTimeUpdateRuntime
{
	[Serializable]
	public class GameObjectStrutureChangeArgs : IChangeArgs
	{
		public string GameObjectPath { get; set; } = string.Empty;
		public string ComponentTypeName { get; set; } = string.Empty;

		public string GeneratePayload(JsonSerializerSettings JSONSettings) =>
			$"property,\n{JsonConvert.SerializeObject(this, Formatting.Indented, JSONSettings)}";

		public bool IsAdd { get; set; }
	}
}