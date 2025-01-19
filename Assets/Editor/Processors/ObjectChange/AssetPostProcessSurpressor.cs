using System;

namespace RTUEditor
{
	/// <summary>
	/// Surpresses the RTU asset post processor whilst not disposed
	/// </summary>
	public class AssetPostProcessSurpressor : IDisposable
	{
		public AssetPostProcessSurpressor()
		{
			RTUAssetPostProcesor.Surpress = true;
		}
		public void Dispose()
		{
			RTUAssetPostProcesor.Surpress = false;
		}
	}
}