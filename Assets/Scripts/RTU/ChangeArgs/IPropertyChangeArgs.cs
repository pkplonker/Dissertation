using System;
using Newtonsoft.Json;

namespace RealTimeUpdateRuntime
{
	public interface IPropertyChangeArgs : IChangeArgs
	{
		public string GameObjectPath { get; set; }
		public string ComponentTypeName { get; set; }
		public string PropertyPath { get; set; }
		public object Value { get; set; }

		[JsonIgnore]
		public Type ValueType { get; set; }
	}
}