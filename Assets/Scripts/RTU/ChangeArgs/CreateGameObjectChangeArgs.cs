using System;
using Newtonsoft.Json;

namespace RealTimeUpdateRuntime
{
	[Serializable]
	public class CreateGameObjectChangeArgs : IChangeArgs
	{
		public static string MESSAGE_IDENTIFER = "CreateGameObject";

		public string GameObjectPath { get; set; } = string.Empty;

		public string GeneratePayload(JsonSerializerSettings JSONSettings) =>
			$"{MESSAGE_IDENTIFER}\n{JsonConvert.SerializeObject(this, Formatting.Indented, JSONSettings)}";
		
	}
}