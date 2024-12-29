using Newtonsoft.Json;
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

		public void Process(ObjectChangeEventStream stream, int streamIdx, JsonSerializerSettings jsonSettings)
		{
			stream.GetDestroyGameObjectHierarchyEvent(streamIdx, out var destroyGameObjectHierarchyEvent);
			// The destroyed GameObject can not be converted with EditorUtility.InstanceIDToObject as it has already been destroyed.
			var destroyParentGo =
				EditorUtility.InstanceIDToObject(destroyGameObjectHierarchyEvent.parentInstanceId) as
					GameObject;
			Debug.Log(
				$"{ChangeType}: {destroyGameObjectHierarchyEvent.instanceId} with parent {destroyParentGo} in scene {destroyGameObjectHierarchyEvent.scene}.");
		}
	}
}