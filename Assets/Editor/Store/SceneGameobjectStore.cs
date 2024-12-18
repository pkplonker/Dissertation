using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

		public bool TryGetChange(PropertyModification pm, out HashSet<PropertyChangeArgs> args)
		{
			args = new HashSet<PropertyChangeArgs>();
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
						foreach (var change in changes)
						{
							args.Add( new PropertyChangeArgs()
							{
								GameObjectPath = fullPath,
								ComponentTypeName = component.GetType().AssemblyQualifiedName,
								PropertyPath = change.Key,
								Value = change.Value,
								ValueType = change.Value.GetType()
							});
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

			//changes = originalCloneComponent.Except(currentCloneComponent).ToDictionary(x => x.Key, x => x.Value);
			foreach (var (originalName, originalValue) in originalCloneComponent)
			{
				// any component
				if (originalName.Equals("gameObject", StringComparison.InvariantCultureIgnoreCase) ||
				    originalName.Equals("transform", StringComparison.InvariantCultureIgnoreCase)) continue;
				// transform 
				if (originalName.Equals("position", StringComparison.InvariantCultureIgnoreCase) ||
				    originalName.Equals("worldToLocalMatrix", StringComparison.InvariantCultureIgnoreCase) ||
				    originalName.Equals("localToWorldMatrix", StringComparison.InvariantCultureIgnoreCase) ||
				    originalName.Equals("lossyScale", StringComparison.InvariantCultureIgnoreCase)) continue;
				if (!currentCloneComponent.TryGetValue(originalName, out var newValue)) continue;
				//if both unity objects(assets) compared the ptr
				bool handled = false;
				if (!handled && newValue is Object valueAsObject && originalValue is Object propValueAsObject)
				{
					if (valueAsObject.GetInstanceID() != propValueAsObject.GetInstanceID())
					{
						changes.Add(originalName, newValue);
					}

					handled = true;
				}
				else if (!handled && newValue is IEnumerable<Object> newEnumerable &&
				         originalValue is IEnumerable<Object> originalEnumerable &&
				         newValue.GetType() != typeof(string))
				{
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
					}
					catch { }
				}
				else if (!handled && !Equals(originalValue, newValue))
				{
					changes.Add(originalName, newValue);
					handled = true;
				}
			}

			return changes.Any();
		}

		private static ComponentClone GetComponentFromClone(GameObjectClone clone, Component component) =>
			clone?.components?.ToList().FirstOrDefault(x => x.Name == component.GetType().ToString());
	}
}