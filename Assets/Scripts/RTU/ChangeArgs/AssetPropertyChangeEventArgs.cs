using System;
using System.Collections.Generic;

namespace RealTimeUpdateRuntime
{
	[Serializable]
	public class AssetPropertyChangeEventArgs
	{
		public Dictionary<string, object> Changes { get; set; }
	}
}