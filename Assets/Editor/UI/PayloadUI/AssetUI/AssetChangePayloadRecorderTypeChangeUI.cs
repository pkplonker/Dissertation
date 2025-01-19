using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using RealTimeUpdateRuntime;
using UnityEditor;
using UnityEngine;

namespace RTUEditor
{
	public abstract class SubAssetChangePayloadRecorderTypeChangeUI
	{
		public abstract List<AssetPropertyChangeEventArgs> Extract(List<AssetPropertyChangeEventArgs> possiblePayloads);
		public List<AssetPropertyChangeEventArgs> FilteredPayloads { get; set; } = new();

		public abstract bool DrawValues(AssetPropertyChangeEventArgs change, List<float> columnWidths);
		public abstract bool PerformUndo(AssetPropertyChangeEventArgs payload);
	}

	public abstract class SubAssetChangePayloadRecorderTypeChangeUI<T> : SubAssetChangePayloadRecorderTypeChangeUI
		where T : AssetPropertyChangeEventArgs
	{
		protected readonly JsonSerializerSettings jsonSettings;

		protected SubAssetChangePayloadRecorderTypeChangeUI(JsonSerializerSettings jsonSettings) =>
			this.jsonSettings = jsonSettings;
	}

	public class AssetChangePayloadRecorderTypeChangeUI : PayloadRecorderTypeChangeUI<AssetPropertyChangeEventArgs>
	{
		private List<SubAssetChangePayloadRecorderTypeChangeUI> subHandlers;
		private List<AssetPropertyChangeEventArgs> localFilteredPayloads = new();

		protected override IReadOnlyList<AssetPropertyChangeEventArgs> filteredPayloads
		{
			get => subHandlers.SelectMany(x => x.FilteredPayloads).Concat(localFilteredPayloads).ToList().AsReadOnly();
			set { }
		}

		protected override string name { get; } = "Asset Changes";

		protected override IReadOnlyList<AssetPropertyChangeEventArgs> originalPayloads { get; set; }

		public AssetChangePayloadRecorderTypeChangeUI(IReadOnlyList<IPayload> payloads,
			JsonSerializerSettings jsonSettings) : base(payloads, jsonSettings) { }

		private List<SubAssetChangePayloadRecorderTypeChangeUI> GenerateSubHandlers(
			JsonSerializerSettings jsonSettings)
		{
			List<SubAssetChangePayloadRecorderTypeChangeUI> localSubhandlers = new();
			var subHandlerTypes = TypeRepository.GetTypes()
				.Where(x => x.IsSubclassOf(typeof(SubAssetChangePayloadRecorderTypeChangeUI)) && !x.IsAbstract)
				.ToList();

			foreach (var converter in subHandlerTypes)
			{
				var subHandlerConcrete = (SubAssetChangePayloadRecorderTypeChangeUI) Activator.CreateInstance(
					converter,
					new object[] {jsonSettings});
				localSubhandlers.Add(subHandlerConcrete);
			}

			return localSubhandlers;
		}

		protected override void ExtractPayloads(IReadOnlyList<IPayload> payloads)
		{
			subHandlers ??= GenerateSubHandlers(jsonSettings);
			var possiblePayloads = payloads.OfType<AssetPropertyChangeEventArgs>().ToList();
			foreach (var handler in subHandlers)
			{
				possiblePayloads = handler.Extract(possiblePayloads);
			}

			this.filteredPayloads = possiblePayloads;
			List<AssetPropertyChangeEventArgs> customPayloads = new List<AssetPropertyChangeEventArgs>();
			foreach (var r in possiblePayloads
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
				customPayloads.Add(new AssetPropertyChangeEventArgs
				{
					ID = r.Key,
					Path = AssetDatabase.GetAssetPath(EditorUtility.InstanceIDToObject(r.Key)),
					Changes = changesDict,
					OriginalValues = originalValues
				});
			}

			localFilteredPayloads = customPayloads;
		}

		protected override void DrawValues(AssetPropertyChangeEventArgs change,
			List<float> columnWidths)
		{
			if (change == null) return;
			bool handled = false;
			foreach (var handler in subHandlers)
			{
				handled = handler.DrawValues(change, columnWidths);
				if (handled) break;
			}

			if (!handled)
			{
				GUILayout.Label(change.Path, GUILayout.Width(columnWidths[0]));
				GUILayout.Label(string.Join(',', change.Changes.Select(x => x.Key)), GUILayout.Width(columnWidths[1]));
			}
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

		private void PerformUndo(AssetPropertyChangeEventArgs payload)
		{
			foreach (var handler in subHandlers)
			{
				if (handler.PerformUndo(payload))
				{
					break;
				}
			}
			
			try
			{
				if (!payload.OriginalValues.Any()) return;
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