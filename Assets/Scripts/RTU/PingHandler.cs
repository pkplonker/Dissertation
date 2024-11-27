using System.Net.Sockets;
using System.Text;
using UnityEngine;

namespace RealTimeUpdateRuntime
{
	public class PingHandler : IRTUCommandHandler
	{
		public void Process(NetworkStream stream, string payload)
		{
			Debug.Log("Received ping from Editor.");
			byte[] ackMessage = Encoding.UTF8.GetBytes("pong");
			stream.Write(ackMessage, 0, ackMessage.Length);
		}
	}
}