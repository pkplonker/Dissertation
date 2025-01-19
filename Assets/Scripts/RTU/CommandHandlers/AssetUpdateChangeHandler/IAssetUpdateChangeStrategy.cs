using System;
using Newtonsoft.Json;

namespace RealTimeUpdateRuntime
{
	public interface IAssetUpdateChangeStrategy
	{
		public static string INSTANCE_STRING = "(Instance)";

		public string EXTENSION { get; }
		public void Update(string payload, JsonSerializerSettings jsonSettings);
		void MultiUpdate(string payload, JsonSerializerSettings jsonSettings);
	}
}