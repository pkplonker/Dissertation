using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using RealTimeUpdateRuntime;
using UnityEditor;
using UnityEngine;

namespace RTUEditor
{
	public class PayloadRecorderEditor : EditorWindow
	{
		private static JsonSerializerSettings jsonSettings;
		private static PayloadRecorder PayloadRecorder;
		private bool showComponentChange = true;
		private static Vector2 WINDOW_INITAL_POSITION = new Vector2(100, 100);
		private static Vector2 WINDOW_INITAL_SIZE = new Vector2(1000, 1000);
		private static bool init;
		private static List<Type> converters;
		private static List<PayloadRecorderTypeChangeUI> changeUIs = new();

		public static void Show(PayloadRecorder payloadRecorder, JsonSerializerSettings settings)
		{
			jsonSettings = settings;
			PayloadRecorderEditor.PayloadRecorder = payloadRecorder;

			converters ??= TypeRepository.GetTypes()
				.Where(x => x.IsSubclassOf(typeof(PayloadRecorderTypeChangeUI)) && !x.IsAbstract).ToList();

			var payloads = PayloadRecorder.Payloads;
			changeUIs ??= new();
			foreach (var converter in converters)
			{
				changeUIs.Add((PayloadRecorderTypeChangeUI) Activator.CreateInstance(converter, new object[] {payloads}));
			}

			ShowInternal();
		}

		[MenuItem("SH/Replay RTU Changes")]
		public static void ShowWindow()
		{
			ShowInternal();
		}

		private static void ShowInternal()
		{
			var window = GetWindow<PayloadRecorderEditor>("Replay RTU Changes", true);
			window.position = new Rect(WINDOW_INITAL_POSITION, WINDOW_INITAL_SIZE);
		}

		private void OnGUI()
		{
			var hasShownContent = false;
			foreach (var changeUI in changeUIs)
			{
				hasShownContent |= changeUI?.Draw() ?? true;
			}

			if (hasShownContent && GUILayout.Button("Submit"))
			{
				foreach (var changeUI in changeUIs)
				{
					changeUI?.Replay();
				}
				Close();
			}
		}

		public static void Finish() => changeUIs = null;
	}
}