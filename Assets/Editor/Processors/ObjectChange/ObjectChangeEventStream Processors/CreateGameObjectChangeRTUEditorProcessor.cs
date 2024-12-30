using Newtonsoft.Json;
using RealTimeUpdateRuntime;
using UnityEditor;
using UnityEngine;

namespace RTUEditor.ObjectChange
{
	public class CreateGameObjectChangeRTUEditorProcessor : IObjectChangeProcessor
	{
		public ObjectChangeKind ChangeType => ObjectChangeKind.CreateGameObjectHierarchy;
		public IEditorRtuController RTUController { get; }

		public CreateGameObjectChangeRTUEditorProcessor(IEditorRtuController controller)
		{
			this.RTUController = controller;
		}

		public void Process(ObjectChangeEventStream stream, int streamIdx, JsonSerializerSettings jsonSettings, SceneGameObjectStore sceneGameObjectStore)
		{
			stream.GetCreateGameObjectHierarchyEvent(streamIdx, out var createGameObjectHierarchyEvent);
			var newGameObject =
				EditorUtility.InstanceIDToObject(createGameObjectHierarchyEvent.instanceId) as GameObject;
			var payload = new CreateGameObjectChangeArgs()
			{
				GameObjectPath = newGameObject.GetFullName()
			}.GeneratePayload(jsonSettings);
			sceneGameObjectStore.CloneGameObject(newGameObject);
			RTUController.SendMessageToGame(payload);
			RTUDebug.Log($"{ChangeType}: {newGameObject}.");
		}
	}
}