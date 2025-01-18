using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEditor;

namespace RealTimeUpdateRuntime
{
	[Serializable]
	public class AssetPropertyChangeEventArgs : IPayload
	{
		public static string MESSAGE_IDENTIFER = "AssetUpdate";
		public Dictionary<string, object> Changes { get; set; }

		[JsonIgnore]
		public int ID { get; set; }

		[JsonIgnore]
		public Dictionary<string, object> OriginalValues { get; set; }

		public string Path { get; set; }
		public string Type { get; set; }

		public virtual List<string> GeneratePayload(JsonSerializerSettings JSONSettings) => new()
			{$"{MESSAGE_IDENTIFER}\n{JsonConvert.SerializeObject(this, Formatting.Indented, JSONSettings)}"};

#if UNITY_EDITOR
		public static AssetPropertyChangeEventArgs GenerateCombinedChange(
			IGrouping<int, AssetPropertyChangeEventArgs> group)
		{
			var changesDict = GenerateChangesDict(group, args => args.Changes, orignal: false);
			var originalValues = GenerateChangesDict(group, args => args.OriginalValues, orignal: true);
			return new AssetPropertyChangeEventArgs
			{
				ID = group.Key,
				Path = AssetDatabase.GetAssetPath(EditorUtility.InstanceIDToObject(group.Key)),
				Changes = changesDict,
				OriginalValues = originalValues
			};
		}

		protected static Dictionary<string, object> GenerateChangesDict(
			IGrouping<int, AssetPropertyChangeEventArgs> group,
			Func<AssetPropertyChangeEventArgs, Dictionary<string, object>> propGetter, bool orignal)
		{
			return group
				.SelectMany(x => propGetter?.Invoke(x))
				.ToLookup(x => x.Key, x => x.Value)
				.ToDictionary(x => x.Key, x => orignal ? x.First() : x.Last());
		}
#endif
	}
}