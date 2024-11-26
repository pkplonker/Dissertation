using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using RTU;
namespace Editor
{
	public static class RTUProcessor
	{
		private static TcpClient client;
		private static NetworkStream stream;
		public static bool IsConnected => client != null;
		private static Thread pingThread;

		static RTUProcessor()
		{
			Undo.postprocessModifications += PostprocessModificationsCallback;
			Undo.undoRedoPerformed += OnUndoRedoPerformed;
		}

		private static void OnUndoRedoPerformed()
		{
			// Need to do something about undoing a change as it's not reflected in the modifications callback 
		}

		static UndoPropertyModification[] PostprocessModificationsCallback(UndoPropertyModification[] modifications)
		{
			if (client != null)
			{
				foreach (var modification in modifications)
				{
					if (modification.currentValue is PropertyModification pm)
					{
						ProcessPropertyModification(pm);
					}
				}
			}

			return modifications;
		}

		private static void ProcessPropertyModification(PropertyModification pm)
		{
			if (pm.target is Component component)
			{
				var go = component.gameObject;
				var path = GetGameObjectPath(go);
				var args = new PropertyChangeArgs()
				{
					GameObjectPath = path,
					ComponentTypeName = component.GetType().AssemblyQualifiedName,
					PropertyPath = pm.propertyPath,
					Value = pm.value,
					ValueType = pm.value.GetType()
				};
				SendMessageToGame($"property,\n{JsonConvert.SerializeObject(args)}");
			}
		}
		private static string GetGameObjectPath(GameObject obj)
		{
			string path = "/" + obj.name;
			while (obj.transform.parent != null)
			{
				obj = obj.transform.parent.gameObject;
				path = "/" + obj.name + path;
			}
			return path;
		}

		public static void Connect()
		{
			Disconnect();
			client = new TcpClient("127.0.0.1", 5000);
			stream = client.GetStream();
			pingThread = new Thread(SendPing);
			pingThread.IsBackground = true;
			pingThread.Start();
			Debug.Log("Connected to game server.");
			RTUScene.Show();
		}

		private static void SendPing()
		{
			while (IsConnected)
			{
				try
				{
					byte[] pingMessage = Encoding.UTF8.GetBytes("ping,");
					stream.Write(pingMessage, 0, pingMessage.Length);
					Debug.Log("Sent ping to game.");

					byte[] buffer = new byte[256];
					int bytesRead = stream.Read(buffer, 0, buffer.Length);
					string response = Encoding.UTF8.GetString(buffer, 0, bytesRead);

					if (response == "pong")
					{
						Debug.Log("Received pong from game.");
					}
					else
					{
						Debug.LogWarning("Unexpected response: " + response);
					}

					Thread.Sleep(1000);
				}
				catch (System.Exception ex)
				{
					Debug.LogError("Error during ping: " + ex.Message);
					Disconnect();
				}
			}
		}

		public static void Disconnect()
		{
			if (IsConnected)
			{
				RTUScene.Close();
			}
			if (client != null)
			{
				client.Close();
				client = null;
			}

			if (stream != null)
			{
				stream.Close();
				stream = null;
			}
		}

		public static void SendMessageToGame(string message)
		{
			if (client == null || !client.Connected)
			{
				Debug.LogError("Not connected to the game server.");
				return;
			}

			byte[] data = Encoding.ASCII.GetBytes(message);
			stream.Write(data, 0, data.Length);
			stream.Flush();
			Debug.Log("Message sent to game: " + message);
		}
	}
}