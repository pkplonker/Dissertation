using Newtonsoft.Json;
using RealTimeUpdateRuntime;
using UnityEditor;
using UnityEngine;

namespace RTUEditor.ObjectChange
{
	public class GameObjectStrutureChangeHierachyRTUEditorProcessor : IObjectChangeProcessor
	{
		public ObjectChangeKind ChangeType => ObjectChangeKind.ChangeGameObjectStructureHierarchy;
		public IEditorRtuController RTUController { get; }

		public GameObjectStrutureChangeHierachyRTUEditorProcessor(IEditorRtuController controller)
		{
			this.RTUController = controller;
		}

		public void Process(ObjectChangeEventStream stream, int streamIdx, JsonSerializerSettings jsonSettings, SceneGameObjectStore sceneGameObjectStore)
		{
			stream.GetChangeGameObjectStructureHierarchyEvent(streamIdx,
				out var changeGameObjectStructureHierarchy);
			var gameObject =
				EditorUtility.InstanceIDToObject(changeGameObjectStructureHierarchy.instanceId) as
					GameObject;
			RTUDebug.Log($"{ChangeType}: {gameObject} in scene {changeGameObjectStructureHierarchy.scene}.");
		}
	}
}