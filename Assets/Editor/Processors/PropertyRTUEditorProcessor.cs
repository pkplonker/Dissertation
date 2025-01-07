using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using RealTimeUpdateRuntime;
using RTUEditor.AssetStore;
using UnityEditor;
using UnityEngine;

namespace RTUEditor
{
	public class PropertyRTUEditorProcessor : IRTUEditorProcessor
	{
		private readonly JsonSerializerSettings JSONSettings;
		private readonly EditorRtuController controller;
		private Dictionary<GameObject, Clone> clones = new();
		private readonly SceneGameObjectStore sceneGameObjectStore;

		public PropertyRTUEditorProcessor(EditorRtuController controller)
		{
			this.controller = controller;
			sceneGameObjectStore = controller.SceneGameObjectStore;

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
				clones.Clear();
				foreach (var modification in modifications)
				{
					if (modification.currentValue is PropertyModification pm)
					{
						Clone clone = null;
						if (pm.target is Component component)
						{
							var go = component.gameObject;
							if (!clones.TryGetValue(go, out clone))
							{
								clone = sceneGameObjectStore.CloneGameObject(go);
								clones[go] = clone;
							}
						}

						ProcessPropertyModification(pm, clone);
					}
				}
			}

			return modifications;
		}

		private void ProcessPropertyModification(PropertyModification pm, Clone clone)
		{
			if (TryGetChange(pm, JSONSettings, clone, out HashSet<IPayload> changes))
			{
				foreach (var change in changes)
				{
					try
					{
						controller.SendPayloadToGame(change);
					}
					catch (Exception e)
					{
						RTUDebug.LogWarning($"Unable to create property change message {e.Message}");
					}
				}
			}
		}

		public bool TryGetChange(PropertyModification pm, JsonSerializerSettings settings, Clone currentClone,
			out HashSet<IPayload> args)
		{
			args = new HashSet<IPayload>();
			if (pm.target is Component component)
			{
				if (ComponentChange(settings, currentClone, args, component)) return true;
			}

			if (pm.target is GameObject gameObject)
			{
				if (GameObjectChange(settings, pm, args, gameObject)) return true;
			}

			return false;
		}

		private bool GameObjectChange(JsonSerializerSettings settings, PropertyModification pm, HashSet<IPayload> args,
			GameObject gameObject)
		{
			GameObjectClone currentClone = null;
			if (pm.target is GameObject targetGameObject)
			{
				currentClone = sceneGameObjectStore.CloneGameObject(targetGameObject) as GameObjectClone;
			}

			if (sceneGameObjectStore.TryGetExistingGameObjectClone(gameObject.GetInstanceID(),
				    out var originalClone))
			{
				if (HasGameObjectChange(originalClone, currentClone, out var changes))
				{
					sceneGameObjectStore.TryRemoveClone(gameObject.GetInstanceID(), out var originalName);
					sceneGameObjectStore.AddClone(gameObject.GetFullName(), currentClone);
					try
					{
						foreach (var change in changes)
						{
							args.Add(new GameObjectPropertyPayload
							{
								InstanceID = currentClone.InstanceID,
								GameObjectPath = originalName, // in case this is what has changed
								PropertyPath = change.Key,
								Value = change.Value,
								ValueType = change.Value.GetType()
							});
						}
					}
					catch (Exception e)
					{
						RTUDebug.LogError($"Failed to generate GameObject property change payload string {e.Message}");
					}

					return true;
				}
			}
			else
			{
				RTUDebug.LogWarning($"Unable to locate current GameObject clone for {currentClone?.Name}");
			}

			return false;
		}

		private bool ComponentChange(JsonSerializerSettings settings, Clone currentClone, HashSet<IPayload> args,
			Component component)
		{
			var go = component.gameObject;
			var fullPath = go.GetFullName();
			if (sceneGameObjectStore.TryGetExistingGameObjectClone(go.GetInstanceID(), out var originalClone))
			{
				if (originalClone is GameObjectClone originalGameobjectClone &&
				    currentClone is GameObjectClone currentGameobjectClone &&
				    HasComponentChange(originalGameobjectClone, currentGameobjectClone, component, out var changes))
				{
					sceneGameObjectStore.AddClone(fullPath, currentGameobjectClone);
					try
					{
						foreach (var change in changes)
						{
							args.Add(new ComponentPropertyPayload()
							{
								GameObjectPath = fullPath,
								ComponentTypeName = component.GetType().AssemblyQualifiedName,
								PropertyPath = change.Key,
								Value = change.Value,
								ValueType = change.Value.GetType()
							});
						}
					}
					catch (Exception e)
					{
						RTUDebug.LogError($"Failed to generate property change payload string {e.Message}");
					}

					return true;
				}
			}

			return false;
		}

