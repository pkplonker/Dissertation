using System;

namespace RTUEditor.AssetStore
{
	public class TextureClone : Clone
	{
		public string ByteHash { get; set; }
		public TextureClone(string name) : base(name) { }

		public TextureClone(string name, StringComparer StringComparer) : base(name, StringComparer) { }
	}
}