using RealTimeUpdateRuntime;
using RTUEditor.AssetStore;

namespace RTUEditor.ObjectChange
{
	public class TextureAssetChangePayloadStrategy : DefaultAssetChangePayloadStrategy
	{
		public override bool TryGenerateArgs(Clone originalClone, Clone currentClone,
			out AssetPropertyChangeEventArgs args)
		{
			// TODO needs updating to handle png change
			return base.TryGenerateArgs(originalClone, currentClone, out args);
		}
	}
}