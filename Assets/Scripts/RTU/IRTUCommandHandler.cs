
	using System.Collections.Generic;
	using System.Net.Sockets;

	public interface IRTUCommandHandler
	{
		void Process(NetworkStream stream, string payload);
	}
