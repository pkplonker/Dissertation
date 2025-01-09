using System;
using Newtonsoft.Json;

namespace RealTimeUpdateRuntime
{
	public interface IPropertyPayload : IPayload
	{
		public string GameObjectPath { get; set; }
		public string ComponentTypeName { get; set; }
		public string MemberName { get; set; }
		public object Value { get; set; }

		[JsonIgnore]
		public Type ValueType { get; set; }
	}
}