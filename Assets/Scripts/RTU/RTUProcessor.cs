using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using WebSocketSharp.Server;

namespace RealTimeUpdateRuntime
{
	public class RTUProcessor : MonoBehaviour
	{
		private CancellationTokenSource cancellationTokenSource;
		private Thread serverThread;

		private WebSocketServer webSocketServer;

		void Start()
		{
			cancellationTokenSource = new CancellationTokenSource();
			serverThread = new Thread(() => StartServer(cancellationTokenSource.Token));
			serverThread.Start();
			Debug.Log("WebSocket server thread started.");
		}

		public async Task StartServer(CancellationToken token)
		{
			try
			{
				int port = 6666;
				webSocketServer = new WebSocketServer(port);
				webSocketServer.AddWebSocketService<RTUWebSocketBehavior>("/RTU");
				webSocketServer.Start();
			}
			catch (Exception e)
			{
				Debug.Log($"err {e}");
				return;
			}

			Debug.Log("Server started. Waiting for connections...");

			while (true)
			{
				token.ThrowIfCancellationRequested();
			}
		}

		void OnApplicationQuit()
		{
			cancellationTokenSource?.Cancel();
			cancellationTokenSource?.Dispose();
			webSocketServer?.Stop();
			Debug.Log("WebSocket server shutting down.");
		}
	}
}