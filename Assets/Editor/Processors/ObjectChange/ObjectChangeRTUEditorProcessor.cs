using System;
using System.Collections.Generic;
using System.Linq;
using RealTimeUpdateRuntime;
using UnityEditor;
using UnityEngine;

namespace RTUEditor.ObjectChange
{
	public class ObjectChangeRTUEditorProcessor : IRTUEditorProcessor, IMessageSender
	{
		public IEditorRtuController controller { get; set; }
		private Dictionary<ObjectChangeKind, IObjectChangeProcessor> objectChangeProcessors;
		public void SendMessageToGame(string message) => controller.SendMessageToGame(message);
		public bool IsConnected { get; }

		public ObjectChangeRTUEditorProcessor(IEditorRtuController controller)
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
				.ForEach(x => RTUDebug.Log($"Registering Object Change Sub-Processor: {x}"))
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
				RTUDebug.Log(type.ToString());
				if (objectChangeProcessors.TryGetValue(type, out var processor))
				{
					processor.Process(stream, i, controller.JsonSettings, controller.SceneGameObjectStore);
				}
				// switch (type)
				// {
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