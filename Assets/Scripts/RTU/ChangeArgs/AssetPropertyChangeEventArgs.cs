using System;
using System.Collections.Generic;

namespace RealTimeUpdateRuntime
{
	[Serializable]
	public class AssetPropertyChangeEventArgs
	{
		public Dictionary<string, object> Changes { get; set; }
		public string Path { get; set; }
		public string Type { get; set; }
	}
}