namespace RTUEditor
{
	public interface IMessageSender
	{
		public void SendMessageToGame(string message);
		public bool IsConnected { get; }
	}
}