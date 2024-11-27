using System.Collections.Generic;
using System.Net.Sockets;

namespace RealTimeUpdateRuntime
{
	public interface IRTUCommandHandler
	{
		void Process(NetworkStream stream, string payload);
	}
}