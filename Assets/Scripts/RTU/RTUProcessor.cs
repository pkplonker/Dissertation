using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
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
		}

		public void StartServer(CancellationToken token)
		{
			try
			{
				Debug.Log("Starting Server");
				int port = 6666;
				string localIP;
				// https://stackoverflow.com/questions/6803073/get-local-ip-address
				if (System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
				{
					using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
					{
						socket.Connect("8.8.8.8", 65530);
						IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
						localIP = endPoint.Address.ToString();
						Debug.Log(localIP);
					}
				}
				else
				{
					Debug.LogWarning("Not connected to network - unable to support RTU");
					return;
				}

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