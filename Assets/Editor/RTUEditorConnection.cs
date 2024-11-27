using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using WebSocketSharp;

namespace Editor
{
	public static class RTUEditorConnection
	{
		private static WebSocket socket;
		public static bool IsConnected => socket?.ReadyState == WebSocketState.Open;

		public static void Connect(string ipAddress, Action completeCallback = null)
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
				completeCallback?.Invoke();
				Debug.Log("Connected to the server");
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
			};
			socket.OnMessage += (_, args) => Debug.Log($"Message received: {args.Data}");
			socket.OnError += (_, args) => Debug.Log($"Error connection to game: {args.Message}");

			socket.Connect();
		}

		public static void SendMessageToGame(string message)
		{
			if (!IsConnected)
			{
				Debug.LogError("Not connected to the game server.");
				return;
			}

			Debug.Log($"Sending Message: {message}");
			socket.Send(Encoding.UTF8.GetBytes(message));
		}

		public static void Disconnect()
		{
			if (socket != null)
			{
				socket.Close();
			}
		}
	}
}