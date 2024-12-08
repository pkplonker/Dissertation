using System;
using System.Collections.Generic;
using System.IO;

namespace RTUEditor.AssetStore
{
	public class Clone : Dictionary<string, object>
	{
		public string Name { get; private set; }
		public string Type => Path.GetExtension(Name).Trim('.').ToLowerInvariant();
		public Clone(string name, StringComparer StringComparer) : base(StringComparer)
		{
			this.Name = name;
		}

		public Clone(string name)
		{
			this.Name = name;
		}
	}

	public class TextureClone : Clone
	{
		public string ByteHash { get; set; }
		public TextureClone(string name) : base(name) { }

		public TextureClone(string name, StringComparer StringComparer) : base(name, StringComparer) { }
	}
}