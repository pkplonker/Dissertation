using System.Collections.Generic;
using Newtonsoft.Json;

namespace RealTimeUpdateRuntime
{
	public interface IChangeArgs
	{
		public List<string>  GeneratePayload(JsonSerializerSettings JSONSettings);
	}
}