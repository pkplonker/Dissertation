using System;
using System.Collections.Generic;
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
		private static readonly Queue<Action> ExecutionQueue = new Queue<Action>();
		public TaskScheduler Schedular { get; private set; }
		public StringScriptableObject IP;
		public IntScriptableObject Port;
		private void Awake()
		{
			gameObject.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector;
		}

		void Start()
		{
			cancellationTokenSource = new CancellationTokenSource();
			serverThread = new Thread(() =>
			{
				try
				{
					StartServer(cancellationTokenSource.Token);
				}
				catch
				{
					webSocketServer = null;
					serverThread = null;
				}
			});
			serverThread.Start();
			Schedular = TaskScheduler.FromCurrentSynchronizationContext();
		}

		public void StartServer(CancellationToken token)
		{
			try
			{
				RTUDebug.Log("Starting Server");
				string localIP;
				// https://stackoverflow.com/questions/6803073/get-local-ip-address
				if (System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
				{
					using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
					{
						socket.Connect("8.8.8.8", 65530);
						IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
						localIP = endPoint.Address.ToString();
						RTUDebug.Log(localIP);
					}
				}
				else
				{
					RTUDebug.LogWarning("Not connected to network - unable to support RTU");
					return;
				}

				webSocketServer = new WebSocketServer(Port.Value);
				webSocketServer.AddWebSocketService<RTUWebSocketBehavior>("/RTU");
				webSocketServer.Start();
			}
			catch (Exception e)
			{
				RTUDebug.Log($"err {e}");
				return;
			}

			RTUDebug.Log("Server started. Waiting for connections...");

			while (true)
			{
				token.ThrowIfCancellationRequested();
			}
		}

		public static void Enqueue(Action action)
		{
			lock (ExecutionQueue)
			{
				ExecutionQueue.Enqueue(action);
			}
		}

		void Update()
		{
			lock (ExecutionQueue)
			{
				while (ExecutionQueue.Count > 0)
				{
					try
					{
						ExecutionQueue.Dequeue().Invoke();
					}
					catch (Exception e)
					{
						RTUDebug.LogError($"Failed to execute main thread dispatcher action {e.Message}");
					}
				}
			}
		}

		void OnApplicationQuit()
		{
			cancellationTokenSource?.Cancel();
			cancellationTokenSource?.Dispose();
			webSocketServer?.Stop();
			RTUDebug.Log("WebSocket server shutting down.");
		}
	}
}