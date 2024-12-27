using Newtonsoft.Json;

namespace RealTimeUpdateRuntime
{
	public interface IPropertyChangeArgs
	{
		public string GameObjectPath { get; set; }
		public string ComponentTypeName { get; set; }
		public string PropertyPath { get; set; }
		public string GeneratePayload(JsonSerializerSettings JSONSettings);
	}
}