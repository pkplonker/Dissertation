using System.Collections.Generic;
using System.Linq;
using RealTimeUpdateRuntime;
using UnityEngine;

namespace RTUEditor
{
	public class ComponentPropChangePayloadRecorderTypeChangeUI : PayloadRecorderTypeChangeUI<ComponentPropertyPayload>
	{
		protected override string name { get; } = "Component Property Changes";

		public ComponentPropChangePayloadRecorderTypeChangeUI(IReadOnlyList<IPayload> payloads) : base(payloads) { }

		protected override void ExtractPayloads(IReadOnlyList<IPayload> payloads)
		{
			this.filteredPayloads = payloads
				.OfType<ComponentPropertyPayload>()
				.GroupBy(x => new {x.GameObjectPath, x.PropertyPath})
				.Select(group => group.Last()).OrderBy(x => x.GameObjectPath).ToList();
		}

		protected override void DrawValues(ComponentPropertyPayload change,
			List<float> columnWidths)
		{
			if (change == null) return;
			GUILayout.Label(change.GameObjectPath, GUILayout.Width(columnWidths[0]));
			GUILayout.Label(change.PropertyPath, GUILayout.Width(columnWidths[1]));
			GUILayout.Label(change.Value?.ToString() ?? "NULL", GUILayout.Width(columnWidths[2]));
		}

		protected override List<string> GetColumnHeaders() => new() {"GameObject", "Property", "Values"};
		public override void Replay() { }
	}
}