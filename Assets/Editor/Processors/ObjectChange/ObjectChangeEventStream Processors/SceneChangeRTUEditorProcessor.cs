using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace RTUEditor.ObjectChange
{
	public class SceneChangeRTUEditorProcessor : IObjectChangeProcessor
	{
		public ObjectChangeKind ChangeType => ObjectChangeKind.ChangeScene;
		public IEditorRtuController RTUController { get; }

		public SceneChangeRTUEditorProcessor(IEditorRtuController controller)
		{
			this.RTUController = controller;
		}

		public void Process(ObjectChangeEventStream stream, int streamIdx, JsonSerializerSettings jsonSettings, SceneGameObjectStore sceneGameObjectStore)
		{
			stream.GetChangeSceneEvent(streamIdx, out var changeSceneEvent);
			Debug.Log($"{ChangeType}: {changeSceneEvent.scene}");
		}
	}
}