using RealTimeUpdateRuntime;
using RTUEditor.AssetStore;

namespace RTUEditor.ObjectChange
{
	public interface IAssetChangePayloadStrategy
	{
		bool TryGenerateArgs(Clone originalClone, Clone currentClone, out AssetPropertyChangeEventArgs args);
	}
}