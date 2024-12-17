using System;

namespace RealTimeUpdateRuntime
{
	[Serializable]
	public class PropertyChangeArgs
	{
		public string GameObjectPath = string.Empty;
		public string ComponentTypeName = string.Empty;
		public string PropertyPath = string.Empty;
		public object Value = string.Empty;
		public Type ValueType;

		public PropertyChangeArgs Clone() =>
			new()
			{
				GameObjectPath = this.GameObjectPath,
				ComponentTypeName = this.ComponentTypeName,
				PropertyPath = this.PropertyPath,
				Value = this.Value,
				ValueType = this.ValueType,
			};
	}
}