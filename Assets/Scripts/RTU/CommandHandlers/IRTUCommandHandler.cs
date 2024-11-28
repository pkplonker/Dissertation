using System;
using System.Threading;
using System.Threading.Tasks;

namespace RealTimeUpdateRuntime
{
	public interface IRTUCommandHandler
	{
		void Process(CommandHandlerArgs commandHandlerArgs);
	}

	public abstract class RTUCommandHandlerBase : IRTUCommandHandler
	{
		public abstract void Process(CommandHandlerArgs commandHandlerArgs);
		
	}

	public struct CommandHandlerArgs
	{
		public string Payload { get; set; }
	}
}