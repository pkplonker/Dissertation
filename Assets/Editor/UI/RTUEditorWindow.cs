using System;
using System.Diagnostics;
using System.Threading.Tasks;
using RealTimeUpdateRuntime;
using RTUEditor.AssetStore;
using UnityEditor;
using UnityEngine;

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
			EditorGUILayout.BeginHorizontal();
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
			if (GUILayout.Button("Build"))
			{
				try
				{
					BuildPipeline.BuildPlayer(new string[] {"Assets/Scenes/RTUTest.unity"}, gamePath,
						BuildTarget.StandaloneWindows64, BuildOptions.None);

					//Process.Start(gamePath);
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
				}
				catch (Exception e)
				{
					RTUDebug.LogWarning($"Failed to Launch built game {e.Message}");
				}
			}

			EditorGUILayout.EndHorizontal();
			EditorGUILayout.BeginHorizontal();
			ip = EditorGUILayout.TextField("IP Address", ip);
			if (GUILayout.Button("Run Game & Connect"))
			{
				try
				{
					Task.Run(() =>
					{
						Process.Start(gamePath);
						var timeout = 10;
						var timeoutCount = 0;

						while (!controller.IsConnected && timeoutCount<10)
						{
							timeoutCount++;
							controller.Connect(ip);
							Task.Delay(1000);
						}
					});
					
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
					controller.Connect(ip);
				}
			}

			EditorGUILayout.EndHorizontal();
			EditorGUILayout.BeginHorizontal();

			if (GUILayout.Button("Scene Test"))
			{
				controller.ShowScene();
			}

			if (GUILayout.Button("Reload processors"))
			{
				controller.CreateProcessors();
			}

			if (GUILayout.Button("Reload asset store"))
			{
				RTUAssetStore.GenerateDictionary();
			}

			EditorGUILayout.EndHorizontal();
		}
	}
}