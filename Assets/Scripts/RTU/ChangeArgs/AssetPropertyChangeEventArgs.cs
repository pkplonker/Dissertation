using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace RealTimeUpdateRuntime
{
	[Serializable]
	public class AssetPropertyChangeEventArgs : IPayload
	{
		public static string MESSAGE_IDENTIFER = "AssetUpdate";
		public Dictionary<string, object> Changes { get; set; }

		[JsonIgnore]
		public int ID { get; set; }

		[JsonIgnore]
		public Dictionary<string, object> OriginalValues { get; set; }

		public string Path { get; set; }
		public string Type { get; set; }

		public virtual List<string> GeneratePayload(JsonSerializerSettings JSONSettings) => new()
			{$"{MESSAGE_IDENTIFER}\n{JsonConvert.SerializeObject(this, Formatting.Indented, JSONSettings)}"};
	}
}