using System.Collections.Generic;
using Newtonsoft.Json;

namespace RealTimeUpdateRuntime
{
	public class GameObjectCollectionPropertyChangeArgs : IPropertyChangeArgs
	{
		public string GameObjectPath { get; set; }
		public string ComponentTypeName { get; set; }
		public string PropertyPath { get; set; }
		public List<string> ValuePaths { get; set; }

		public string GeneratePayload(JsonSerializerSettings JSONSettings) =>
			$"GameObjectCollectionProperty,\n{JsonConvert.SerializeObject(this, Formatting.Indented, JSONSettings)}";	}
}