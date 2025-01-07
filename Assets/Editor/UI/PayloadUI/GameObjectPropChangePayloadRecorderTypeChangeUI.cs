using System.Collections.Generic;
using System.Linq;
using RealTimeUpdateRuntime;
using UnityEngine;

namespace RTUEditor
{
	public class
		GameObjectPropChangePayloadRecorderTypeChangeUI : PayloadRecorderTypeChangeUI<GameObjectPropertyPayload>
	{
		protected override string name { get; } = "GameObject Property Changes";

		public GameObjectPropChangePayloadRecorderTypeChangeUI(IReadOnlyList<IPayload> payloads) : base(payloads) { }

		protected override void ExtractPayloads(IReadOnlyList<IPayload> payloads)
		{
			this.originalPayloads = payloads.OfType<GameObjectPropertyPayload>().ToList();
			this.filteredPayloads = payloads
				.OfType<GameObjectPropertyPayload>()
				.GroupBy(x => new {x.InstanceID, x.PropertyPath})
				.Select(group => group.Last()).OrderBy(x => x.InstanceID).ToList();
		}

		protected override void DrawValues(GameObjectPropertyPayload change,
			List<float> columnWidths)
		{
			if (change == null) return;
			var originalName = originalPayloads.First(x => x.InstanceID == change.InstanceID).GameObjectPath;
			GUILayout.Label(originalName, GUILayout.Width(columnWidths[0]));
			GUILayout.Label(change.PropertyPath, GUILayout.Width(columnWidths[1]));
			GUILayout.Label(change.Value?.ToString() ?? "NULL", GUILayout.Width(columnWidths[2]));
		}

		protected override List<string> GetColumnHeaders() => new() {"GameObject", "Property", "Values"};
	}
}