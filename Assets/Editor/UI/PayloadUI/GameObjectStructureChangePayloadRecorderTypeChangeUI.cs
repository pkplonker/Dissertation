using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using RealTimeUpdateRuntime;
using UnityEngine;

namespace RTUEditor
{
	public class
		GameObjectStructureChangePayloadRecorderTypeChangeUI : PayloadRecorderTypeChangeUI<GameObjectStructurePayload>
	{
		protected override string name { get; } = "Component Add/Delete Changes";

		public GameObjectStructureChangePayloadRecorderTypeChangeUI(IReadOnlyList<IPayload> payloads,
			JsonSerializerSettings jsonSettings) : base(payloads, jsonSettings) { }

		protected override void ExtractPayloads(IReadOnlyList<IPayload> payloads)
		{
			var goAddPayloads = payloads.OfType<CreateGameObjectPayload>().Select(x => x.GameObjectPath);
			this.originalPayloads = payloads.OfType<GameObjectStructurePayload>().Where(x =>
					!goAddPayloads.Any(y => y.Equals(x.GameObjectPath, StringComparison.InvariantCultureIgnoreCase)))
				.ToList(); // Filter out any that are from GameObject Adds as we don't want to manually show these as these will be captured with GameObject Add
			this.filteredPayloads = originalPayloads; // no need to filter as they don't "stack"
		}

		protected override void DrawValues(GameObjectStructurePayload change,
			List<float> columnWidths)
		{
			if (change == null) return;
			GUILayout.Label(change.GameObjectPath, GUILayout.Width(columnWidths[0]));
			GUILayout.Label(change.ComponentTypeName, GUILayout.Width(columnWidths[1]));
			GUILayout.Label(change.IsAdd ? "Add" : "Delete", GUILayout.Width(columnWidths[2]));
		}

		protected override List<string> GetColumnHeaders() => new() {"GameObject", "Component", "Add/Delete"};

		public override bool Replay()
		{
			for (var i = 0; i < toggles.Count; i++)
			{
				if (!toggles[i]) continue;
				var payload = filteredPayloads[i];
				try
				{
					PerformReplay(payload);
				}
				catch (Exception e)
				{
					RTUDebug.LogWarning(
						$"Failed to replay component structure change for {payload.GameObjectPath} : {payload.ComponentTypeName} : {e.Message}");
				}
			}

			return toggles.Any(x => x);
		}

		public static void PerformReplay(GameObjectStructurePayload payload)
		{
			try
			{
				GameObjectStructureChangeHandler.Perform(payload);
			}
			catch (Exception e)
			{
				RTUDebug.Log($"Failed to update GameObject structure {payload.GameObjectPath} : {e.Message} : {e?.InnerException}");
			}
		}
	}
}