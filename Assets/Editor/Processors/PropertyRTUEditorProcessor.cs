using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using RealTimeUpdateRuntime;
using RealTimeUpdateRuntime.ClassWrappers;
using UnityEditor;
using UnityEngine;
using Vector3 = System.Numerics.Vector3;

namespace RTUEditor
{
	public class PropertyRTUEditorProcessor : IRTUEditorProcessor
	{
		private readonly SceneGameobjectStore sceneGameObjectStore;
		public IMessageSender controller { get; set; }

		public PropertyRTUEditorProcessor(EditorRtuController controller)
		{
			sceneGameObjectStore = new SceneGameobjectStore(controller);
			this.controller = controller;
			Undo.postprocessModifications += PostprocessModificationsCallback;
			Undo.undoRedoPerformed += OnUndoRedoPerformed;
		}

		private void OnUndoRedoPerformed()
		{
			// TODO Need to do something about undoing a change as it's not reflected in the modifications callback 
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
			if (sceneGameObjectStore.TryGetChange(pm, out HashSet<PropertyChangeArgs> args))
			{
				foreach (var change in args)
				{
					var message =
						$"property,\n{JsonConvert.SerializeObject(change, Formatting.Indented, new JsonSerializerSettings {ReferenceLoopHandling = ReferenceLoopHandling.Ignore})}";
					controller.SendMessageToGame(message);
				}
			}
		}
	}
}