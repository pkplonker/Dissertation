﻿using System;
using System.Collections.Generic;
using System.Linq;
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

		public SceneGameObjectStore(EditorRtuController controller)
		{
			controller.SceneChanged += SceneChanged;
		}

		private void SceneChanged(RTUScene rtuScene)
		{
			scene = rtuScene.GetScene();
			if (scene.HasValue)
			{
				CreateClones(scene.Value.GetRootGameObjects().Select(x => x.transform), parentPath: string.Empty);
			}
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

		private static string GetSceneFullName(GameObject go, string parentPath) =>
			parentPath == string.Empty ? go.name : parentPath + $"/{go.name}";

		public bool TryGetExistingClone(string name, out Clone result) => clones.TryGetValue(name, out result);

		public void AddClone(string fullPath, GameObjectClone newClone) => clones[fullPath] = newClone;
	}
}