using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace RealTimeUpdateRuntime
{
	[Serializable]
	public class MaterialAssetPropertyChangeEventArgs : AssetPropertyChangeEventArgs
	{
		public Dictionary<string, Dictionary<string, object>> ShaderProperties { get; set; } =
			new(StringComparer.InvariantCultureIgnoreCase);

		[JsonIgnore]
		public Dictionary<string, Dictionary<string, object>> ShaderPropertiesOriginalValues { get; set; } =
			new(StringComparer.InvariantCultureIgnoreCase);

		public MaterialAssetPropertyChangeEventArgs() { }

		public MaterialAssetPropertyChangeEventArgs(AssetPropertyChangeEventArgs other)
		{
			Changes = other.Changes;
			ID = other.ID;
			OriginalValues = other.OriginalValues;
			Path = other.Path;
			Type = other.Type;
		}
	}
}