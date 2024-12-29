using System.IO;
using RealTimeUpdateRuntime;
using RTUEditor.AssetStore;
using UnityEditor;
using UnityEngine;

namespace RTUEditor.ObjectChange
{
	public class SceneChangeRTUEditorProcessor : IObjectChangeProcessor
	{
		public ObjectChangeKind ChangeType => ObjectChangeKind.ChangeScene;
		public IMessageSender RTUController { get; }

		public SceneChangeRTUEditorProcessor(EditorRtuController controller)
		{
			this.RTUController = controller;
		}

		public void Process(ObjectChangeEventStream stream, int streamIdx)
		{
			stream.GetChangeSceneEvent(streamIdx, out var changeSceneEvent);
			Debug.Log($"{ChangeType}: {changeSceneEvent.scene}");
		}
	}
}