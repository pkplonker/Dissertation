using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RealTimeUpdateRuntime
{
	public class MaterialAssetUpdateChangeStrategy : BaseAssetUpdateChangeStrategy
	{
		public override string EXTENSION { get; } = "mat";
		protected override List<UnityEngine.Object> GetElements()
		{
			var elements = UnityEngine.Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None)
				.Select(x => x.TryGetComponent<Renderer>(out var comp) ? comp : null).WhereNotNull()
				.SelectMany(x => x.materials).Cast<UnityEngine.Object>().ToList();
			return elements;
		}
	}
}