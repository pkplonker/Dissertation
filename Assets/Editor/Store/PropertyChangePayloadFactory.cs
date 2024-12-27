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
				CreateGameObjectPropertyChanage(settings, args, change, fullPath, component, targetGo);
			}
			else if (change.Value is UnityEngine.Component Targetcomponent)
			{
				CreateComponentPropertyChange(settings, args, change, fullPath, component, Targetcomponent);
			}
			else
			{
				CreatePropertyChange(settings, args, change, fullPath, component);
			}
		}

		private static void CreatePropertyChange(JsonSerializerSettings settings, HashSet<string> args, KeyValuePair<string, object> change,
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

		private static void CreateComponentPropertyChange(JsonSerializerSettings settings, HashSet<string> args, KeyValuePair<string, object> change,
			string fullPath, Component component, Component Targetcomponent)
		{
			args.Add(new ComponentPropertyChangeArgs()
			{
				GameObjectPath = fullPath,
				ComponentTypeName = component.GetType().AssemblyQualifiedName,
				PropertyPath = change.Key,
				TargetComponentTypeName = Targetcomponent.GetType(),
				TargetGOPath = Targetcomponent.gameObject.GetFullName(),
			}.GeneratePayload(settings));
		}

		private static void CreateGameObjectPropertyChanage(JsonSerializerSettings settings, HashSet<string> args, KeyValuePair<string, object> change,
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