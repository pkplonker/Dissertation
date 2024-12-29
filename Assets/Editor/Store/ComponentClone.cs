using System;
using System.Collections.Generic;

namespace RTUEditor.AssetStore
{
	public class ComponentClone : Clone
	{
		public ComponentClone(string name) : base(name) { }
		public ComponentClone(string name, StringComparer StringComparer) : base(name, StringComparer) { }
		public override string Type => Name;
	}

	public class ComponentCloneTypeComparerer : IEqualityComparer<ComponentClone>
	{
		public bool Equals(ComponentClone x, ComponentClone y) => x?.Name == y?.Name;

		public int GetHashCode(ComponentClone obj) => obj.Name?.GetHashCode() ?? 0;
	}
}