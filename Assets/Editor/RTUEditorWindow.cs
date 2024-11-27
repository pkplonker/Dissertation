using UnityEditor;
using UnityEngine;

namespace Editor
{
	public class RTUEditorWindow : EditorWindow
	{
		private RTUProcessorPropertyUpdate rtuProcessorPropertyUpdate;
		private string ip = "127.0.0.1";

		[MenuItem("SH/RTU")]
		public static void ShowWindow()
		{
			GetWindow<RTUEditorWindow>("Editor Client");
		}

		private void OnGUI()
		{
			if (RTUEditorConnection.IsConnected)
			{
				if (GUILayout.Button("Disconnect from Game"))
				{
					RTUEditorConnection.Disconnect();
				}
			}
			else
			{
				if (GUILayout.Button("Connect to Game"))
				{
					RTUEditorConnection.Connect(ip, () =>
					{
						rtuProcessorPropertyUpdate ??= new RTUProcessorPropertyUpdate();
						RTUScene.Show();
					});
				}
			}

			ip = EditorGUILayout.TextField("IP Address", ip);

			if (GUILayout.Button("Send Test Data to Game"))
			{
				RTUEditorConnection.SendMessageToGame("Hello, Game!");
			}

			if (GUILayout.Button("Scene Test"))
			{
				RTUScene.Show();
			}

			GUILayout.Label(RTUEditorConnection.IsConnected ? "Connected" : "Disconnected");
		}
	}
}