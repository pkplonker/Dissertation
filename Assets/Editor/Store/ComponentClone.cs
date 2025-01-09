using System;
using System.Collections.Generic;
using RealTimeUpdateRuntime;
using UnityEngine;

namespace RTUEditor.AssetStore
{
	public class ComponentClone : Clone
	{
		public ComponentClone(string name) : base(name) { }
		public ComponentClone(string name, StringComparer StringComparer) : base(name, StringComparer) { }
		public override string Type => Name;

		public Dictionary<string, object> GetMembersAsJsonCompatible(GameObject gameObject)
		{
			var type = Type.GetTypeIncludingUnity();
			var originalComponent = gameObject.GetComponent(type);
			var dict = new Dictionary<string, object>();
			var adaptors = MemberAdaptorUtils.GetMemberAdapters(type);
			foreach (var prop in adaptors)
			{
				object val = null;

				try
				{
					val = prop.GetValue(originalComponent);
					dict.Add(prop.Name, val);
				}
				catch { }
			}

			return dict;
		}
	}

	public class ComponentCloneTypeComparerer : IEqualityComparer<ComponentClone>
	{
		public bool Equals(ComponentClone x, ComponentClone y) => x?.Name == y?.Name;

		public int GetHashCode(ComponentClone obj) => obj.Name?.GetHashCode() ?? 0;
	}
}