using System.Collections.Generic;
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
			GameObject destroyParentGo =
				EditorUtility.InstanceIDToObject(destroyGameObjectHierarchyEvent.parentInstanceId) as
					GameObject;
			string parentGameObjectPath = string.Empty;
			List<GameObject> currentChildrenGos = null;
			if (destroyParentGo != null)
			{
				parentGameObjectPath = destroyParentGo.GetFullName();
				currentChildrenGos = destroyParentGo.GetComponentsInChildren<Transform>().Select(x => x.gameObject)
					.Where(x => x != destroyParentGo)
					.ToList();
			}

			var payload = new DestroyGameObjectChangeArgs()
			{
				ParentGameObjectPath = parentGameObjectPath,
				CurrentChildren = currentChildrenGos?.Select(x => x.name).ToList() ?? null,
			}.GeneratePayload(jsonSettings);
			sceneGameObjectStore.RemoveClone(destroyGameObjectHierarchyEvent.instanceId);
			foreach (var load in payload)
			{
				RTUController.SendMessageToGame(load);
			}

			RTUDebug.Log(
				$"{ChangeType}: {destroyGameObjectHierarchyEvent.instanceId} with parent {destroyParentGo} in scene {destroyGameObjectHierarchyEvent.scene}.");
		}
	}
}