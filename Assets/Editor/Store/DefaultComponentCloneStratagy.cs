using System.Collections.Generic;
using Object = UnityEngine.Object;

namespace RTUEditor.AssetStore
{
	public class DefaultComponentCloneStratagy : DefaultCloneStrategy
	{
		protected virtual List<string> ExcludedProperties { get; set; } = new List<string>() {};

		public override Clone Clone(Object asset, string path)
		{
			var clone = CloneInternal(asset, asset.GetType(), new ComponentClone(path), ExcludedProperties) as ComponentClone;

			return clone;
		}
	}
}