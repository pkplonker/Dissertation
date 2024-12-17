using System;
using System.Security.Cryptography;
using UnityEngine;
using Object = UnityEngine.Object;

namespace RTUEditor.AssetStore
{
	public class TextureCloneStrategy : DefaultCloneStrategy
	{
		public override Clone Clone(Object asset, string path)
		{
			var clone = CloneInternal(asset, asset.GetType(), new TextureClone(path)) as TextureClone;

			try
			{
				using (var md5 = MD5.Create())
				{
					clone.ByteHash = BitConverter.ToString(md5.ComputeHash((asset as Texture2D).GetRawTextureData()))
						.Replace("-", "")
						.ToLowerInvariant();
				}
			}
			catch (Exception e)
			{
				Debug.LogError($"Failed to generate texture byte hash {asset.name}: {e.Message}");
			}

			return clone;
		}
	}
}