using System;
using System.Collections.Generic;

namespace RTUEditor.AssetStore
{
	public class AssetTypeCollection : Dictionary<string, Clone>
	{
		public AssetTypeCollection() { }

		public AssetTypeCollection(StringComparer StringComparer) : base(StringComparer) { }
	}
}