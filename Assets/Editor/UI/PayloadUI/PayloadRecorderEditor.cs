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
		private static ComponentPropChangePayloadRecorderTypeChangeUI componentPropChangePayloadRecorderTypeChangeUI;
		private static GameObjectPropChangePayloadRecorderTypeChangeUI gameObjectPropChangePayloadRecorderTypeChangeUI;
		private static Vector2 WINDOW_INITAL_POSITION = new Vector2(100, 100);
		private static Vector2 WINDOW_INITAL_SIZE = new Vector2(1000, 1000);

		public static void Show(PayloadRecorder payloadRecorder, JsonSerializerSettings settings)
		{
			jsonSettings = settings;
			PayloadRecorderEditor.PayloadRecorder = payloadRecorder;
			ShowInternal();
			var payloads = PayloadRecorder.Payloads;
			componentPropChangePayloadRecorderTypeChangeUI =
				new ComponentPropChangePayloadRecorderTypeChangeUI(payloads);
			gameObjectPropChangePayloadRecorderTypeChangeUI =
				new GameObjectPropChangePayloadRecorderTypeChangeUI(payloads);
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
			bool hasShownContent = false;
			hasShownContent |= componentPropChangePayloadRecorderTypeChangeUI?.Draw() ?? true;
			hasShownContent |= gameObjectPropChangePayloadRecorderTypeChangeUI?.Draw() ?? true;

			if (hasShownContent && GUILayout.Button("Submit"))
			{
				// Replay changes.
			}
		}

		public static void Finish()
		{
			componentPropChangePayloadRecorderTypeChangeUI = null;
			gameObjectPropChangePayloadRecorderTypeChangeUI = null;
		}
	}
}