using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using RealTimeUpdateRuntime;
using UnityEditor;
using UnityEngine;

namespace RTUEditor
{
	public class ComponentPropChangePayloadRecorderTypeChangeUI : PayloadRecorderTypeChangeUI<ComponentPropertyPayload>
	{
		protected override string name { get; } = "Component Property Changes";

		public ComponentPropChangePayloadRecorderTypeChangeUI(IReadOnlyList<IPayload> payloads,
			JsonSerializerSettings jsonSettings) : base(payloads, jsonSettings) { }

		protected override void ExtractPayloads(IReadOnlyList<IPayload> payloads)
		{
			this.filteredPayloads = payloads
				.OfType<ComponentPropertyPayload>()
				.GroupBy(x => new {x.GameObjectPath, PropertyPath = x.MemberName})
				.Select(group => group.Last()).OrderBy(x => x.GameObjectPath).ToList();
		}

		protected override void DrawValues(ComponentPropertyPayload change,
			List<float> columnWidths)
		{
			if (change == null) return;
			GUILayout.Label(change.GameObjectPath, GUILayout.Width(columnWidths[0]));
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
					var go = GameObject.Find(payload.GameObjectPath);
					if (go == null)
					{
						RTUDebug.LogWarning($"Failed to locate GameObject {payload.InstanceID}");
						continue;
					}

					var compType = payload.ComponentTypeName.GetTypeIncludingUnity();
					var component = go.GetComponent(compType);
					if (component == null)
					{
						RTUDebug.LogWarning(
							$"Failed to locate component {payload.InstanceID} : {payload.ComponentTypeName}");
						continue;
					}

					var member = MemberAdaptorUtils.GetMemberAdapter(compType, payload.MemberName);
					member.SetValue(component, payload.Value);
				}
				catch (Exception e)
				{
					RTUDebug.LogWarning(
						$"Failed to replay component property change for {payload.InstanceID} : {payload.ComponentTypeName} : {e.Message}");
				}
			}
		}
	}
}