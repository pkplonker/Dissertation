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
		CreateGameObjectChangePayloadRecorderTypeChangeUI : PayloadRecorderTypeChangeUI<CreateGameObjectPayload>
	{
		private List<GameObjectStructurePayload> componentCreationPayloads;
		private List<RefreshComponentPayload> componentRefreshPayloads;
		protected override string name { get; } = "Create GameObject Changes";

		public CreateGameObjectChangePayloadRecorderTypeChangeUI(IReadOnlyList<IPayload> payloads,
			JsonSerializerSettings jsonSettings) : base(payloads, jsonSettings) { }

		protected override void ExtractPayloads(IReadOnlyList<IPayload> payloads)
		{
			this.originalPayloads = payloads.OfType<CreateGameObjectPayload>().ToList();
			this.filteredPayloads = originalPayloads; // no need to filter as they don't "stack"
			componentCreationPayloads = payloads.OfType<GameObjectStructurePayload>().Where(x =>
					filteredPayloads.Any(y =>
						y.GameObjectPath.Equals(x.GameObjectPath, StringComparison.InvariantCultureIgnoreCase) &&
						x.IsAdd))
				.ToList();
			componentRefreshPayloads = payloads.OfType<RefreshComponentPayload>().Where(x =>
					filteredPayloads.Any(y =>
						y.GameObjectPath.Equals(x.GameObjectPath, StringComparison.InvariantCultureIgnoreCase)))
				.ToList();
		}

		protected override void DrawValues(CreateGameObjectPayload change,
			List<float> columnWidths)
		{
			if (change == null) return;
			GUILayout.Label(change.GameObjectPath, GUILayout.Width(columnWidths[0]));
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
					var go = new GameObject();
					var pathElements = payload.GameObjectPath.Split('/');
					go.name = pathElements.Last();
					if (pathElements.Count() > 1)
					{
						var parentPath = String.Join('/', pathElements);
						var parent = GameObject.Find(parentPath);
						if (parent == null)
						{
							throw new Exception("Unable to locate parent for object creation");
						}

						go.transform.parent = parent.transform;
					}

					componentCreationPayloads.Where(x =>
							x.GameObjectPath.Equals(payload.GameObjectPath,
								StringComparison.InvariantCultureIgnoreCase))
						.ForEach(GameObjectStructureChangePayloadRecorderTypeChangeUI.PerformReplay);
					componentRefreshPayloads.Where(x =>
							x.GameObjectPath.Equals(payload.GameObjectPath,
								StringComparison.InvariantCultureIgnoreCase))
						.ForEach(x => RefreshComponentHandler.Perform(jsonSettings, x));
				}
				catch (Exception e)
				{
					RTUDebug.LogWarning(
						$"Failed to replay GameObject creation change for {payload.GameObjectPath} : {e.Message}");
				}
			}

			return toggles.Any(x => x);
		}
	}
}