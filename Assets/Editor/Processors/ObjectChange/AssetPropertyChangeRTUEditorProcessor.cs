using System.Collections.Generic;
using Newtonsoft.Json;
using RealTimeUpdateRuntime;
using RTUEditor.AssetStore;
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
			var changes = RTUAssetStore.GetChangedProperties(changeAsset, changeAssetPath);
			var payload = GeneratePayload(changes); // change to strategy pattern for each asset type?
			//MessageSender.SendMessageToGame(payload);
			Debug.Log(
				$"AssetPropertyChanged: {changeAsset} at {changeAssetPath} in scene {changeAssetObjectPropertiesEvent.scene}.");
		}

		private string GeneratePayload(Dictionary<string, object> changes)
		{
			var args = new AssetPropertyChangeEventArgs
			{
				// to be completed
			};
			return $"property,\n{JsonConvert.SerializeObject(args)}";
		}
	}
}