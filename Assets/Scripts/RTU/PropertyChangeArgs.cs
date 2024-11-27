using System;

namespace RealTimeUpdateRuntime
{
	[Serializable]
	public class PropertyChangeArgs
	{
		public string GameObjectPath = string.Empty;
		public string ComponentTypeName = string.Empty;
		public string PropertyPath = string.Empty;
		public string Value = string.Empty;
		public Type ValueType;
	}
}