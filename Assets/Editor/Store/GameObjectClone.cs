using System;
using System.Collections.Generic;

namespace RTUEditor.AssetStore
{
	public class GameObjectClone : Clone
	{
		public GameObjectClone(string name) : base(name) { }
		public GameObjectClone(string name, StringComparer StringComparer) : base(name, StringComparer) { }
		public HashSet<ComponentClone> components = new();
	}
}