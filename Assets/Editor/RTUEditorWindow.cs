using UnityEditor;
using UnityEngine;

namespace Editor
{
	public class RTUEditorWindow : EditorWindow
	{
		private RTUProcessorPropertyUpdate rtuProcessorPropertyUpdate;

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
					RTUEditorConnection.Connect();
					if (rtuProcessorPropertyUpdate == null)
					{
						rtuProcessorPropertyUpdate = new RTUProcessorPropertyUpdate();
					}
					RTUScene.Show();
				}
			}

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