using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace RealTimeUpdateRuntime
{
	[Serializable]
	public class TextureAssetPropertyChangeEventArgs : AssetPropertyChangeEventArgs
	{
		public Object ImageData { get; set; } = null;

		public TextureAssetPropertyChangeEventArgs() { }

		public TextureAssetPropertyChangeEventArgs(AssetPropertyChangeEventArgs other)
		{
			Changes = other.Changes;
			ID = other.ID;
			OriginalValues = other.OriginalValues;
			Path = other.Path;
			Type = other.Type;
		}
	}
}