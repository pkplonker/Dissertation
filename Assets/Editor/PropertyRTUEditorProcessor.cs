using Newtonsoft.Json;
using RealTimeUpdateRuntime;
using UnityEditor;
using UnityEngine;

namespace Editor
{
	public class PropertyRTUEditorProcessor : IRTUEditorProcessor
	{
		public EditorRtuController controller { get; set; }
		public PropertyRTUEditorProcessor(EditorRtuController controller)
		{
			this.controller = controller;
			Undo.postprocessModifications += PostprocessModificationsCallback;
			Undo.undoRedoPerformed += OnUndoRedoPerformed;
		}

		private void OnUndoRedoPerformed()
		{
			// Need to do something about undoing a change as it's not reflected in the modifications callback 
		}

		private UndoPropertyModification[] PostprocessModificationsCallback(UndoPropertyModification[] modifications)
		{
			if (controller.IsConnected)
			{
				foreach (var modification in modifications)
				{
					if (modification.currentValue is PropertyModification pm)
					{
						ProcessPropertyModification(pm);
					}
				}
			}

			return modifications;
		}

		private void ProcessPropertyModification(PropertyModification pm)
		{
			if (pm.target is Component component)
			{
				var go = component.gameObject;
				var path = GetGameObjectPath(go);
				var args = new PropertyChangeArgs()
				{
					GameObjectPath = path,
					ComponentTypeName = component.GetType().AssemblyQualifiedName,
					PropertyPath = pm.propertyPath,
					Value = pm.value,
					ValueType = pm.value.GetType()
				};
				controller.SendMessageToGame($"property,\n{JsonConvert.SerializeObject(args)}");
				return;
			}
		}

		private string GetGameObjectPath(GameObject obj)
		{
			string path = "/" + obj.name;
			while (obj.transform.parent != null)
			{
				obj = obj.transform.parent.gameObject;
				path = "/" + obj.name + path;
			}

			return path;
		}

		public object Clone() => throw new System.NotImplementedException();
	}
}