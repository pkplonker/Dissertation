using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
						var bytes = Convert.FromBase64String((string)change);
						var decompressed = bytes.Decompress();
						tex2d.LoadRawTextureData(decompressed);
						tex2d.Apply();
					}
				}
			}
		}
	}
}