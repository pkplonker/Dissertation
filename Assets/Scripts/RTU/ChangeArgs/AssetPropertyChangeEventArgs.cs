using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace RealTimeUpdateRuntime
{
	[Serializable]
	public class AssetPropertyChangeEventArgs: IChangeArgs
	{
		public static string MESSAGE_IDENTIFER = "AssetProperty";
		public Dictionary<string, object> Changes { get; set; }
		public string Path { get; set; }
		public string Type { get; set; }
		public string GeneratePayload(JsonSerializerSettings JSONSettings) => throw new NotImplementedException();
	}
}