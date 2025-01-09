using Newtonsoft.Json;
using RealTimeUpdateRuntime;
using UnityEditor;

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
			RTUDebug.Log($"{ChangeType}: {changeSceneEvent.scene}");
		}
	}
}