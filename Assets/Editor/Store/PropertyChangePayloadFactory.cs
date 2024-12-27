using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using RealTimeUpdateRuntime;
using UnityEngine;

namespace RTUEditor
{
	public class PropertyChangePayloadFactory
	{
		public void CreatePayload(JsonSerializerSettings settings, HashSet<string> args,
			KeyValuePair<string, object> change, string fullPath,
			Component component)
		{
			if (change.Value is UnityEngine.GameObject targetGo)
			{
				CreateGameObjectPropertyChange(settings, args, change, fullPath, component, targetGo);
			}
			else if (change.Value is UnityEngine.Component Targetcomponent)
			{
				CreateComponentPropertyChange(settings, args, change, fullPath, component, Targetcomponent);
			}
			else if (change.Value is IList list)
			{
				var type = list.GetType().GetElementTypeForCollection();
				if (type == typeof(UnityEngine.Object) || type.IsSubclassOf(typeof(UnityEngine.Object)))
				{
					CreateUnityObjectCollectionPropertyChange(settings, args, change, fullPath, component, type, list);
				}
				else
				{
					CreatePropertyChange(settings, args, change, fullPath, component);
				}
			}
			else
			{
				CreatePropertyChange(settings, args, change, fullPath, component);
			}
		}

		private static void CreateUnityObjectCollectionPropertyChange(JsonSerializerSettings settings, HashSet<string> args,
			KeyValuePair<string, object> change, string fullPath, Component component, Type type, IList il)
		{
			if (type.IsSubclassOf(typeof(GameObject)) || type == typeof(UnityEngine.GameObject))
			{
				CreateGameObjectCollectionPropertyChange(settings, args, change, fullPath, component, il);
			}

			else if (type.IsSubclassOf(typeof(Component)) || type == typeof(UnityEngine.Component))
			{
				CreateComponentCollectionPropertyChange(settings, args, change, fullPath, component, il);
			}
			else
			{
				// todo implement
				Debug.Log("Asset collection");
			}
		}

		private static void CreateGameObjectCollectionPropertyChange(JsonSerializerSettings settings, HashSet<string> args,
			KeyValuePair<string, object> change, string fullPath, Component component, IList il)
		{
			var pathList = new List<string>();
			foreach (var element in il)
			{
				try
				{
					if (element is GameObject go)
					{
						pathList.Add(go.GetFullName());
					}

					continue;
				}
				catch (UnityEngine.UnassignedReferenceException) { }

				pathList.Add(null);
			}

			args.Add(new GameObjectCollectionPropertyChangeArgs
			{
				GameObjectPath = fullPath,
				ComponentTypeName = component.GetType().AssemblyQualifiedName,
				PropertyPath = change.Key,
				ValuePaths = pathList,
			}.GeneratePayload(settings));
		}

		private static void CreateComponentCollectionPropertyChange(JsonSerializerSettings settings, HashSet<string> args,
			KeyValuePair<string, object> change, string fullPath, Component component, IList il)
		{
			var pathList = new List<ComponentCollectionElement>();
			foreach (var element in il)
			{
				try
				{
					if (element is Component comp)
					{
						pathList.Add(new ComponentCollectionElement()
						{
							TargetGOPath = comp.gameObject.GetFullName(),
							TargetComponentType = comp.GetType(), 
						});
					}

					continue;
				}
				catch (UnityEngine.UnassignedReferenceException) { }

				pathList.Add(null);
			}

			args.Add(new ComponentCollectionPropertyChangeArgs
			{
				GameObjectPath = fullPath,
				ComponentTypeName = component.GetType().AssemblyQualifiedName,
				PropertyPath = change.Key,
				ValuePaths = pathList,
			}.GeneratePayload(settings));
		}

		private static void CreatePropertyChange(JsonSerializerSettings settings, HashSet<string> args,
			KeyValuePair<string, object> change,
			string fullPath, Component component)
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

		private static void CreateComponentPropertyChange(JsonSerializerSettings settings, HashSet<string> args,
			KeyValuePair<string, object> change,
			string fullPath, Component component, Component Targetcomponent)
		{
			args.Add(new ComponentPropertyChangeArgs()
			{
				GameObjectPath = fullPath,
				ComponentTypeName = component.GetType().AssemblyQualifiedName,
				PropertyPath = change.Key,
				TargetComponentType = Targetcomponent.GetType(),
				TargetGOPath = Targetcomponent.gameObject.GetFullName(),
			}.GeneratePayload(settings));
		}

		private static void CreateGameObjectPropertyChange(JsonSerializerSettings settings, HashSet<string> args,
			KeyValuePair<string, object> change,
			string fullPath, Component component, GameObject targetGo)
		{
			args.Add(new GameObjectPropertyChangeArgs()
			{
				GameObjectPath = fullPath,
				ComponentTypeName = component.GetType().AssemblyQualifiedName,
				PropertyPath = change.Key,
				ValuePath = targetGo.GetFullName(),
			}.GeneratePayload(settings));
		}
	}
}