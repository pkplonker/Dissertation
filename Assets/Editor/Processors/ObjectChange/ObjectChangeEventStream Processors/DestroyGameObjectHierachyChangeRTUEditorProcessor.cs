using Newtonsoft.Json;
using RealTimeUpdateRuntime;
using UnityEditor;

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

			RTUController.SendPayloadToGame(new DestroyGameObjectPayload()
			{
				GameObjectName = goName,
			});

			RTUDebug.Log(
				$"{ChangeType}: {destroyGameObjectHierarchyEvent.instanceId} in scene {destroyGameObjectHierarchyEvent.scene}.");
		}
	}
}