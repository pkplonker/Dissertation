using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RealTimeUpdateRuntime;
using RTUEditor.AssetStore;
using UnityEditor;

namespace RTUEditor
{
	public class EditorRtuController : IEditorRtuController
	{
		private readonly RTUEditorConnection connection;
		private readonly List<IRTUEditorProcessor> handlers;
		public event Action<RTUScene> SceneChanged;
		public event Action ReplayedChanges;

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
		private PayloadRecorder payloadRecorder;
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
			payloadRecorder = new PayloadRecorder(JsonSettings);
		}

		public void SendPayloadToGame(IPayload payload)
		{
			foreach (var message in payload.GeneratePayload(JsonSettings))
			{
				connection.SendMessageToGame(message);
			}

			payloadRecorder.Record(payload);
		}

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
				RecorderFinish();
			}
			catch (Exception e)
			{
				RTUDebug.LogError($"Failed to close scene on disconnect: {e.Message}");
			}

			try
			{
				disconnectCallback?.Invoke();
			}
			catch (Exception e)
			{
				RTUDebug.LogError($"Failed to execute disconnection callback: {e.Message}");
			}
		}

		private void RecorderFinish()
		{
			payloadRecorder.Finish(x =>
			{
				if (x) ReplayedChanges?.Invoke();
			});
		}

		private Action OnConnection(Action connectCallback)
		{
			return () =>
			{
				try
				{
					Selection.objects = null;
					ShowScene();
					CreateProcessors();
					payloadRecorder.Start();
				}
				catch (Exception e)
				{
					RTUDebug.LogError($"Failed to setup editor for connection: {e.Message}");
					connection.Disconnect();
				}

				try
				{
					connectCallback?.Invoke();
				}
				catch (Exception e)
				{
					RTUDebug.LogError($"Failed to execute connection callback: {e.Message}");
					connection.Disconnect();
				}
			};
		}

		public void CreateProcessors()
		{
			foreach (var handler in handlers.OfType<IDisposable>())
			{
				handler.Dispose();
			}

			handlers.Clear();
			handlers.AddRange(AppDomain.CurrentDomain.GetAssemblies()
				.SelectMany(x => x.GetTypes())
				.Where(x => x.GetInterfaces().Contains(typeof(IRTUEditorProcessor)))
				.ForEach(x => RTUDebug.Log($"Registering Editor Handlers: {x}"))
				.Select(x =>
					(IRTUEditorProcessor) Activator.CreateInstance(x, new object[] {this}))
				.ToList());
		}
	}
}