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
					if (PostProcessChange(change, out var postProcessChange))
					{
						controller.SendMessageToGame($"property,\n{JsonConvert.SerializeObject(postProcessChange)}");
					}
					else
					{
						var message =
							$"property,\n{JsonConvert.SerializeObject(change, Formatting.Indented, new JsonSerializerSettings {ReferenceLoopHandling = ReferenceLoopHandling.Ignore})}";
						controller.SendMessageToGame(message);
					}

					// https://discussions.unity.com/t/jsonserializationexception-self-referencing-loop-detected/877513/8 to mitigate vector3 self looping w/ normalized
				}
			}
		}

		private bool PostProcessChange(PropertyChangeArgs change, out PropertyChangeArgs o)
		{
			o = change.Clone();
			if (change.Value is UnityEngine.Vector3 v3)
			{
				o.Value = new Vector3Wrapper(v3);
				return true;
			}

			return false;
		}
	}

	
}