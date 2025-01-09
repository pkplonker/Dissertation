using RealTimeUpdateRuntime;

namespace RTUEditor
{
	public interface IMessageSender
	{
		public void SendPayloadToGame(IPayload payload);
		public bool IsConnected { get; }
	}
}