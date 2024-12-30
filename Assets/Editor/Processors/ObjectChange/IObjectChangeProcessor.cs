using Newtonsoft.Json;
using UnityEditor;

namespace RTUEditor.ObjectChange
{
	internal interface IObjectChangeProcessor
	{
		public ObjectChangeKind ChangeType { get; }

		public void Process(ObjectChangeEventStream stream, int streamIdx, JsonSerializerSettings jsonSettings,
			SceneGameObjectStore sceneGameObjectStore);
	}
}