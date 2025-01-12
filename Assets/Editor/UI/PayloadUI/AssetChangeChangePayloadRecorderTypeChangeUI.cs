using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Newtonsoft.Json;
using RealTimeUpdateRuntime;
using UnityEditor;
using UnityEngine;

namespace RTUEditor
{
	public class
		AssetChangeChangePayloadRecorderTypeChangeUI : PayloadRecorderTypeChangeUI<AssetPropertyChangeEventArgs>
	{
		protected override string name { get; } = "Component Add/Delete Changes";

		public AssetChangeChangePayloadRecorderTypeChangeUI(IReadOnlyList<IPayload> payloads,
			JsonSerializerSettings jsonSettings) : base(payloads, jsonSettings) { }

		protected override void ExtractPayloads(IReadOnlyList<IPayload> payloads)
		{
			this.filteredPayloads = payloads.OfType<AssetPropertyChangeEventArgs>().ToList();
			List<AssetPropertyChangeEventArgs> customPayloads = new List<AssetPropertyChangeEventArgs>();
			var result = filteredPayloads
				.GroupBy(x => x.ID);
			foreach (var r in result)
			{
				var changesDict = r
					.SelectMany(x => x.Changes)
					.ToLookup(x => x.Key, x => x.Value)
					.ToDictionary(x => x.Key, x => x.Last());
				var originalValues = r
					.SelectMany(x => x.OriginalValues)
					.ToLookup(x => x.Key, x => x.Value)
					.ToDictionary(x => x.Key, x => x.First());
				customPayloads.Add(new AssetPropertyChangeEventArgs
				{
					ID = r.Key,
					Path = AssetDatabase.GetAssetPath(EditorUtility.InstanceIDToObject(r.Key)),
					Changes = changesDict,
					OriginalValues = originalValues
				});
			}

			filteredPayloads = customPayloads;
		}

		protected override void DrawValues(AssetPropertyChangeEventArgs change,
			List<float> columnWidths)
		{
			if (change == null) return;
			GUILayout.Label(change.Path, GUILayout.Width(columnWidths[0]));
			GUILayout.Label(string.Join(',', change.Changes.Select(x => x.Key)), GUILayout.Width(columnWidths[1]));
		}

		protected override List<string> GetColumnHeaders() => new() {"Path", "Changes"};

		public override void Close()
		{
			Replay();// this is inverse of the others as we need to undo things when they're not selected
		}
		public override bool Replay()
		{
			for (var i = 0; i < toggles.Count; i++)
			{
				if (toggles[i])
					continue; // this is inverse of the others as we need to undo things when they're not selected
				var payload = filteredPayloads[i];
				try
				{
					PerformUndo(payload);
				}
				catch (Exception e)
				{
					RTUDebug.LogWarning(
						$"Failed to handle asset change for {payload.Path} : {e.Message}");
				}
			}

			return toggles.Any(x => x);
		}

		private static void PerformUndo(AssetPropertyChangeEventArgs payload)
		{
			try
			{
				var obj = EditorUtility.InstanceIDToObject(payload.ID);
				var adaptors = MemberAdaptorUtils.GetMemberAdaptersAsDict(obj.GetType());
				foreach (var kvp in payload.OriginalValues)
				{
					var adaptor = adaptors[kvp.Key];
					adaptor.SetValue(obj, kvp.Value);
				}
			}
			catch (Exception e)
			{
				RTUDebug.Log(
					$"Failed to update asset {payload.Path} : {e.Message} : {e?.InnerException}");
			}
		}
	}
}