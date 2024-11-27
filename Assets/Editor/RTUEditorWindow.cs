using UnityEditor;
using UnityEngine;

namespace Editor
{
	public class RTUEditorWindow : EditorWindow
	{
		private EditorRtuController controller = new EditorRtuController();
		private string ip = "127.0.0.1";

		[MenuItem("SH/RTU")]
		public static void ShowWindow()
		{
			GetWindow<RTUEditorWindow>("Editor Client");
		}

		private void OnGUI()
		{
			if (controller.IsConnected)
			{
				if (GUILayout.Button("Disconnect from Game"))
				{
					controller.Disconnect();
				}
			}
			else
			{
				if (GUILayout.Button("Connect to Game"))
				{
					controller.Connect(ip, null);
				}
			}

			ip = EditorGUILayout.TextField("IP Address", ip);

			if (GUILayout.Button("Send Test Data to Game"))
			{
				controller.SendMessageToGame("Hello, Game!");
			}

			if (GUILayout.Button("Scene Test"))
			{
				controller.ShowScene();
			}

			GUILayout.Label(controller.IsConnected ? "Connected" : "Disconnected");
		}
	}
}