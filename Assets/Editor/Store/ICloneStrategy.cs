using UnityEngine;

namespace RTUEditor.AssetStore
{
	public interface ICloneStrategy
	{
		public Clone Clone(Object asset, string path);
	}
}