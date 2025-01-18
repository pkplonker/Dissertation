using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEditor;

namespace RealTimeUpdateRuntime
{
	[Serializable]
	public class TextureAssetPropertyChangeEventArgs : AssetPropertyChangeEventArgs
	{
		public Object ImageData { get; set; } = null;

		public TextureAssetPropertyChangeEventArgs() { }

		public TextureAssetPropertyChangeEventArgs(AssetPropertyChangeEventArgs other)
		{
			Changes = other.Changes;
			ID = other.ID;
			OriginalValues = other.OriginalValues;
			Path = other.Path;
			Type = other.Type;
		}

#if UNITY_EDITOR
		public new static TextureAssetPropertyChangeEventArgs GenerateCombinedChange(
			IGrouping<int, AssetPropertyChangeEventArgs> group)
		{
			var changesDict = GenerateChangesDict(group, args => args.Changes, false);
			var originalValues = GenerateChangesDict(group, args => args.OriginalValues, true);
			return new TextureAssetPropertyChangeEventArgs
			{
				ID = group.Key,
				Path = AssetDatabase.GetAssetPath(EditorUtility.InstanceIDToObject(group.Key)),
				Changes = changesDict,
				OriginalValues = originalValues
			};
		}
#endif
	}
}