using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using RealTimeUpdateRuntime;
using RTUEditor.AssetStore;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace RTUEditor
{
	public class SceneGameobjectStore
	{
		private readonly Scene scene;
		private readonly RTUScene rtuScene;
		private Dictionary<string, Clone> clones = new(StringComparer.CurrentCultureIgnoreCase);
		private GameObjectCloneStrategy gameObjectCloneStrategy = new();

		public SceneGameobjectStore(EditorRtuController controller)
		{
			rtuScene = controller.Scene;
			scene = rtuScene.GetScene();
			CreateClones(scene.GetRootGameObjects().Select(x => x.transform), parentPath: string.Empty);
		}

		// recursive
		private void CreateClones(IEnumerable<Transform> transforms, string parentPath)
		{
			foreach (var trans in transforms)
			{
				var clone = CloneGameObject(trans.gameObject, parentPath);
				if (clones.TryAdd(GetSceneFullName(trans.gameObject, parentPath), clone) && trans.childCount > 0)
				{
					CreateClones(trans.GetChildren(), clone.Name);
				}
			}
		}

		private Clone CloneGameObject(GameObject go, string parentPath)
			=> gameObjectCloneStrategy.Clone(go, GetSceneFullName(go, parentPath));

		private Clone CloneGameObject(GameObject go)
			=> gameObjectCloneStrategy.Clone(go, go.GetFullName());

		private static string GetSceneFullName(GameObject go, string parentPath) =>
			parentPath == string.Empty ? go.name : parentPath + $"/{go.name}";

		public bool TryGetChange(PropertyModification pm, JsonSerializerSettings settings, out HashSet<string> args)
		{
			args = new HashSet<string>();
			if (pm.target is Component component)
			{
				var go = component.gameObject;
				var currentClone = CloneGameObject(go);
				var fullPath = go.GetFullName();
				if (TryGetExistingClone(fullPath, out var originalClone))
				{
					if (originalClone is GameObjectClone originalGameobjectClone &&
					    currentClone is GameObjectClone currentGameobjectClone &&
					    HasChange(originalGameobjectClone, currentGameobjectClone, component, out var changes))
					{
						clones[fullPath] = currentGameobjectClone;
						try
						{
							foreach (var change in changes)
							{
								// todo move to factory
								if (change.Value is UnityEngine.GameObject targetGo)
								{
									args.Add(new GameObjectPropertyChangeArgs()
									{
										GameObjectPath = fullPath,
										ComponentTypeName = component.GetType().FullName,
										PropertyPath = change.Key,
										ValuePath = targetGo.GetFullName(),
									}.GeneratePayload(settings));
								}
								else if (change.Value is UnityEngine.Component Targetcomponent)
								{
									args.Add(new ComponentPropertyChangeArgs()
									{
										GameObjectPath = fullPath,
										ComponentTypeName = component.GetType().FullName,
										PropertyPath = change.Key,
										ValuePath =
											@$"{Targetcomponent.gameObject.GetFullName()}\{Targetcomponent.name}",
									}.GeneratePayload(settings));
								}
								else
								{
									args.Add(new PropertyChangeArgs()
									{
										GameObjectPath = fullPath,
										ComponentTypeName = component.GetType().FullName,
										PropertyPath = change.Key,
										Value = change.Value,
										ValueType = change.Value.GetType()
									}.GeneratePayload(settings));
								}
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

			return false;
		}

		private bool TryGetExistingClone(string name, out Clone result) => clones.TryGetValue(name, out result);

		protected virtual bool HasChange(GameObjectClone originalClone, GameObjectClone currentClone,
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

			var adaptors = MemberAdaptorUtils.GetMemberAdapters(component.GetType());
			foreach (var (originalName, oldValue) in originalCloneComponent)
			{
				if (originalName.Equals("gameobject", StringComparison.InvariantCultureIgnoreCase) ||
				    originalName.Equals("transform", StringComparison.InvariantCultureIgnoreCase)) continue;
				bool handled = false;

				var adaptor = adaptors.FirstOrDefault(x =>
					x.Name.Equals(originalName, StringComparison.InvariantCultureIgnoreCase));

				if (oldValue is Matrix4x4) continue;
				if (!currentCloneComponent.TryGetValue(originalName, out var newValue)) handled = true;
				if (oldValue == null && newValue == null) handled = true;
				if (!handled && adaptor != null && ((oldValue != null && oldValue?.GetType() != adaptor.MemberType) ||
				                                    (newValue != null && newValue?.GetType() != adaptor.MemberType)))
				{
					// The parsed type is not the same as the property type and as such (Because we've a class)
					if (oldValue is ulong || newValue is ulong)
					{
						if (!oldValue?.Equals(newValue) ?? true)
						{
							handled = AddToChanges(changes, originalName, adaptor.GetValue(component));
						}

						continue;
					}

					if (oldValue is not int &&
					    newValue is not int) // this is a unityobject reference so isn't a mismatch
					{
						//Something has gone wrong
						RTUDebug.LogWarning(
							$"type mismatch {newValue?.GetType()} : {oldValue?.GetType()} for {originalName}");
					}
				}

				var type = adaptor.MemberType;

				if (!handled && type.IsSubclassOf(typeof(UnityEngine.Object)))
				{
					try
					{
						handled = AddToChanges(changes, originalName, adaptor.GetValue(component) as Object);
					}
					catch { }

					handled = true;
				}
				else if (!handled && newValue is IEnumerable<Object> newEnumerable &&
				         oldValue is IEnumerable<Object> originalEnumerable &&
				         newValue.GetType() != typeof(string))
				{
					handled = HandleUnityObjectCollection(originalEnumerable, newEnumerable);
				}
				else if (!handled && type.IsArray)
				{
					handled = HandleArray(changes, oldValue, newValue, originalName);
				}
				else if (!handled && newValue is IEnumerable newObjectEnumerable &&
				         oldValue is IEnumerable originalObjectEnumerable &&
				         newValue.GetType() != typeof(string))
				{
					handled = HandleCollection(changes, originalObjectEnumerable, newObjectEnumerable, originalName,
						newValue);
				}

				if (!handled && (type.IsValueType || type != typeof(string)) && !Equals(oldValue, newValue))
				{
					handled = AddToChanges(changes, originalName, newValue);
				}

				if (!handled && type.IsClass)
				{
					if (type == typeof(string) && !Equals(oldValue, newValue))
					{
						handled = AddToChanges(changes, originalName, newValue);
					}

					// need to handle
				}
			}

			return changes.Any();
		}

		private static bool HandleCollection(Dictionary<string, object> changes, IEnumerable originalObjectEnumerable,
			IEnumerable newObjectEnumerable, string originalName, object newValue)
		{
			bool handled = false;
			try
			{
				var originalList = originalObjectEnumerable.Cast<object>().ToList();
				var newList = newObjectEnumerable.Cast<object>().ToList();
				if (originalList.Count == newList.Count)
				{
					for (int i = 0; i < originalList.Count; i++)
					{
						if (!originalList.ElementAt(i).Equals(newList.ElementAt(i)))
						{
							handled = AddToChanges(changes, originalName, newValue);
							break;
						}
					}
				}
				else
				{
					handled = AddToChanges(changes, originalName, newValue);
				}

				handled = true;
			}
			catch { }

			return handled;
		}

		private static bool HandleArray(Dictionary<string, object> changes, object oldValue, object newValue,
			string originalName)
		{
			bool handled = false;
			try
			{
				var originalArray = oldValue as Array;
				var newArray = newValue as Array;

				if (originalArray.Length == newArray.Length)
				{
					for (int i = 0; i < originalArray.Length; i++)
					{
						if (!Equals(originalArray.GetValue(i), newArray.GetValue(i)))
						{
							handled = AddToChanges(changes, originalName, newValue);

							break;
						}
					}
				}
				else
				{
					handled = AddToChanges(changes, originalName, newValue);
				}

				handled = true;
			}
			catch (Exception e)
			{
				Debug.LogWarning($"Failed array comparison {e.Message}");
			}

			return handled;
		}

		private static bool HandleUnityObjectCollection(IEnumerable<Object> originalEnumerable,
			IEnumerable<Object> newEnumerable)
		{
			bool handled = false;
			try
			{
				var originalHashset = new HashSet<Object>(originalEnumerable);
				var newHashset = new HashSet<Object>(newEnumerable);
				if (originalHashset.Count == newHashset.Count)
				{
					for (int i = 0; i < originalHashset.Count; i++)
					{
						if (originalHashset.ElementAt(i).GetInstanceID() ==
						    newHashset.ElementAt(i).GetInstanceID())
						{
							// todo handle
						}
					}
				}
				else
				{
					// todo handle new elements being added / reordered
				}

				handled = true;
			}
			catch { }

			return handled;
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