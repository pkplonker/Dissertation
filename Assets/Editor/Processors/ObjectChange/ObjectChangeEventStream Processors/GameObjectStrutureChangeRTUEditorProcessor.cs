using System;
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

		public void Process(ObjectChangeEventStream stream, int streamIdx)
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
				    HasChange(originalGameobjectClone, currentGameobjectClone, out var changes))
				{
					sceneGameObjectStore.AddClone(fullPath, currentGameobjectClone);
					try
					{
						foreach (var change in changes)
						{
							args.Add(new PropertyChangeArgs()
							{
								GameObjectPath = fullPath,
								ComponentTypeName = component.GetType().AssemblyQualifiedName,
								PropertyPath = change.Key,
								Value = change.Value,
								ValueType = change.Value.GetType()
							}.GeneratePayload(settings));
						}
					}
					catch (Exception e)
					{
						RTUDebug.LogError($"Failed to generate property change payload string {e.Message}");
					}

					return true;
				}
			}
		}

		private bool HasChange(GameObjectClone originalGameobjectClone, GameObjectClone currentGameobjectClone, out object o)
		{
			throw new NotImplementedException();
		}
	}