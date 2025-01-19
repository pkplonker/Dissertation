using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using RealTimeUpdateRuntime;
using UnityEditor;

namespace RTUEditor
{
	public class TextureSubAssetChangePayloadRecorderTypeChangeUI : SubAssetChangePayloadRecorderTypeChangeUI<
		TextureAssetPropertyChangeEventArgs>
	{
		public TextureSubAssetChangePayloadRecorderTypeChangeUI(JsonSerializerSettings jsonSettings) : base(
			jsonSettings) { }

		public override List<AssetPropertyChangeEventArgs> Extract(List<AssetPropertyChangeEventArgs> possiblePayloads)
		{
			var interimPayloads = possiblePayloads.OfType<TextureAssetPropertyChangeEventArgs>().ToList();
			var result = possiblePayloads.Except(interimPayloads).ToList();
			var customPayloads = new List<TextureAssetPropertyChangeEventArgs>();
			foreach (var r in interimPayloads
				         .GroupBy(x => x.ID))
			{
				var changesDict = r
					.SelectMany(x => x.Changes)
					.ToLookup(x => x.Key, x => x.Value)
					.ToDictionary(x => x.Key, x => x.Last());
				var originalValues = r
					.SelectMany(x => x.OriginalValues)
					.ToLookup(x => x.Key, x => x.Value)
					.ToDictionary(x => x.Key, x => x.First());

				customPayloads.Add(new TextureAssetPropertyChangeEventArgs()
				{
					ID = r.Key,
					Path = AssetDatabase.GetAssetPath(EditorUtility.InstanceIDToObject(r.Key)),
					Changes = changesDict,
					OriginalValues = originalValues,
				});
			}

			this.FilteredPayloads = customPayloads.OfType<AssetPropertyChangeEventArgs>().ToList();
			return result.OfType<AssetPropertyChangeEventArgs>().ToList();
		}

		public override bool DrawValues(AssetPropertyChangeEventArgs change, List<float> columnWidths) => false;
		public override bool PerformUndo(AssetPropertyChangeEventArgs payload) => false;
	}
}