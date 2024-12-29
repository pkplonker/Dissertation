using Newtonsoft.Json;

namespace RealTimeUpdateRuntime
{
	public interface IChangeArgs
	{
		public string GeneratePayload(JsonSerializerSettings JSONSettings);
	}
}