using System;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace RealTimeUpdateRuntime
{
	public interface IRTUCommandHandler
	{
		void Process(CommandHandlerArgs commandHandlerArgs, JsonSerializerSettings jsonSettings);
	}

	public abstract class RTUCommandHandlerBase : IRTUCommandHandler
	{
		public abstract void Process(CommandHandlerArgs commandHandlerArgs, JsonSerializerSettings jsonSettings);
		
	}

	public struct CommandHandlerArgs
	{
		public string Payload { get; set; }
	}
}