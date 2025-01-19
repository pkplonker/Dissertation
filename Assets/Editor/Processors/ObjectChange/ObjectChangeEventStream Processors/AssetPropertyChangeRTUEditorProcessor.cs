using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
			stream.GetChangeAssetObjectPropertiesEvent(streamIdx, out var changeAssetObjectPropertiesEvent);
			var changeAsset = EditorUtility.InstanceIDToObject(changeAssetObjectPropertiesEvent.instanceId);
			Process(changeAsset, sceneGameObjectStore);
		}

		public void Process(Object changeAsset, SceneGameObjectStore sceneGameObjectStore = null)
		{
			var changeAssetPath = AssetDatabase.GetAssetPath(changeAsset);
			var extension = Path.GetExtension(changeAssetPath).Trim('.');
			if (changeAsset is AssetImporter importer)
			{
				RTUImportAsset.ImportAsset(importer);

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
					if (assetChangePayloadStrategyFactory.GeneratePayload(existingClone, newClone, extension,
						    changeAsset,
						    out var payload))
					{
						controller.SendPayloadToGame(payload);
						RTUDebug.Log(
							$"AssetPropertyChanged: {changeAsset} at {changeAssetPath}.");
					}
				}
				else
				{
					RTUDebug.LogWarning($"Failed to generate clone for current asset {changeAssetPath}");
				}
			}
			else if (sceneGameObjectStore != null)
			{
				var clones = sceneGameObjectStore.GetClones();
				var results = new List<MultiObjectAssetChange>();
				foreach (var clone in clones.Values.OfType<GameObjectClone>())
				{
					foreach (var component in clone.components)
					{
						if (component.ContainsValue(changeAsset, out var key))
						{
							results.Add(new MultiObjectAssetChange()
							{
								PropName = key,
								ComponentName = component.Name,
								GameObject = clone.Name,
							});
						}
					}
				}

				var args = new MultiObjectAssetChangeEventArgs
				{
					ID = changeAsset.GetInstanceID(),
					ImpactedAssets = results,
					Value = changeAsset,
					ValueType = changeAsset.GetType(),
				};
				controller.SendPayloadToGame(args);
				RTUDebug.Log(
					$"MultiObjectAssetPropertyChanged: {changeAsset} at {changeAssetPath}.");
			}
		}
	}
}