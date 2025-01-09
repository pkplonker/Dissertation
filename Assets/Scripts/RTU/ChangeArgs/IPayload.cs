using System.Collections.Generic;
using Newtonsoft.Json;

namespace RealTimeUpdateRuntime
{
	public interface IPayload
	{
		public List<string>  GeneratePayload(JsonSerializerSettings JSONSettings);
	}
}