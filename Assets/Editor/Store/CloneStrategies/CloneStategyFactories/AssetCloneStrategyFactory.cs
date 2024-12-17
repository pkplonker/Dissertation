using System;
using System.Collections.Generic;

namespace RTUEditor.AssetStore
{
	public class AssetCloneStrategyFactory
	{
		private ICloneStrategy defaultCloneStrategy = new DefaultCloneStrategy();
		private Dictionary<string, ICloneStrategy> cloneStrategies = new(StringComparer.InvariantCultureIgnoreCase)
		{
			{"PNG", new TextureCloneStrategy()},
		};

		public ICloneStrategy GetCloneStrategy(string assetType) =>
			cloneStrategies.GetValueOrDefault(assetType, defaultCloneStrategy);
	}
}