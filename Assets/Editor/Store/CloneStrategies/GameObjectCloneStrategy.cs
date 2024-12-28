using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace RTUEditor.AssetStore
{
	public class GameObjectCloneStrategy : DefaultCloneStrategy
	{
		private static ComponentCloneStrategyFactory componentCloneStrategyFactory = new();

		public override Clone Clone(Object asset, string path)
		{
			if (asset is not GameObject go) return null;
			var clone = CloneInternal(asset, asset.GetType(), new GameObjectClone(path)) as GameObjectClone;
			var components = new ConcurrentBag<ComponentClone>();

			Parallel.ForEach(go.GetComponents(typeof(Component)), component =>
			{
				var strategy = componentCloneStrategyFactory.GetCloneStrategy(component);
				if (strategy != null)
				{
					var clonedComponent = strategy.Clone(component, component.GetType().ToString());
					if (clonedComponent is ComponentClone componentClone)
					{
						components.Add(componentClone);
					}
				}
			});

			clone.components = components.ToList();
			return clone;
		}
	}
}