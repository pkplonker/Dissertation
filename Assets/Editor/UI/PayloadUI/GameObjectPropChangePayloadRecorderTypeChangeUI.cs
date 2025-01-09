using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using RealTimeUpdateRuntime;
using UnityEditor;
using UnityEngine;

namespace RTUEditor
{
	public class
		GameObjectPropChangePayloadRecorderTypeChangeUI : PayloadRecorderTypeChangeUI<GameObjectPropertyPayload>
	{
		protected override string name { get; } = "GameObject Property Changes";

		public GameObjectPropChangePayloadRecorderTypeChangeUI(IReadOnlyList<IPayload> payloads,
			JsonSerializerSettings jsonSettings) : base(payloads, jsonSettings) { }

		protected override void ExtractPayloads(IReadOnlyList<IPayload> payloads)
		{
			this.originalPayloads = payloads.OfType<GameObjectPropertyPayload>().ToList();
			this.filteredPayloads = payloads
				.OfType<GameObjectPropertyPayload>()
				.GroupBy(x => new {x.InstanceID, PropertyPath = x.MemberName})
				.Select(group => group.Last()).OrderBy(x => x.InstanceID).ToList();
		}

		protected override void DrawValues(GameObjectPropertyPayload change,
			List<float> columnWidths)
		{
			if (change == null) return;
			var originalName = originalPayloads.First(x => x.InstanceID == change.InstanceID).GameObjectPath;
			GUILayout.Label(originalName, GUILayout.Width(columnWidths[0]));
			GUILayout.Label(change.MemberName, GUILayout.Width(columnWidths[1]));
			GUILayout.Label(change.Value?.ToString() ?? "NULL", GUILayout.Width(columnWidths[2]));
		}

		protected override List<string> GetColumnHeaders() => new() {"GameObject", "Property", "Values"};

		public override void Replay()
		{
			for (var i = 0; i < toggles.Count; i++)
			{
				if (!toggles[i]) continue;
				var payload = filteredPayloads[i];
				try
				{
					var originalName = originalPayloads.First(x => x.InstanceID == payload.InstanceID).GameObjectPath;
					var go = GameObject.Find(originalName);
					if (go == null)
					{
						RTUDebug.LogWarning($"Failed to locate GameObject {payload.InstanceID}");
						continue;
					}

					var member = MemberAdaptorUtils.GetMemberAdapter(typeof(GameObject), payload.MemberName);
					member.SetValue(go, payload.Value);
				}
				catch (Exception e)
				{
					RTUDebug.LogWarning(
						$"Failed to replay GameObject property change for {payload.InstanceID} : {e.Message}");
				}
			}
		}
	}
}