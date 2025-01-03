using System;
using System.Diagnostics;
using RealTimeUpdateRuntime;
using RTUEditor.AssetStore;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace RTUEditor
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
					RTUDebug.LogWarning($"Failed to Launch built game {e.Message}");
				}
			}

			if (GUILayout.Button("Build + Run Game"))
			{
				try
				{
					BuildPipeline.BuildPlayer(new string[] {"Assets/Scenes/RTUTest.unity"}, gamePath,
						BuildTarget.StandaloneWindows64, BuildOptions.AutoRunPlayer);

					//Process.Start(gamePath);
				}
				catch (Exception e)
				{
					RTUDebug.LogWarning($"Failed to Launch built game {e.Message}");
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

			GUILayout.Label("---------Debug---------------");
			if (GUILayout.Button("Reload processors"))
			{
				controller.CreateProcessors();
			}

			if (GUILayout.Button("Reload asset store"))
			{
				RTUAssetStore.GenerateDictionary();
			}

			GUILayout.Label(controller.IsConnected ? "Connected" : "Disconnected");
		}
	}
}