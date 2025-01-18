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
			var groupings = filteredPayloads
				.GroupBy(x => x.ID);
			foreach (var group in groupings)
			{
				GenerateChanges(group, customPayloads);
			}

			filteredPayloads = customPayloads;
		}

		private void GenerateChanges(IGrouping<int, AssetPropertyChangeEventArgs> group,
			List<AssetPropertyChangeEventArgs> customPayloads)
		{
			if (group.OfType<MaterialAssetPropertyChangeEventArgs>().Any())
			{
				customPayloads.Add(MaterialAssetPropertyChangeEventArgs.GenerateCombinedChange(group));
			}
			else if (group.OfType<MaterialAssetPropertyChangeEventArgs>().Any())
			{
				customPayloads.Add(TextureAssetPropertyChangeEventArgs.GenerateCombinedChange(group));
			}
			else
			{
				customPayloads.Add(AssetPropertyChangeEventArgs.GenerateCombinedChange(group));
			}
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
			Replay(); // this is inverse of the others as we need to undo things when they're not selected
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