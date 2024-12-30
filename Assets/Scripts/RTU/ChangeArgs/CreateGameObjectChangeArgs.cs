using System;
using Newtonsoft.Json;

namespace RealTimeUpdateRuntime
{
	[Serializable]
	public class CreateGameObjectChangeArgs : IChangeArgs
	{
		public string GameObjectPath { get; set; } = string.Empty;
		
		public string GeneratePayload(JsonSerializerSettings JSONSettings) =>
			$"CreateGameObject\n{JsonConvert.SerializeObject(this, Formatting.Indented, JSONSettings)}";
		
	}
}