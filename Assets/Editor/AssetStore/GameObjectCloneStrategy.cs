using Object = UnityEngine.Object;

namespace RTUEditor.AssetStore
{
	public class GameObjectCloneStrategy : DefaultCloneStrategy
	{
		public override Clone Clone(Object asset, string path)
		{
			var clone = CloneInternal(asset, asset.GetType(), new GameObjectClone(path)) as GameObjectClone;

			return clone;
		}
	}
}