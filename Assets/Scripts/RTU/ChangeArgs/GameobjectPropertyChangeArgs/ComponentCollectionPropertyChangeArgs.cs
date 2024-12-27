using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace RealTimeUpdateRuntime
{
	public class ComponentCollectionPropertyChangeArgs : IPropertyChangeArgs
	{
		public string GameObjectPath { get; set; }
		public string ComponentTypeName { get; set; }
		public string PropertyPath { get; set; }
		public List<ComponentCollectionElement> ValuePaths { get; set; }

		public string GeneratePayload(JsonSerializerSettings JSONSettings) =>
			$"ComponentCollectionProperty,\n{JsonConvert.SerializeObject(this, Formatting.Indented, JSONSettings)}";
	}

	public class ComponentCollectionElement
	{
		public Type TargetComponentType { get; set; }
		public string TargetGOPath { get; set; }
	}
}