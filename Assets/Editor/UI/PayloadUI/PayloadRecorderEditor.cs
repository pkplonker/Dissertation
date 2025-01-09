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
		private static Action<bool> FinishCompleteCallback;

		public static void Show(PayloadRecorder payloadRecorder, JsonSerializerSettings settings,
			Action<bool> finishCompleteCallback)
		{
			FinishCompleteCallback = finishCompleteCallback;
			jsonSettings = settings;
			PayloadRecorderEditor.PayloadRecorder = payloadRecorder;

			converters ??= TypeRepository.GetTypes()
				.Where(x => x.IsSubclassOf(typeof(PayloadRecorderTypeChangeUI)) && !x.IsAbstract).ToList();

			var payloads = PayloadRecorder.Payloads;
			changeUIs ??= new();
			var hasChanges = false;
			foreach (var converter in converters)
			{
				var changeUI = (PayloadRecorderTypeChangeUI) Activator.CreateInstance(converter,
					new object[] {payloads, jsonSettings});
				changeUIs.Add(changeUI);
				hasChanges |= changeUI.HasChanges();
			}

			if (hasChanges)
			{
				ShowInternal();
			}
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
			if (changeUIs != null)
			{
				foreach (var changeUI in changeUIs)
				{
					hasShownContent |= changeUI?.Draw() ?? true;
				}

				if (hasShownContent && GUILayout.Button("Submit"))
				{
					if (EditorUtility.DisplayDialog("This action is irreversible",
						    "Are you sure you want to replay selected changes?", "Yes", "Cancel"))
					{
						var changesMade = false;
						foreach (var changeUI in changeUIs)
						{
							// TODO In theory, the order in which we execute these could be important,
							// for example, handling property changes for renamed GameObjects
							changesMade |= changeUI?.Replay() ?? false;
						}

						FinishCompleteCallback?.Invoke(changesMade);
						Close();
					}
				}
			}

			if (GUILayout.Button("Close"))
			{
				if (EditorUtility.DisplayDialog("All changes will be lost", "Are you sure you want to close?", "Close",
					    "Cancel"))
				{
					FinishCompleteCallback?.Invoke(false);
					Close();
				}
			}
		}

		public static void Finish() => changeUIs = null;
	}
}