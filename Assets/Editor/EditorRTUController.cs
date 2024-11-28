using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Editor
{
	public class EditorRtuController
	{
		private RTUEditorConnection connection = new RTUEditorConnection();
		private List<IRTUEditorProcessor> handlers;
		private RTUScene scene = new RTUScene();
		public bool IsConnected => connection?.IsConnected ?? false;

		private void CloseScene() => scene.Close();

		public void ShowScene() => scene.ShowScene();

		public void SendMessageToGame(string message) => connection.SendMessageToGame(message);

		public void Disconnect()
		{
			CloseScene();
			connection.Disconnect();
		}

		public void Connect(string ip, Action connectCallback = null, Action disconnectCallback = null)
		{
			connection.Connect(ip, OnConnection(connectCallback), b => OnDisconnect(disconnectCallback, b));
		}

		private void OnDisconnect(Action disconnectCallback, bool b)
		{
			try
			{
				CloseScene();
			}
			catch (Exception e)
			{
				Debug.LogError($"Failed to close scene on disconnect: {e.Message}");
			}

			try
			{
				disconnectCallback?.Invoke();
			}
			catch (Exception e)
			{
				Debug.LogError($"Failed to execute disconnection callback: {e.Message}");
			}
		}

		private Action OnConnection(Action connectCallback)
		{
			return () =>
			{
				try
				{
					CreateHandlers();
					ShowScene();
				}
				catch (Exception e)
				{
					Debug.LogError($"Failed to setup editor for connection: {e.Message}");
					connection.Disconnect();
				}

				try
				{
					connectCallback?.Invoke();
				}
				catch (Exception e)
				{
					Debug.LogError($"Failed to execute connection callback: {e.Message}");
					connection.Disconnect();
				}
			};
		}

		private void CreateHandlers()
		{
			handlers = AppDomain.CurrentDomain.GetAssemblies()
				.SelectMany(x => x.GetTypes())
				.Where(x => x.GetInterfaces().Contains(typeof(IRTUEditorProcessor)))
				.ForEach(x => Debug.Log($"Registering Editor Handlers: {x}"))
				.Select(x =>
					(IRTUEditorProcessor) Activator.CreateInstance(x, new object[] {this}))
				.ToList();
		}
	}
}