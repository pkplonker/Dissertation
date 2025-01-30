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
		private readonly HashSet<RTUEditorConnection> connections;
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
		public bool IsConnected => connections?.Any(x => x.IsConnected) ?? false;
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
			connections = new(new RTUEditorConnectionComparer());
			JsonSettings = new JSONSettingsCreator().Create();
			payloadRecorder = new PayloadRecorder(JsonSettings);
			RTUAssetPostProcesor.Init(this);
		}

		public void SendPayloadToGame(IPayload payload)
		{
			foreach (var message in payload.GeneratePayload(JsonSettings))
			{
				connections.ForEach(x => x.SendMessageToGame(message));
			}

			payloadRecorder.Record(payload);
		}

		public void Disconnect(string ip)
		{
			var connection = connections.FirstOrDefault(x => x.IPAddress.Equals(ip));
			connection?.Disconnect();
			if (!connections.Any())
			{
				CloseScene();
			}
		}

		public void DisconnectAll()
		{
			CloseScene();
			connections.ForEach(x => Disconnect(x.IPAddress));
		}

		public void Connect(string ip, int port, Action connectCallback = null, Action disconnectCallback = null)
		{
			var connection = new RTUEditorConnection();
			connections.Add(connection);
			connection.Connect(ip, port, OnConnection(connectCallback, connection),
				b => OnDisconnect(disconnectCallback, b, connection));
		}

		private void OnDisconnect(Action disconnectCallback, bool b, RTUEditorConnection connection)
		{
			ClearHandlers();
			try
			{
				CloseScene();
			}
			catch (Exception e)
			{
				RTUDebug.LogError($"Failed to close scene on disconnect: {e.Message}");
			}

			try
			{
				ThreadingHelpers.ActionOnScheduler(RecorderFinish, scheduler);
			}
			catch (Exception e)
			{
				RTUDebug.LogError($"Failed to show replayble changes: {e.Message}");
			}

			try
			{
				disconnectCallback?.Invoke();
			}
			catch (Exception e)
			{
				RTUDebug.LogError($"Failed to execute disconnection callback: {e.Message}");
			}

			connections.Remove(connection);
		}

		private void RecorderFinish()
		{
			payloadRecorder.Finish(x =>
			{
				if (x) ReplayedChanges?.Invoke();
			});
		}

		private Action OnConnection(Action connectCallback, RTUEditorConnection connection)
		{
			return async () =>
			{
				try
				{
					await ThreadingHelpers.ActionOnSchedulerAsync(RTUAssetStore.GenerateDictionary, scheduler);
				}
				catch (Exception e)
				{
					RTUDebug.LogError($"Failed to setup Asset Store: {e.Message}");
					connection.Disconnect();
					return;
				}

				try
				{
					await ThreadingHelpers.ActionOnSchedulerAsync(() => Selection.objects = null, scheduler);
					ShowScene();
					CreateProcessors();
					payloadRecorder.Start();
				}
				catch (Exception e)
				{
					RTUDebug.LogError($"Failed to setup editor for connection: {e.Message}");
					connection.Disconnect();
					return;
				}

				try
				{
					connectCallback?.Invoke();
				}
				catch (Exception e)
				{
					RTUDebug.LogError($"Failed to execute connection callback: {e.Message}");
					connection.Disconnect();
					return;
				}
			};
		}

		public void CreateProcessors()
		{
			ClearHandlers();
			handlers.AddRange(AppDomain.CurrentDomain.GetAssemblies()
				.SelectMany(x => x.GetTypes())
				.Where(x => x.GetInterfaces().Contains(typeof(IRTUEditorProcessor)))
				.ForEach(x => RTUDebug.Log($"Registering Editor Handlers: {x}"))
				.Select(x =>
					(IRTUEditorProcessor) Activator.CreateInstance(x, new object[] {this}))
				.ToList());
		}

		private void ClearHandlers()
		{
			foreach (var handler in handlers.OfType<IDisposable>())
			{
				handler.Dispose();
			}

			handlers.Clear();
		}

		public bool HasConnection(string tempValue)
		{
			return connections.Any(x => x.IPAddress.Equals(tempValue));
		}
	}
}