		private bool HasGameObjectChange(Clone originalClone, GameObjectClone currentClone,
			out Dictionary<string, object> changes)
		{
			changes = new Dictionary<string, object>();
			var adaptors = MemberAdaptorUtils.GetMemberAdaptersAsDict(typeof(GameObject));
			foreach (var (originalName, oldValue) in originalClone)
			{
				if (!adaptors.TryGetValue(originalName, out var adaptor)) continue;

				var type = adaptor.MemberType;

				if (!currentClone.TryGetValue(originalName, out var newValue)) continue;
				if (oldValue == null && newValue == null) continue;

				if (HandleValueType(changes, type, oldValue, newValue, originalName)) continue;
				if (HandleClass(changes, type, oldValue, newValue, originalName)) continue;
			}

			return changes.Any();
		}

		protected virtual bool HasComponentChange(GameObjectClone originalClone, GameObjectClone currentClone,
			Component component,
			out Dictionary<string, object> changes)
		{
			changes = new Dictionary<string, object>();
			var originalCloneComponent = GetComponentFromClone(originalClone, component);
			var currentCloneComponent = GetComponentFromClone(currentClone, component);
			// working on the assumption that only the values change.
			if (originalCloneComponent == null || currentCloneComponent == null)
			{
				Debug.LogWarning("Failed to generate component clone from change");
				return false;
			}

			var adaptors = MemberAdaptorUtils.GetMemberAdaptersAsDict(component.GetType());

			foreach (var (originalName, oldValue) in originalCloneComponent)
			{
				if (originalName.Equals("gameobject", StringComparison.InvariantCultureIgnoreCase) ||
				    originalName.Equals("transform", StringComparison.InvariantCultureIgnoreCase)) continue;

				if (!adaptors.TryGetValue(originalName, out var adaptor)) continue;

				var type = adaptor.MemberType;

				if (oldValue is Matrix4x4) continue;
				if (!currentCloneComponent.TryGetValue(originalName, out var newValue)) continue;
				if (oldValue == null && newValue == null) continue;

				if (HandleClassUlong(component, changes, adaptor, oldValue, newValue, originalName)) continue;
				if (HandleValueType(changes, type, oldValue, newValue, originalName)) continue;
				if (HandleClass(changes, type, oldValue, newValue, originalName)) continue;
			}

			return changes.Any();
		}

		private static bool HandleClassUlong(Component component, Dictionary<string, object> changes,
			IMemberAdapter adaptor,
			object oldValue, object newValue, string originalName)
		{
			if (adaptor != null && ((oldValue != null && oldValue?.GetType() != adaptor.MemberType) ||
			                        (newValue != null && newValue?.GetType() != adaptor.MemberType)))
			{
				// The parsed type is not the same as the property type and as such (Because it's a class)
				if (oldValue is ulong || newValue is ulong)
				{
					if (!oldValue?.Equals(newValue) ?? true)
					{
						return AddToChanges(changes, originalName, adaptor.GetValue(component));
					}

					return true;
				}

				if (oldValue is not int &&
				    newValue is not int) // this is a unityobject reference so isn't a mismatch
				{
					if (oldValue is not IList && newValue is not IList)
					{
						RTUDebug.LogWarning(
							$"type mismatch {newValue?.GetType()} : {oldValue?.GetType()} for {originalName}");
						return true;
					}
				}
			}

			return false;
		}

		private static bool HandleClass(Dictionary<string, object> changes, Type type, object oldValue,
			object newValue,
			string originalName)
		{
			if (type.IsClass)
			{
				if (type == typeof(string) && !Equals(oldValue, newValue))
				{
					return AddToChanges(changes, originalName, newValue);
				}
			}

			return false;
		}

		private static bool HandleValueType(Dictionary<string, object> changes, Type type,
			object oldValue, object newValue,
			string originalName)
		{
			if ((type.IsValueType || type != typeof(string)) && !Equals(oldValue, newValue))
			{
				return AddToChanges(changes, originalName, newValue);
			}

			return false;
		}

		private static bool AddToChanges(Dictionary<string, object> changes, string originalName, object newValue)
		{
			bool handled;
			changes.Add(originalName, newValue);
			handled = true;
			return handled;
		}

		private static ComponentClone GetComponentFromClone(GameObjectClone clone, Component component) =>
			clone?.components?.ToList().FirstOrDefault(x => x.Name == component.GetType().ToString());
	}
}