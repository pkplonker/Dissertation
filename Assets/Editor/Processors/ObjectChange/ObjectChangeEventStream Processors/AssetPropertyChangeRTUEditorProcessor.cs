using System;
using System.IO;
using Newtonsoft.Json;
using RealTimeUpdateRuntime;
using RTUEditor.AssetStore;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace RTUEditor.ObjectChange
{
	public class AssetPropertyChangeRTUEditorProcessor : IObjectChangeProcessor
	{
		private readonly AssetChangePayloadStrategyFactory assetChangePayloadStrategyFactory;
		public ObjectChangeKind ChangeType => ObjectChangeKind.ChangeAssetObjectProperties;
		public IEditorRtuController controller { get; }

		public AssetPropertyChangeRTUEditorProcessor(IEditorRtuController controller)
		{
			this.controller = controller;
			assetChangePayloadStrategyFactory = new AssetChangePayloadStrategyFactory();
		}

		public void Process(ObjectChangeEventStream stream, int streamIdx, JsonSerializerSettings jsonSettings,
			SceneGameObjectStore sceneGameObjectStore)
		{
			{
				stream.GetChangeAssetObjectPropertiesEvent(streamIdx, out var changeAssetObjectPropertiesEvent);
				var changeAsset = EditorUtility.InstanceIDToObject(changeAssetObjectPropertiesEvent.instanceId);
				var changeAssetPath = AssetDatabase.GetAssetPath(changeAsset);
				var extension = Path.GetExtension(changeAssetPath).Trim('.');
				if (changeAsset is AssetImporter importer)
				{ 
					AssetDatabase.ImportAsset(importer.assetPath, ImportAssetOptions.ForceUpdate);
					var asset = AssetDatabase.LoadAssetAtPath(changeAssetPath, typeof(UnityEngine.Object));
					if (asset != null)
					{
						changeAsset = asset;
					}
				}
				if (RTUAssetStore.TryGetExistingClone(changeAssetPath, extension, out var existingClone))
				{
					if (RTUAssetStore.GenerateClone(changeAsset, changeAssetPath, extension, out var newClone))
					{
						
						if (assetChangePayloadStrategyFactory.GeneratePayload(existingClone, newClone, extension,changeAsset,
							    out var payload))
						{
							controller.SendPayloadToGame(payload);
							RTUDebug.Log(
								$"AssetPropertyChanged: {changeAsset} at {changeAssetPath} in scene {changeAssetObjectPropertiesEvent.scene}.");
						}
					}
					else
					{
						RTUDebug.LogWarning($"Failed to generate clone for current asset {changeAssetPath}");
					}
				}
				else
				{
					RTUDebug.LogWarning("Failed to get asset for path");
				}
			}
		}
	}
}