﻿using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using RealTimeUpdateRuntime;
using UnityEditor;
using UnityEngine;

namespace RTUEditor.ObjectChange
{
	public class DestroyGameObjectHierachyChangeRTUEditorProcessor : IObjectChangeProcessor
	{
		public ObjectChangeKind ChangeType => ObjectChangeKind.DestroyGameObjectHierarchy;
		public IEditorRtuController RTUController { get; }

		public DestroyGameObjectHierachyChangeRTUEditorProcessor(IEditorRtuController controller)
		{
			this.RTUController = controller;
		}

		public void Process(ObjectChangeEventStream stream, int streamIdx, JsonSerializerSettings jsonSettings,
			SceneGameObjectStore sceneGameObjectStore)
		{
			stream.GetDestroyGameObjectHierarchyEvent(streamIdx, out var destroyGameObjectHierarchyEvent);

			sceneGameObjectStore.TryRemoveClone(destroyGameObjectHierarchyEvent.instanceId, out var goName);

			var payload = new DestroyGameObjectChangeArgs()
			{
				GameObjectName = goName,
			}.GeneratePayload(jsonSettings);
			
			foreach (var load in payload)
			{
				RTUController.SendMessageToGame(load);
			}

			RTUDebug.Log(
				$"{ChangeType}: {destroyGameObjectHierarchyEvent.instanceId} in scene {destroyGameObjectHierarchyEvent.scene}.");
		}
	}
}