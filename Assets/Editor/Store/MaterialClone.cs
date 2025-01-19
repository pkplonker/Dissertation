using System;
using System.Collections.Generic;
using RealTimeUpdateRuntime;

namespace RTUEditor.AssetStore
{
	public class MaterialClone : Clone
	{
		public Dictionary<string, object> ShaderProperties { get; set; } =
			new(StringComparer.InvariantCultureIgnoreCase);

		public Dictionary<string, object> ShaderPropertiesOriginalValues { get; set; } =
			new(StringComparer.InvariantCultureIgnoreCase);

		public MaterialClone(string name) : base(name) { }

		public MaterialClone(string name, StringComparer StringComparer) : base(name, StringComparer) { }
	}

	
}