using UnityEditor;
using UnityEngine;

namespace RTUEditor.ObjectChange
{
	public class AssetPropertyChangeRTUEditorProcessor : IObjectChangeProcessor
	{
		public ObjectChangeKind ChangeType => ObjectChangeKind.ChangeAssetObjectProperties;

		public AssetPropertyChangeRTUEditorProcessor(IMessageSender messageSender)
		{
			this.MessageSender = messageSender;
		}

		public IMessageSender MessageSender { get; }

		public void Process(ObjectChangeEventStream stream, int streamIdx)
		{
			stream.GetChangeAssetObjectPropertiesEvent(streamIdx, out var changeAssetObjectPropertiesEvent);
			var changeAsset = EditorUtility.InstanceIDToObject(changeAssetObjectPropertiesEvent.instanceId);
			var changeAssetPath = AssetDatabase.GUIDToAssetPath(changeAssetObjectPropertiesEvent.guid);
			Debug.Log(
				$"AssetPropertyChanged: {changeAsset} at {changeAssetPath} in scene {changeAssetObjectPropertiesEvent.scene}.");
		}
	}
}