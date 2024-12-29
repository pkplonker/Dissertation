using System.Linq;
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
			clone.components = go.GetComponents(typeof(Component))
				.Select(x => componentCloneStrategyFactory.GetCloneStrategy(x)?.Clone(x, x.GetType().ToString()))
				.OfType<ComponentClone>()
				.ToList();
			return clone;
		}
	}
}