using System;

namespace RTUEditor.AssetStore
{
	public class ComponentClone : Clone
	{
		public ComponentClone(string name) : base(name) { }
		public ComponentClone(string name, StringComparer StringComparer) : base(name, StringComparer) { }
	}
}