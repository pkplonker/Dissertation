using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace RealTimeUpdateRuntime
{
	[CustomPropertyChangeArgs(typeof(Object))]
	[Serializable]
	public class UnityObjectComponentPropertyChangeArgs : ComponentPropertyChangeArgs
	{
		public override List<string> GeneratePayload(JsonSerializerSettings JSONSettings) => new List<string>()
			{$"{MESSAGE_IDENTIFER}\n{JsonConvert.SerializeObject(this, Formatting.Indented, JSONSettings)}"};

		[JsonIgnore]
		public override object Value { get; set; }

		public Object UnityObjetValue => Value as Object;
	}
}