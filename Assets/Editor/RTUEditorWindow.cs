using System;
using System.Diagnostics;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Editor
{
	public class RTUEditorWindow : EditorWindow
	{
		private EditorRtuController controller = new EditorRtuController();
		private string ip = "127.0.0.52";
		private string gamePath = "S:\\Users\\pkplo\\OneDrive\\Desktop\\RTUBuild\\Dev\\RTUIdeaTest.exe";

		[MenuItem("SH/RTU")]
		public static void ShowWindow()
		{
			GetWindow<RTUEditorWindow>("Editor Client");
		}

		private void OnGUI()
		{
			gamePath = EditorGUILayout.TextField("Game Exe Path", gamePath);

			if (GUILayout.Button("Run Game"))
			{
				try
				{
					Process.Start(gamePath);
				}
				catch (Exception e)
				{
					Debug.LogWarning($"Failed to Launch built game {e.Message}");
				}
			}
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