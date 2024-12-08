using System.IO;
using Newtonsoft.Json;
using RealTimeUpdateRuntime;
using RTUEditor.AssetStore;
using UnityEditor;
using UnityEngine;

namespace RTUEditor.ObjectChange
{
	public class AssetPropertyChangeRTUEditorProcessor : IObjectChangeProcessor
	{
		private readonly AssetChangePayloadStrategyFactory assetChangePayloadStrategyFactory;
		public ObjectChangeKind ChangeType => ObjectChangeKind.ChangeAssetObjectProperties;

		public AssetPropertyChangeRTUEditorProcessor(IMessageSender messageSender)
		{
			this.MessageSender = messageSender;
			assetChangePayloadStrategyFactory = new AssetChangePayloadStrategyFactory();
		}

		public IMessageSender MessageSender { get; }

		public void Process(ObjectChangeEventStream stream, int streamIdx)
		{
			stream.GetChangeAssetObjectPropertiesEvent(streamIdx, out var changeAssetObjectPropertiesEvent);
			var changeAsset = EditorUtility.InstanceIDToObject(changeAssetObjectPropertiesEvent.instanceId);
			var changeAssetPath = AssetDatabase.GUIDToAssetPath(changeAssetObjectPropertiesEvent.guid);
			var type = Path.GetExtension(changeAssetPath).Trim('.');

			if (RTUAssetStore.TryGetExistingClone(changeAssetPath, type, out var databaseClone))
			{
				if (RTUAssetStore.GenerateClone(changeAssetPath, type, out var currentClone))
				{
					if (assetChangePayloadStrategyFactory.GeneratePayload(databaseClone,currentClone, type, out var payload))
					{
						MessageSender.SendMessageToGame(payload);
						Debug.Log(
							$"AssetPropertyChanged: {changeAsset} at {changeAssetPath} in scene {changeAssetObjectPropertiesEvent.scene}.");
					}
				}
				else
				{
					Debug.LogWarning($"Failed to generate clone for current asset {changeAssetPath}");
				}
			}
			else
			{
				Debug.LogWarning("Failed to get asset for path");
			}
		}
	}
}