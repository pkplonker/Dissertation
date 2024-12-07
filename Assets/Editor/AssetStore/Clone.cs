using System.Collections.Generic;

namespace RTUEditor.AssetStore
{
	public class Clone : Dictionary<string, object> { }

	public class TextureClone : Clone
	{
		public string ByteHash { get; set; }
	}

}