using System.IO;
using Newtonsoft.Json;
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

		public void Process(ObjectChangeEventStream stream, int streamIdx, JsonSerializerSettings jsonSettings)		{
			stream.GetCreateGameObjectHierarchyEvent(streamIdx, out var createGameObjectHierarchyEvent);
			var newGameObject =
				EditorUtility.InstanceIDToObject(createGameObjectHierarchyEvent.instanceId) as GameObject;
			Debug.Log($"{ChangeType}: {newGameObject} in scene {createGameObjectHierarchyEvent.scene}.");
		}
	}
}