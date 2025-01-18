using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEditor;

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

#if UNITY_EDITOR
		public new static MaterialAssetPropertyChangeEventArgs GenerateCombinedChange(
			IGrouping<int, AssetPropertyChangeEventArgs> group)
		{
			var changesDict = GenerateChangesDict(group, args => args.Changes, false);
			var originalValues = GenerateChangesDict(group, args => args.OriginalValues, true);
			var shaderProperties = GenerateShaderPropertiesDict(group, args => args.ShaderProperties, false);
			var shaderPropertiesOriginalValues =
				GenerateShaderPropertiesDict(group, args => args.ShaderPropertiesOriginalValues, true);
			return new MaterialAssetPropertyChangeEventArgs
			{
				ID = group.Key,
				Path = AssetDatabase.GetAssetPath(EditorUtility.InstanceIDToObject(group.Key)),
				Changes = changesDict,
				ShaderProperties = shaderProperties,
				ShaderPropertiesOriginalValues = shaderPropertiesOriginalValues,
				OriginalValues = originalValues
			};
		}

		protected static Dictionary<string, Dictionary<string, object>> GenerateShaderPropertiesDict(
			IGrouping<int, AssetPropertyChangeEventArgs> group,
			Func<MaterialAssetPropertyChangeEventArgs, Dictionary<string, Dictionary<string, object>>> propGetter,
			bool orignal)
		{
			var dict = new Dictionary<string, Dictionary<string, object>>();
			group.OfType<MaterialAssetPropertyChangeEventArgs>()
				.SelectMany(x => propGetter?.Invoke(x))
				.ToLookup(x => x.Key, x => x.Value)
				.ForEach(x =>
				{
					dict.Add(x.Key, x.SelectMany(y => y)
						.ToLookup(y => y.Key, y => y.Value)
						.ToDictionary(y => y.Key, y => orignal ? y.First() : y.Last()));
					;
				});

			return dict;
		}

#endif
		
	}
}