using System;
using System.Collections.Generic;
using System.Linq;
using RealTimeUpdateRuntime;
using RTUEditor.AssetStore;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RTUEditor
{
	public class SceneGameObjectStore
	{
		private Scene? scene;
		private Dictionary<string, Clone> clones = new(StringComparer.CurrentCultureIgnoreCase);
		private GameObjectCloneStrategy gameObjectCloneStrategy = new();
		private RTUScene rtuScene;

		public SceneGameObjectStore(EditorRtuController controller)
		{
			controller.SceneChanged += SceneChanged;
		}

		private void SceneChanged(RTUScene rtuScene)
		{
			if (this.rtuScene != null)
			{
				rtuScene.SceneCreated -= RefreshScene;
			}

			this.rtuScene = rtuScene;
			rtuScene.SceneCreated += RefreshScene;
		}

		private void RefreshScene(Scene? scene)
		{
			this.scene = scene;
			RefreshClones();
		}

		private void RefreshClones()
		{
			if (scene.HasValue)
			{
				CreateClonesStart(scene.Value.GetRootGameObjects().Select(x => x.transform), parentPath: string.Empty);
			}
		}

		private void CreateClonesStart(IEnumerable<Transform> select, string parentPath)
		{
			clones.Clear();
			CreateClones(scene.Value.GetRootGameObjects().Select(x => x.transform), parentPath: string.Empty);
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

		public Clone CloneGameObject(GameObject go)
			=> gameObjectCloneStrategy.Clone(go, go.GetFullName());

		public Clone CloneGameObjectAndStore(GameObject go)
		{
			var clone = CloneGameObject(go);
			AddClone(go.GetFullName(), clone as GameObjectClone);
			return clone;
		}

		private static string GetSceneFullName(GameObject go, string parentPath) =>
			parentPath == string.Empty ? go.name : parentPath + $"/{go.name}";

		public bool TryGetExistingClone(string name, out Clone result) => clones.TryGetValue(name, out result);

		public bool TryGetExistingGameObjectClone(int instanceId, out Clone result)
		{
			result = clones.Values.OfType<GameObjectClone>().FirstOrDefault(x => x.InstanceID == instanceId);
			return result != null;
		}

		public void AddClone(string fullPath, GameObjectClone newClone) => clones[fullPath] = newClone;

		public bool TryRemoveClone(int instanceId, out string name)
		{
			name = string.Empty;
			if (scene == null)
			{
				RTUDebug.LogWarning("Trying to remove gameobject when no scene is set in scene store");
				return false;
			}

			KeyValuePair<string, Clone> clone = clones.FirstOrDefault(x =>
			{
				if (x.Value is GameObjectClone goc)
				{
					return goc.InstanceID == instanceId;
				}

				return false;
			});
			name = (clone.Value as GameObjectClone)?.Name ?? String.Empty;
			if (clone.Equals(default(KeyValuePair<string, Clone>)) || !clones.Remove(clone.Key))
			{
				RTUDebug.LogWarning($"Failed to destroy clone for {instanceId}");
				return false;
			}

			return true;
		}

		public IReadOnlyList<Clone> GetReadOnlyClones() => clones.Values.ToList().AsReadOnly();

		public void Refresh()
		{
			throw new NotImplementedException();
		}
	}
}