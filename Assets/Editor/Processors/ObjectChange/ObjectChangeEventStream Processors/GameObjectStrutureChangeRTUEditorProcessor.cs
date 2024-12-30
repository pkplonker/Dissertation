using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using RealTimeUpdateRuntime;
using RTUEditor.AssetStore;
using UnityEditor;
using UnityEngine;

namespace RTUEditor.ObjectChange
{
	public class GameObjectStrutureChangeRTUEditorProcessor : IObjectChangeProcessor
	{
		private readonly SceneGameObjectStore sceneGameObjectStore;
		public ObjectChangeKind ChangeType => ObjectChangeKind.ChangeGameObjectStructure;
		public IEditorRtuController RTUController { get; }

		public GameObjectStrutureChangeRTUEditorProcessor(IEditorRtuController controller)
		{
			this.RTUController = controller;
			this.sceneGameObjectStore = controller.SceneGameObjectStore;
		}

		public void Process(ObjectChangeEventStream stream, int streamIdx, JsonSerializerSettings jsonSettings, SceneGameObjectStore sceneGameObjectStore)
		{
			stream.GetChangeGameObjectStructureEvent(streamIdx, out var changeGameObjectStructure);
			var gameObject =
				EditorUtility.InstanceIDToObject(changeGameObjectStructure.instanceId) as GameObject;
			Debug.Log($"{ChangeType}: {gameObject} in scene {changeGameObjectStructure.scene}.");

			var fullPath = gameObject.GetFullName();

			var currentClone = sceneGameObjectStore.CloneGameObject(gameObject);

			if (sceneGameObjectStore.TryGetExistingClone(fullPath, out var originalClone))
			{
				if (originalClone is GameObjectClone originalGameobjectClone &&
				    currentClone is GameObjectClone currentGameobjectClone &&
				    HasChange(originalGameobjectClone, currentGameobjectClone, fullPath,
					    out List<IChangeArgs> changes))
				{
					sceneGameObjectStore.AddClone(fullPath, currentGameobjectClone);
					try
					{
						foreach (var change in changes)
						{
							var payload = change.GeneratePayload(jsonSettings);
							RTUController.SendMessageToGame(payload);
							RTUDebug.Log(
								$"GameObject component changed {fullPath}.");
						}
					}
					catch (Exception e)
					{
						RTUDebug.LogError($"Failed to generate property change payload string {e.Message}");
					}
				}
			}
		}

		private bool HasChange(GameObjectClone originalGameobjectClone,
			GameObjectClone currentGameobjectClone,
			string fullPath,
			out List<IChangeArgs> payloads)
		{
			payloads = new List<IChangeArgs>();
			var comparer = new ComponentCloneTypeComparerer();
			var removedDifferences =
				originalGameobjectClone.components.Except(currentGameobjectClone.components, comparer);
			if (removedDifferences.Any())
			{
				foreach (var difference in removedDifferences)
				{
					payloads.Add(new GameObjectStructureChangeArgs
					{
						GameObjectPath = fullPath,
						ComponentTypeName = difference.Name,
						IsAdd = false
					});
				}

				return true;
			}

			var addedDifferences =
				currentGameobjectClone.components.Except(originalGameobjectClone.components, comparer);
			if (addedDifferences.Any())
			{
				foreach (var difference in addedDifferences)
				{
					payloads.Add(new GameObjectStructureChangeArgs
					{
						GameObjectPath = fullPath,
						ComponentTypeName = difference.Name,
						IsAdd = true
					});
					
					payloads.Add(new RefreshComponentChangeArgs
					{
						GameObjectPath = fullPath,
						ComponentTypeName = difference.Name,
						Members = difference,
					});
					
				}

				return true;
			}

			return false;
		}
	}
}