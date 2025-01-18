using System.Collections.Generic;
using System.Linq;
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
				.Select(x => x.material).SelectMany(x=>x.GetTexturePropertyNameIDs().Select(x.GetTexture)).Cast<UnityEngine.Object>().ToList();
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
	}
}