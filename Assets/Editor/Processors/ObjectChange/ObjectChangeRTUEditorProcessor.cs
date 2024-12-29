using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace RTUEditor.ObjectChange
{
	public class ObjectChangeRTUEditorProcessor : IRTUEditorProcessor, IMessageSender
	{
		public EditorRtuController controller { get; set; }
		private Dictionary<ObjectChangeKind, IObjectChangeProcessor> objectChangeProcessors;
		public void SendMessageToGame(string message) => controller.SendMessageToGame(message);
		public bool IsConnected { get; }

		public ObjectChangeRTUEditorProcessor(EditorRtuController controller)
		{
			this.controller = controller;
			ObjectChangeEvents.changesPublished += ChangesPublished;
			CreateObjectChangeProcessors();
		}

		private void CreateObjectChangeProcessors()
		{
			objectChangeProcessors = (AppDomain.CurrentDomain.GetAssemblies()
				.SelectMany(x => x.GetTypes())
				.Where(x => x.GetInterfaces().Contains(typeof(IObjectChangeProcessor)))
				.ForEach(x => Debug.Log($"Registering Object Change Sub-Processor: {x}"))
				.Select(x =>
					(IObjectChangeProcessor) Activator.CreateInstance(x, new object[] {this.controller}))
				.ToDictionary(x => x.ChangeType, x => x));
		}

		// example code from https://docs.unity3d.com/6000.0/Documentation/ScriptReference/ObjectChangeKind.html
		private void ChangesPublished(ref ObjectChangeEventStream stream)
		{
			for (int i = 0; i < stream.length; ++i)
			{
				var type = stream.GetEventType(i);
				if (objectChangeProcessors.TryGetValue(type, out var processor))
				{
					processor.Process(stream, i);
				}
				// switch (type)
				// {
				// 	case ObjectChangeKind.ChangeScene: // SceneChangeRTUEditorProcessor
				// 		stream.GetChangeSceneEvent(i, out var changeSceneEvent);
				// 		Debug.Log($"{type}: {changeSceneEvent.scene}");
				// 		break;
				//
				// 	case ObjectChangeKind.CreateGameObjectHierarchy: // CreateGameObjectChangeRTUEditorProcessor
				// 		stream.GetCreateGameObjectHierarchyEvent(i, out var createGameObjectHierarchyEvent);
				// 		var newGameObject =
				// 			EditorUtility.InstanceIDToObject(createGameObjectHierarchyEvent.instanceId) as GameObject;
				// 		Debug.Log($"{type}: {newGameObject} in scene {createGameObjectHierarchyEvent.scene}.");
				// 		break;
				//
				// 	case ObjectChangeKind.ChangeGameObjectStructureHierarchy: // GameObjectStrutureChangeHierachyRTUEditorProcessor
				// 		stream.GetChangeGameObjectStructureHierarchyEvent(i,
				// 			out var changeGameObjectStructureHierarchy);
				// 		var gameObject =
				// 			EditorUtility.InstanceIDToObject(changeGameObjectStructureHierarchy.instanceId) as
				// 				GameObject;
				// 		Debug.Log($"{type}: {gameObject} in scene {changeGameObjectStructureHierarchy.scene}.");
				// 		break;
				//
				// 	case ObjectChangeKind.ChangeGameObjectStructure: // GameObjectStrutureChangeRTUEditorProcessor
				// 		stream.GetChangeGameObjectStructureEvent(i, out var changeGameObjectStructure);
				// 		var gameObjectStructure =
				// 			EditorUtility.InstanceIDToObject(changeGameObjectStructure.instanceId) as GameObject;
				// 		Debug.Log($"{type}: {gameObjectStructure} in scene {changeGameObjectStructure.scene}.");
				// 		break;
				//
				// 	case ObjectChangeKind.ChangeGameObjectParent: //GameObjectParentChangeRTUEditorProcessor
				// 		stream.GetChangeGameObjectParentEvent(i, out var changeGameObjectParent);
				// 		var gameObjectChanged =
				// 			EditorUtility.InstanceIDToObject(changeGameObjectParent.instanceId) as GameObject;
				// 		var newParentGo =
				// 			EditorUtility.InstanceIDToObject(changeGameObjectParent.newParentInstanceId) as GameObject;
				// 		var previousParentGo =
				// 			EditorUtility.InstanceIDToObject(changeGameObjectParent.previousParentInstanceId) as
				// 				GameObject;
				// 		Debug.Log(
				// 			$"{type}: {gameObjectChanged} from {previousParentGo} to {newParentGo} from scene {changeGameObjectParent.previousScene} to scene {changeGameObjectParent.newScene}.");
				// 		break;
				//
				// 	case ObjectChangeKind.ChangeGameObjectOrComponentProperties: // ignore as handled via other method
				// 		stream.GetChangeGameObjectOrComponentPropertiesEvent(i, out var changeGameObjectOrComponent);
				// 		var goOrComponent = EditorUtility.InstanceIDToObject(changeGameObjectOrComponent.instanceId);
				// 		if (goOrComponent is GameObject go)
				// 		{
				// 			Debug.Log(
				// 				$"{type}: GameObject {go} change properties in scene {changeGameObjectOrComponent.scene}.");
				// 		}
				// 		else if (goOrComponent is Component component)
				// 		{
				// 			Debug.Log(
				// 				$"{type}: Component {component} change properties in scene {changeGameObjectOrComponent.scene}.");
				// 		}
				//
				// 		break;
				//
				// 	case ObjectChangeKind.DestroyGameObjectHierarchy: // DestroyGameObjectHierachyChangeRTUEditorProcessor
				// 		stream.GetDestroyGameObjectHierarchyEvent(i, out var destroyGameObjectHierarchyEvent);
				// 		// The destroyed GameObject can not be converted with EditorUtility.InstanceIDToObject as it has already been destroyed.
				// 		var destroyParentGo =
				// 			EditorUtility.InstanceIDToObject(destroyGameObjectHierarchyEvent.parentInstanceId) as
				// 				GameObject;
				// 		Debug.Log(
				// 			$"{type}: {destroyGameObjectHierarchyEvent.instanceId} with parent {destroyParentGo} in scene {destroyGameObjectHierarchyEvent.scene}.");
				// 		break;
				//
				// 	case ObjectChangeKind.CreateAssetObject:
				// 		stream.GetCreateAssetObjectEvent(i, out var createAssetObjectEvent);
				// 		var createdAsset = EditorUtility.InstanceIDToObject(createAssetObjectEvent.instanceId);
				// 		var createdAssetPath = AssetDatabase.GUIDToAssetPath(createAssetObjectEvent.guid);
				// 		Debug.Log(
				// 			$"{type}: {createdAsset} at {createdAssetPath} in scene {createAssetObjectEvent.scene}.");
				// 		break;
				//
				// 	case ObjectChangeKind.DestroyAssetObject:
				// 		stream.GetDestroyAssetObjectEvent(i, out var destroyAssetObjectEvent);
				// 		// The destroyed asset can not be converted with EditorUtility.InstanceIDToObject as it has already been destroyed.
				// 		Debug.Log(
				// 			$"{type}: Instance Id {destroyAssetObjectEvent.instanceId} with Guid {destroyAssetObjectEvent.guid} in scene {destroyAssetObjectEvent.scene}.");
				// 		break;
				//
				// 	case ObjectChangeKind.ChangeAssetObjectProperties:
				//
				// 		stream.GetChangeAssetObjectPropertiesEvent(i, out var changeAssetObjectPropertiesEvent);
				// 		var changeAsset = EditorUtility.InstanceIDToObject(changeAssetObjectPropertiesEvent.instanceId);
				// 		var changeAssetPath = AssetDatabase.GUIDToAssetPath(changeAssetObjectPropertiesEvent.guid);
				// 		Debug.Log(
				// 			$"{type}: {changeAsset} at {changeAssetPath} in scene {changeAssetObjectPropertiesEvent.scene}.");
				// 		break;
				//
				// 	case ObjectChangeKind.UpdatePrefabInstances:
				// 		stream.GetUpdatePrefabInstancesEvent(i, out var updatePrefabInstancesEvent);
				// 		string s = "";
				// 		s +=
				// 			$"{type}: scene {updatePrefabInstancesEvent.scene}. Instances ({updatePrefabInstancesEvent.instanceIds.Length}):\n";
				// 		foreach (var prefabId in updatePrefabInstancesEvent.instanceIds)
				// 		{
				// 			s += EditorUtility.InstanceIDToObject(prefabId).ToString() + "\n";
				// 		}
				//
				// 		Debug.Log(s);
				// 		break;
				// }
			}
		}
	}
}