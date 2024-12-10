using UnityEngine;

namespace RTUEditor.AssetStore
{
	public interface ICloneAssetStrategy
	{
		public Clone CloneAsset(Object asset, string path);
	}
}