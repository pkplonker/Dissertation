using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace RealTimeUpdateRuntime
{
	[Serializable]
	public class ReparentGameObjectPayload : IPayload
	{
		public static string MESSAGE_IDENTIFER = "ReparentGameObject";
		public string GameObjectName { get; set; }
		public string NewParentGameObjectName { get; set; }

		public List<string> GeneratePayload(JsonSerializerSettings JSONSettings) => new()
		{
			$"{MESSAGE_IDENTIFER}\n{JsonConvert.SerializeObject(this, Formatting.Indented, JSONSettings)}"
		};
	}
}