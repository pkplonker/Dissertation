﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RealTimeUpdateRuntime;
using RTUEditor.AssetStore;
using UnityEngine;

namespace RTUEditor
{
	public class EditorRtuController : IEditorRtuController
	{
		private readonly RTUEditorConnection connection;
		private readonly List<IRTUEditorProcessor> handlers;
		public event Action<RTUScene> SceneChanged;
		private RTUScene scene;

		public RTUScene Scene
		{
			get => scene;
			private set
			{
				scene = value;
				SceneChanged?.Invoke(scene);
			}
		}

		private readonly TaskScheduler scheduler;
		public bool IsConnected => connection?.IsConnected ?? false;
		public SceneGameObjectStore SceneGameObjectStore { get; private set; }
		public JsonSerializerSettings JsonSettings { get; private set; }

		private void CloseScene() => Scene?.Close();

		public void ShowScene()
		{
			Scene = new RTUScene(scheduler);
			Scene.ShowScene();
		}

		public EditorRtuController()
		{
			SceneGameObjectStore = new SceneGameObjectStore(this);
			handlers = new List<IRTUEditorProcessor>();
			scheduler = TaskScheduler.FromCurrentSynchronizationContext();
			connection = new RTUEditorConnection(scheduler);
			JsonSettings = new JSONSettingsCreator().Create();
		}

		public void SendMessageToGame(string message) => connection.SendMessageToGame(message);

		public void Disconnect()
		{
			CloseScene();
			connection.Disconnect();
		}

		public void Connect(string ip, Action connectCallback = null, Action disconnectCallback = null)
		{
			RTUAssetStore.GenerateDictionary();
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
					ShowScene();
					CreateProcessors();
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

		public void CreateProcessors()
		{
			handlers.Clear();
			handlers.AddRange(AppDomain.CurrentDomain.GetAssemblies()
				.SelectMany(x => x.GetTypes())
				.Where(x => x.GetInterfaces().Contains(typeof(IRTUEditorProcessor)))
				.ForEach(x => Debug.Log($"Registering Editor Handlers: {x}"))
				.Select(x =>
					(IRTUEditorProcessor) Activator.CreateInstance(x, new object[] {this}))
				.ToList());
		}
	}
}