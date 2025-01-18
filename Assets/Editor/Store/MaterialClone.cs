using System;
using System.Collections.Generic;

namespace RTUEditor.AssetStore
{
	public class MaterialClone : Clone
	{
		public Dictionary<string, Dictionary<string, object>> ShaderProperties { get; set; } = new(StringComparer.InvariantCultureIgnoreCase);

		public Dictionary<string, Dictionary<string, object>> ShaderPropertiesOriginalValues { get; set; } = new(StringComparer.InvariantCultureIgnoreCase);

		public MaterialClone(string name) : base(name)
		{
			SetupDict();
		}

		public MaterialClone(string name, StringComparer StringComparer) : base(name, StringComparer)
		{
			SetupDict();
		}

		private void SetupDict()
		{
			ShaderProperties.Add("float", new(StringComparer.InvariantCultureIgnoreCase));
			ShaderProperties.Add("int", new(StringComparer.InvariantCultureIgnoreCase));
			ShaderProperties.Add("vector", new(StringComparer.InvariantCultureIgnoreCase));
		}
	}
}