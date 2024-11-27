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

		public static void Connect()
		{
			string ipAddress = "127.0.0.1";
			int port = 6666;
			string behaviour = "RTU";
			socket = new WebSocket($"ws://{ipAddress}:{port}/{behaviour}");
			socket.OnOpen += (_, args) => Debug.Log("Connected to the server");
			socket.OnClose += (_, args) => Debug.Log("Closed connection to server");
			socket.OnMessage += (_, args) => Debug.Log($"Message received: {args.Data}");
			socket.OnError += (_, args) => Debug.Log($"Error connection to server: {args.Message}");

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