using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace RTUEditor.AssetStore
{
	public class GameObjectCloneStrategy : DefaultCloneStrategy
	{
		private readonly ICloneStrategy defaultComponentCloneStratagy = new DefaultComponentCloneStratagy();

		private readonly Dictionary<Type, ICloneStrategy> componentCloneStrategies = new()
		{
			{typeof(MeshRenderer), new RendererComponentCloneStrategy()},
			{typeof(SpriteRenderer), new RendererComponentCloneStrategy()},
			{typeof(MeshFilter), new MeshFilterComponentCloneStrategy()},
		};

		public override Clone Clone(Object asset, string path)
		{
			if (asset is not GameObject go) return null;
			var clone = CloneInternal(asset, asset.GetType(), new GameObjectClone(path)) as GameObjectClone;
			clone.components = go.GetComponents(typeof(Component))
				.Select(x =>
				{
					var type = x.GetType();
					ICloneStrategy strategy = null;
					if (componentCloneStrategies.TryGetValue(type, out var cloneStrat))
					{
						strategy = cloneStrat;
					}
					strategy ??= defaultComponentCloneStratagy;
					return strategy?.Clone(x, x.GetType().ToString());
				})
				.OfType<ComponentClone>()
				.ToHashSet();
			return clone;
		}
	}
}