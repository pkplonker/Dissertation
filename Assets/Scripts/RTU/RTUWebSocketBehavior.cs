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
		
		//todo ideally this should register, but given that the websocketsharp uses template argument,
		//we would need to use reflection to get all behaviours, get a path from the object, then use
		//reflection to create a runtime method of the templated func and use that to instantiate. https://stackoverflow.com/questions/2604743/setting-generic-type-at-runtime
		
		private readonly Dictionary<string, IRTUCommandHandler> commandHandlers = new()
		{
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
			Debug.Log($"Disconnected from editor {args.Reason}");
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

	public interface IRTUMessageHandler
	{
		string Path { get; }
	}
}