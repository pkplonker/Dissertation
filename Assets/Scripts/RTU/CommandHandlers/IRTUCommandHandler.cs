using Newtonsoft.Json;

namespace RealTimeUpdateRuntime
{
	public interface IRTUCommandHandler
	{
		void Process(CommandHandlerArgs commandHandlerArgs, JsonSerializerSettings jsonSettings);
		string Tag { get; }
	}

	public abstract class RTUCommandHandlerBase : IRTUCommandHandler
	{
		public abstract void Process(CommandHandlerArgs commandHandlerArgs, JsonSerializerSettings jsonSettings);
		public abstract string Tag { get; }
	}

	public class CommandHandlerArgs
	{
		public string Payload { get; set; }
	}
}