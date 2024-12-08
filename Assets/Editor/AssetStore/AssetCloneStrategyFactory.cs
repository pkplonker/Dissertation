using System;
using System.Collections.Generic;

namespace RTUEditor.AssetStore
{
	public class AssetCloneStrategyFactory
	{
		private ICloneAssetStrategy defaultCloneStrategy = new DefaultCloneAssetStrategy();

		private Dictionary<string, ICloneAssetStrategy> cloneStrategies = new(StringComparer.InvariantCultureIgnoreCase)
		{
			{"PNG", new TextureCloneAssetStrategy()},
		};

		public ICloneAssetStrategy GetCloneStrategy(string assetType) =>
			cloneStrategies.GetValueOrDefault(assetType, defaultCloneStrategy);
	}
}