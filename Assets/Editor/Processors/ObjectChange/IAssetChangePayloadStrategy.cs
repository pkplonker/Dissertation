using RealTimeUpdateRuntime;
using RTUEditor.AssetStore;
using Object = UnityEngine.Object;

namespace RTUEditor.ObjectChange
{
	public interface IAssetChangePayloadStrategy
	{
		bool TryGenerateArgs(Clone originalClone, Clone currentClone, UnityEngine.Object asset,
			out AssetPropertyChangeEventArgs args);

		bool TryGenerateRefreshArgs(Object changeAsset, out AssetPropertyChangeEventArgs args);
	}
}