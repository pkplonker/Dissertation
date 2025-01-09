using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using RealTimeUpdateRuntime;
using UnityEngine;
using Object = UnityEngine.Object;

namespace RTUEditor
{
	public class DestroyGameObjectChangePayloadRecorderTypeChangeUI : PayloadRecorderTypeChangeUI<DestroyGameObjectPayload>
	{
		protected override string name { get; } = "Destroy GameObject Changes";

		public DestroyGameObjectChangePayloadRecorderTypeChangeUI(IReadOnlyList<IPayload> payloads,
			JsonSerializerSettings jsonSettings) : base(payloads, jsonSettings) { }

		protected override void ExtractPayloads(IReadOnlyList<IPayload> payloads)
		{
			this.originalPayloads = payloads.OfType<DestroyGameObjectPayload>().ToList();
			this.filteredPayloads = originalPayloads; // no need to filter as they don't "stack"
		}

		protected override void DrawValues(DestroyGameObjectPayload change,
			List<float> columnWidths)
		{
			if (change == null) return;
			GUILayout.Label(change.GameObjectName, GUILayout.Width(columnWidths[0]));
		}

		protected override List<string> GetColumnHeaders() => new() {"GameObject"};

		public override bool Replay()
		{
			for (var i = 0; i < toggles.Count; i++)
			{
				if (!toggles[i]) continue;
				var payload = filteredPayloads[i];
				try
				{
					var go = GameObject.Find(payload.GameObjectName);
					Object.DestroyImmediate(go);
				}
				catch (Exception e)
				{
					RTUDebug.LogWarning(
						$"Failed to replay GameObject destroy change for {payload.GameObjectName} : {e.Message}");
				}
			}

			return toggles.Any(x => x);
		}
	}
}