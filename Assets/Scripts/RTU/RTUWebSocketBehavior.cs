using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace RealTimeUpdateRuntime
{
	public class RTUWebSocketBehavior : WebSocketBehavior
	{
		private readonly Dictionary<string, IRTUCommandHandler> commandHandlers = new()
		{
			{"ping", new PingHandler()},
			{"property", new PropertyChangeHandler()},
		};

		protected override void OnMessage(MessageEventArgs args)
		{
			try
			{
				var data = Encoding.UTF8.GetString(args.RawData);
				Debug.Log($"Message received raw {data}");
				try
				{
					HandleMessage(data);
				}
				catch (Exception e)
				{
					Debug.LogWarning($"Unable to handle message {e.Message}");
				}
			}
			catch (Exception e)
			{
				Debug.LogWarning($"Unable to parse message {e.Message}");
			}
		}

		protected override void OnClose(CloseEventArgs args)
		{
			Debug.Log($"Connection lost {args.Reason}");
		}

		protected override void OnOpen()
		{
			Debug.Log("Connection established");
		}

		private void HandleMessage(string rawMessage)
		{
			var messages = rawMessage.Split("@").Where(x => !string.IsNullOrEmpty(x));
			foreach (var message in messages)
			{
				try
				{
					var index = message.IndexOf(',');
					string messageType = null;
					string payload = string.Empty;

					if (index != -1)
					{
						messageType = message.Substring(0, index);
						payload = message.Substring(index + 1);
					}

					if (messageType != null && commandHandlers.TryGetValue(messageType, out var handler))
					{
						handler.Process(null, payload);
					}
					else
					{
						Debug.Log($"Unknown command: {messageType ?? "null"}");
					}
				}
				catch (Exception e)
				{
					Debug.LogError($"Failed to parse message: {message} {e.Message}");
				}
			}
		}
	}
}