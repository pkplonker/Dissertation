using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using RealTimeUpdateRuntime;
using UnityEditor;
using UnityEngine;

namespace RTUEditor
{
	public abstract class PayloadRecorderTypeChangeUI
	{
		public abstract bool Draw();

		public abstract bool Replay();

		public abstract bool HasChanges();
	}

	public abstract class PayloadRecorderTypeChangeUI<T> : PayloadRecorderTypeChangeUI where T : IPayload
	{
		protected List<bool> toggles = new();
		protected IReadOnlyList<T> filteredPayloads;
		protected IReadOnlyList<T> originalPayloads;
		protected abstract string name { get; }
		private bool show = true;
		protected readonly JsonSerializerSettings jsonSettings;
		public override bool HasChanges() => filteredPayloads.Any();

		protected PayloadRecorderTypeChangeUI(IReadOnlyList<IPayload> payloads, JsonSerializerSettings jsonSettings)
		{
			this.jsonSettings = jsonSettings;
			ExtractPayloads(payloads);
		}

		protected abstract void ExtractPayloads(IReadOnlyList<IPayload> payloads);

		public override bool Draw()
		{
			if (!filteredPayloads.Any()) return false;
			show = EditorGUILayout.Foldout(show, name + $"({filteredPayloads.Count})");
			if (!show) return true;
			var columns = GetColumnHeadersInternal();
			var columnWidths = new List<float>();
			if (toggles.Count != filteredPayloads.Count)
			{
				toggles.Clear();
				toggles = Enumerable.Repeat(false, filteredPayloads.Count).ToList();
			}

			EditorGUILayout.BeginHorizontal();
			foreach (var column in columns)
			{
				columnWidths.Add(Mathf.Max(100, column.Length * 20));
				EditorGUILayout.LabelField(column, EditorStyles.boldLabel, GUILayout.Width(columnWidths.Last()));
			}

			EditorGUILayout.EndHorizontal();
			if (filteredPayloads == null || !(filteredPayloads?.Any() ?? false)) return true;
			for (var i = 0; i < filteredPayloads.Count; i++)
			{
				var change = filteredPayloads[i];
				EditorGUILayout.BeginHorizontal();
				DrawValues(change, columnWidths);
				toggles[i] = EditorGUILayout.ToggleLeft(string.Empty, toggles[i]);
				EditorGUILayout.EndHorizontal();
			}

			return true;
		}

		private List<string> GetColumnHeadersInternal()
		{
			var headers = GetColumnHeaders();
			if (headers != null)
			{
				headers.Add("Replay");
			}

			return headers;
		}

		protected abstract void DrawValues(T change, List<float> columnWidths);

		protected abstract List<string> GetColumnHeaders();
	}
}