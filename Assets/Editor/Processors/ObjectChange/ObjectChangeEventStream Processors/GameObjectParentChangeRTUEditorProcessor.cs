using System.IO;
using Newtonsoft.Json;
using RealTimeUpdateRuntime;
using RTUEditor.AssetStore;
using UnityEditor;
using UnityEngine;

namespace RTUEditor.ObjectChange
{
	public class GameObjectParentChangeRTUEditorProcessor : IObjectChangeProcessor
	{
		public ObjectChangeKind ChangeType => ObjectChangeKind.ChangeGameObjectParent;
		public IEditorRtuController RTUController { get; }

		public GameObjectParentChangeRTUEditorProcessor(IEditorRtuController controller)
		{
			this.RTUController = controller;
		}

		public void Process(ObjectChangeEventStream stream, int streamIdx, JsonSerializerSettings jsonSettings,
			SceneGameObjectStore sceneGameObjectStore)
		{
			stream.GetChangeGameObjectParentEvent(streamIdx, out var changeGameObjectParent);
			var gameObjectChanged =
				EditorUtility.InstanceIDToObject(changeGameObjectParent.instanceId) as GameObject;
			var newParentGo =
				EditorUtility.InstanceIDToObject(changeGameObjectParent.newParentInstanceId) as GameObject;

			var payload = new ReparentGameObjectChangeArgs()
			{
				GameObjectName = gameObjectChanged.name,
				NewParentGameObjectName = newParentGo?.name ?? string.Empty,
			}.GeneratePayload(jsonSettings);

			foreach (var load in payload)
			{
				RTUController.SendMessageToGame(load);
			}

			RTUDebug.Log(
				$"{ChangeType}: {gameObjectChanged} to {newParentGo} from scene {changeGameObjectParent.previousScene} to scene {changeGameObjectParent.newScene}.");
		}
	}
}