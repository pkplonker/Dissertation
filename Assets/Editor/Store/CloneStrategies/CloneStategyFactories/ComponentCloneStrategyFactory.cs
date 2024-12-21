using System;
using System.Collections.Generic;
using UnityEngine;

namespace RTUEditor.AssetStore
{
	public class ComponentCloneStrategyFactory
	{
		private ICloneStrategy defaultCloneStrategy = new DefaultComponentCloneStratagy();

		private readonly Dictionary<Type, ICloneStrategy> cloneStrategies = new()
		{
			{typeof(MeshRenderer), new RendererComponentCloneStrategy()},
			{typeof(SpriteRenderer), new RendererComponentCloneStrategy()},
			{typeof(MeshFilter), new MeshFilterComponentCloneStrategy()},
		};

		public ICloneStrategy GetCloneStrategy(Component component) =>
			cloneStrategies.GetValueOrDefault(component.GetType(), defaultCloneStrategy);
	}
}