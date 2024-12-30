using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace RealTimeUpdateRuntime
{
	[Serializable]
	public class DestroyGameObjectChangeArgs : IChangeArgs
	{
		public string ParentGameObjectPath { get; set; } = string.Empty;
		public List<string> CurrentChildren { get; set; }

		public string GeneratePayload(JsonSerializerSettings JSONSettings) =>
			$"DestroyGameObject\n{JsonConvert.SerializeObject(this, Formatting.Indented, JSONSettings)}";
		
	}
}