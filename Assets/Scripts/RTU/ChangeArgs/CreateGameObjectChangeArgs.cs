using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace RealTimeUpdateRuntime
{
	[Serializable]
	public class CreateGameObjectChangeArgs : IChangeArgs
	{
		public static string MESSAGE_IDENTIFER = "CreateGameObject";

		public string GameObjectPath { get; set; } = string.Empty;

		public List<string> GeneratePayload(JsonSerializerSettings JSONSettings) => new()
		{
			$"{MESSAGE_IDENTIFER}\n{JsonConvert.SerializeObject(this, Formatting.Indented, JSONSettings)}"
		};
	}
}