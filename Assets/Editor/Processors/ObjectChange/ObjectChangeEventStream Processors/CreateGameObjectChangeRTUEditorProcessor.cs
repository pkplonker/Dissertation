﻿using Newtonsoft.Json;
using RealTimeUpdateRuntime;
using RTUEditor.AssetStore;
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

		public void Process(ObjectChangeEventStream stream, int streamIdx, JsonSerializerSettings jsonSettings,
			SceneGameObjectStore sceneGameObjectStore)
		{
			stream.GetCreateGameObjectHierarchyEvent(streamIdx, out var createGameObjectHierarchyEvent);
			var newGameObject =
				EditorUtility.InstanceIDToObject(createGameObjectHierarchyEvent.instanceId) as GameObject;
			var payload = new CreateGameObjectPayload()
			{
				GameObjectPath = newGameObject.GetFullName()
			};
			var clone = sceneGameObjectStore.CloneGameObjectAndStore(newGameObject);
			(clone as GameObjectClone).components
				.Clear(); // We need to clear the components so that the subsiquent "ChangeGameObjectStructure" commands add the components for us

			RTUController.SendPayloadToGame(payload);

			RTUDebug.Log($"{ChangeType}: {newGameObject}.");
		}
	}
}