using System;
using System.Linq;
using Newtonsoft.Json;
using RealTimeUpdateRuntime;
using RTUEditor.AssetStore;
using UnityEngine;

namespace RTUEditor.ObjectChange
{
	public class TextureAssetChangePayloadStrategy : DefaultAssetChangePayloadStrategy
	{
		public override bool TryGenerateArgs(Clone originalClone, Clone currentClone, UnityEngine.Object asset,
			out AssetPropertyChangeEventArgs args)
		{
			// TODO needs updating to handle png change
			if (base.TryGenerateArgs(originalClone, currentClone, asset, out args) && asset is Texture2D tex)
			{
				var changes = args.Changes.Where(x =>
					x.Key.Equals("imageContentsHash", StringComparison.InvariantCultureIgnoreCase));
				if (!changes.Any()) return true;
				var newArgs = new TextureAssetPropertyChangeEventArgs(args)
				{
					ImageData = Convert.ToBase64String(tex.GetRawTextureData().Compress())
				};
				args = newArgs;
				return true;
			}

			return false;
		}
	}
}