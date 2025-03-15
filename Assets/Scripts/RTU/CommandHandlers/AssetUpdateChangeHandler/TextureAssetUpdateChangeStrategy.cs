using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Newtonsoft.Json;
using UnityEngine;
using Object = UnityEngine.Object;

namespace RealTimeUpdateRuntime
{
	public class TextureAssetUpdateChangeStrategy : BaseAssetUpdateChangeStrategy
	{
		public override string EXTENSION { get; } = "png";

		protected override List<Object> GetElements()
		{
			var elements = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None)
				.Select(x => x.TryGetComponent<Renderer>(out var comp) ? comp : null).WhereNotNull()
				.Select(x => x.material).SelectMany(x => x.GetTexturePropertyNameIDs().Select(x.GetTexture))
				.Cast<UnityEngine.Object>().ToList();
			return elements;
		}

		protected override bool IsMemberNull(IMemberAdapter member, string changeKey)
		{
			if (member == null)
			{
				return true;
			}

			return false;
		}

		public override void Update(string payload, JsonSerializerSettings jsonSettings)
		{
			base.Update(payload, jsonSettings);
			var args = JsonConvert.DeserializeObject<TextureAssetPropertyChangeEventArgs>(payload,
				jsonSettings);
			var changes = args.Changes.Where(x =>
				x.Key.Equals("imageContentsHash", StringComparison.InvariantCultureIgnoreCase));
			if (!changes.Any()) return;
			var change = args.ImageData;

			var elements = GetElements();
			var assetName = Path.GetFileNameWithoutExtension(args.Path);
			var matchingAssets = elements.WhereNotNull().Where(x =>
			{
				var name = RemoveInstanceString(x.name, IAssetUpdateChangeStrategy.INSTANCE_STRING);

				return name.TrimEnd(' ').Equals(assetName, StringComparison.InvariantCultureIgnoreCase);
			}).OfType<Texture>();

			if (matchingAssets.Any())
			{
				foreach (var texture in matchingAssets)
				{
					if (texture is Texture2D tex2d)
					{
						try
						{
							var rawTextureData = Convert.FromBase64String((string) args.ImageData);
							var decompressed = rawTextureData.Decompress();
							var colors = MemoryMarshal.Cast<byte, Color32>(decompressed).ToArray();

							if (!tex2d.isReadable || tex2d.format != TextureFormat.RGBA32)
							{
								Texture2D newTex = new Texture2D(tex2d.width, tex2d.height, TextureFormat.RGBA32,
									false);
								newTex.SetPixels32(colors);
								newTex.Apply();

								var renderers = FindObjectsWithTexture(tex2d);
								foreach (var renderer in renderers)
								{
									if (renderer != null)
									{
										renderer.material.mainTexture = newTex;
									}
								}
							}
							else
							{
								tex2d.SetPixels32(colors);
								tex2d.Apply(updateMipmaps: false);
							}
						}
						catch (Exception e)
						{
							RTUDebug.LogWarning($"Texture update failed: {e}");
						}
					}
				}
			}
		}

		private IEnumerable<Renderer> FindObjectsWithTexture(Texture2D texture)
		{
			foreach (var renderer in Object.FindObjectsOfType<Renderer>())
			{
				if (renderer.material?.mainTexture == texture)
				{
					yield return renderer;
				}
			}

			yield break;
		}
	}
}