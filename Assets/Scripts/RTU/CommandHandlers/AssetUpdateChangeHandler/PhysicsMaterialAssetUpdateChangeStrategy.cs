using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace RealTimeUpdateRuntime
{
	public class PhysicsMaterialAssetUpdateChangeStrategy : BaseAssetUpdateChangeStrategy
	{
		public override string EXTENSION { get; } = "physicmaterial";

		protected override List<Object> GetElements()
		{
			var elements = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None)
				.Select(x => x.TryGetComponent<Collider>(out var comp) ? comp : null).WhereNotNull()
				.Select(x => x.material).Cast<UnityEngine.Object>().ToList();
			return elements;
		}
	}
}