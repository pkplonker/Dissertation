using System.IO;
using RealTimeUpdateRuntime;
using RTUEditor.AssetStore;
using UnityEditor;
using UnityEngine;

namespace RTUEditor.ObjectChange
{
	public class GameObjectParentChangeRTUEditorProcessor : IObjectChangeProcessor
	{
		public ObjectChangeKind ChangeType => ObjectChangeKind.ChangeGameObjectParent;
		public IMessageSender RTUController { get; }

		public GameObjectParentChangeRTUEditorProcessor(EditorRtuController controller)
		{
			this.RTUController = controller;
		}

		public void Process(ObjectChangeEventStream stream, int streamIdx)
		{
			stream.GetChangeGameObjectParentEvent(streamIdx, out var changeGameObjectParent);
			var gameObjectChanged =
				EditorUtility.InstanceIDToObject(changeGameObjectParent.instanceId) as GameObject;
			var newParentGo =
				EditorUtility.InstanceIDToObject(changeGameObjectParent.newParentInstanceId) as GameObject;
			var previousParentGo =
				EditorUtility.InstanceIDToObject(changeGameObjectParent.previousParentInstanceId) as
					GameObject;
			Debug.Log(
				$"{ChangeType}: {gameObjectChanged} from {previousParentGo} to {newParentGo} from scene {changeGameObjectParent.previousScene} to scene {changeGameObjectParent.newScene}.");
		}
	}
}