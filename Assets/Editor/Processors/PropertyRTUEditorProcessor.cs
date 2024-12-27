using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using RealTimeUpdateRuntime;
using UnityEditor;
using UnityEngine;

namespace RTUEditor
{
	public class PropertyRTUEditorProcessor : IRTUEditorProcessor
	{
		private readonly SceneGameobjectStore sceneGameObjectStore;
		private readonly JsonSerializerSettings JSONSettings;
		private readonly IMessageSender controller;

		public PropertyRTUEditorProcessor(EditorRtuController controller)
		{
			sceneGameObjectStore = new SceneGameobjectStore(controller);
			this.controller = controller;
			Undo.postprocessModifications += PostprocessModificationsCallback;
			Undo.undoRedoPerformed += OnUndoRedoPerformed;
			JSONSettings = new JSONSettingsCreator().Create();
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
			if (sceneGameObjectStore.TryGetChange(pm, JSONSettings, out HashSet<string> changes))
			{
				foreach (var change in changes)
				{
					try
					{
						controller.SendMessageToGame(change);
					}
					catch (Exception e)
					{
						RTUDebug.LogWarning($"Unable to create property change message {e.Message}");
					}
				}
			}
		}
	}
}