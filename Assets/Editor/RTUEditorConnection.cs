﻿using System;
using System.Text;
using System.Threading.Tasks;
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
				Debug.LogError($"Unable to create game connection {e.Message}");
				return;
			}

			socket.OnOpen += (_, args) =>
			{
				Debug.Log("Connected to the server");
				completeCallback?.Invoke();
			};
			socket.OnClose += (_, args) =>
			{
				if (args.WasClean)
				{
					Debug.Log("Closed connection to game");
				}
				else
				{
					Debug.LogWarning($"Unable to establish connection to game. Reason: {args.Reason}");
				}
				disconnectCallback?.Invoke(args.WasClean);
			};
			socket.OnMessage += (_, args) => Debug.Log($"Message received: {args.Data}");
			socket.OnError += (_, args) => Debug.Log($"Error connection to game: {args.Message}");

			socket.Connect();
		}

		public void SendMessageToGame(string message)
		{
			if (!IsConnected)
			{
				Debug.LogError("Not connected to the game server.");
				return;
			}
			//Debug.Log($"Sending Message: {message}");
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