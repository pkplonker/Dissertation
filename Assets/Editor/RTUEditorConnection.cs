﻿using System;
using System.Text;
using System.Threading.Tasks;
using RealTimeUpdateRuntime;
using UnityEngine;
using WebSocketSharp;

namespace RTUEditor
{
	public class RTUEditorConnection
	{
		private static WebSocket socket;
		private readonly TaskScheduler scheduler;

		public RTUEditorConnection(TaskScheduler scheduler)
		{
			this.scheduler = scheduler;
		}

		public bool IsConnected => socket?.ReadyState == WebSocketState.Open;

		public void Connect(string ipAddress, Action completeCallback = null, Action<bool> disconnectCallback = null)
		{
			int port = 6666;
			string behaviour = "RTU";
			try
			{
				socket = new WebSocket($"ws://{ipAddress}:{port}/{behaviour}");
			}
			catch (Exception e)
			{
				RTUDebug.LogError($"Unable to create game connection {e.Message}");
				return;
			}

			socket.OnOpen += (_, args) =>
			{
				RTUDebug.Log("Connected to the server");
				completeCallback?.Invoke();
			};
			socket.OnClose += (_, args) =>
			{
				if (args.WasClean)
				{
					RTUDebug.Log("Closed connection to game");
				}
				else
				{
					RTUDebug.LogWarning($"Unable to establish connection to game. Reason: {args.Reason}");
				}
				disconnectCallback?.Invoke(args.WasClean);
			};
			socket.OnMessage += (_, args) => RTUDebug.Log($"Message received: {args.Data}");
			socket.OnError += (_, args) => RTUDebug.Log($"Error connection to game: {args.Message}");

			socket.Connect();
		}

		public void SendMessageToGame(string message)
		{
			if (!IsConnected)
			{
				RTUDebug.LogError("Not connected to the game server.");
				return;
			}
			//RTUDebug.Log($"Sending Message: {message}");
			socket.Send(Encoding.UTF8.GetBytes(message));
		}

		public void Disconnect()
		{
			if (socket != null)
			{
				socket.Close();
			}
		}
	}
}