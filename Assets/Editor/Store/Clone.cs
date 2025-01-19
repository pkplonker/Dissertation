using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RTUEditor.AssetStore
{
	public class Clone : Dictionary<string, object>
	{
		public string Name { get; private set; }
		public virtual string Type => Path.GetExtension(Name).Trim('.').ToLowerInvariant();

		public Clone(string name, StringComparer StringComparer) : base(StringComparer)
		{
			this.Name = name;
		}

		public Clone(string name)
		{
			this.Name = name;
		}

		public bool ContainsValue(object value, out string key)
		{
			key = this.FirstOrDefault(x => x.Value == value).Key;
			return !string.IsNullOrEmpty(key);
		}
	}
}