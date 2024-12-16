using Object = UnityEngine.Object;

namespace RTUEditor.AssetStore
{
	public class ComponentCloneStrategy : DefaultCloneStrategy
	{
		public override Clone Clone(Object asset, string path)
		{
			var clone = CloneInternal(asset, asset.GetType(), new ComponentClone(path)) as ComponentClone;

			return clone;
		}
	}
}