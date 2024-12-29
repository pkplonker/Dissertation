using UnityEditor;
using UnityEngine;

namespace RTUEditor.ObjectChange
{
	public class GameObjectStrutureChangeHierachyRTUEditorProcessor : IObjectChangeProcessor
	{
		public ObjectChangeKind ChangeType => ObjectChangeKind.ChangeGameObjectStructureHierarchy;
		public IMessageSender RTUController { get; }

		public GameObjectStrutureChangeHierachyRTUEditorProcessor(EditorRtuController controller)
		{
			this.RTUController = controller;
		}

		public void Process(ObjectChangeEventStream stream, int streamIdx)
		{
			stream.GetChangeGameObjectStructureHierarchyEvent(streamIdx,
				out var changeGameObjectStructureHierarchy);
			var gameObject =
				EditorUtility.InstanceIDToObject(changeGameObjectStructureHierarchy.instanceId) as
					GameObject;
			Debug.Log($"{ChangeType}: {gameObject} in scene {changeGameObjectStructureHierarchy.scene}.");
		}
	}
}