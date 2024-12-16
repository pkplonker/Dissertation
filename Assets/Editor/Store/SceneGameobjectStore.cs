using System.Collections.Generic;
using System.Linq;
using System.Text;
using RealTimeUpdateRuntime;
using RTUEditor.AssetStore;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RTUEditor
{
	public class SceneGameobjectStore
	{
		private readonly Scene scene;
		private readonly RTUScene rtuScene;
		private Dictionary<GameObject, Clone> clones = new();
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
				if (clones.TryAdd(trans.gameObject, clone) && trans.childCount > 0)
				{
					CreateClones(trans.GetChildren(), clone.Name);
				}
			}
		}

		private Clone CloneGameObject(GameObject go, string parentPath)
			=> gameObjectCloneStrategy.Clone(go, parentPath == string.Empty ? go.name : parentPath + $"/{go.name}");

		public bool TryGetChange(PropertyModification pm, out PropertyChangeArgs args)
		{
			args = null;
			if (pm.target is Component component)
			{
				var go = component.gameObject;
				var path = GetGameObjectPath(go);
				args = new PropertyChangeArgs()
				{
					GameObjectPath = path,
					ComponentTypeName = component.GetType().AssemblyQualifiedName,
					PropertyPath = pm.propertyPath,
					Value = pm.value,
					ValueType = pm.value.GetType()
				};
				return true;
			}

			return true;
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
	}
}