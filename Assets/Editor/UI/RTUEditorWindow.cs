using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using RealTimeUpdateRuntime;
using RTUEditor.AssetStore;
using UnityEditor;
using UnityEngine;

namespace RTUEditor
{
	public class RTUEditorWindow : EditorWindow
	{
		private List<string> potentialConnections = new();
		private EditorRtuController controller = new EditorRtuController();
		private string gamePath = "S:\\Users\\pkplo\\OneDrive\\Desktop\\RTUBuild\\Dev\\RTUIdeaTest.exe";
		private IntScriptableObject PortSO;

		[MenuItem("SH/RTU")]
		public static void ShowWindow()
		{
			GetWindow<RTUEditorWindow>("Editor Client");
		}

		private void OnGUI()
		{
			PortSO = AssetDatabase.LoadAssetAtPath<IntScriptableObject>("Assets/port.asset");

			EditorGUILayout.BeginHorizontal();
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

			// if (GUILayout.Button("Run Game & Connect"))
			// {
			// 	try
			// 	{
			// 		Task.Run(() =>
			// 		{
			// 			Process.Start(gamePath);
			// 			var timeout = 10;
			// 			var timeoutCount = 0;
			//
			// 			while (!controller.IsConnected && timeoutCount < 10)
			// 			{
			// 				timeoutCount++;
			// 				//controller.Connect(IPSO.Value, PortSO.Value);
			// 				Task.Delay(1000);
			// 			}
			// 		});
			// 	}
			// 	catch (Exception e)
			// 	{
			// 		RTUDebug.LogWarning($"Failed to Launch built game {e.Message}");
			// 	}
			// }

			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
			gamePath = EditorGUILayout.TextField("Game Exe Path", gamePath);
			EditorGUILayout.LabelField("Port", GUILayout.MaxWidth(70));
			PortSO.Value = EditorGUILayout.IntField(string.Empty, PortSO.Value);
			EditorGUILayout.EndHorizontal();

			BigSeperator();
			var newConnections = new List<string>();

			foreach (var connection in potentialConnections)
			{
				EditorGUILayout.BeginHorizontal();
				var tempValue = EditorGUILayout.TextField("IP: ", connection);
				if (controller.HasConnection(tempValue))
				{
					if (GUILayout.Button("Disconnect", GUILayout.MaxWidth(75)))
					{
						controller.Disconnect(tempValue);
					}
				}
				else
				{
					if (GUILayout.Button("Connect", GUILayout.MaxWidth(75)) && IPAddress.TryParse(tempValue, out _))
					{
						controller.Connect(tempValue, PortSO.Value);
					}
				}

				if (GUILayout.Button("Remove", GUILayout.MaxWidth(75)))
				{
					controller.Disconnect(tempValue);
				}
				else
				{
					newConnections.Add(tempValue);
				}

				EditorGUILayout.EndHorizontal();
			}

			potentialConnections = newConnections;

			if (GUILayout.Button("Add New Connection"))
			{
				potentialConnections.Add(string.Empty);
			}

			if (GUILayout.Button("Connect All"))
			{
				foreach (var pc in potentialConnections)
				{
					controller.Connect(pc, PortSO.Value);
				}
			}

			BigSeperator();

			SupportActions();
		}

		private static void BigSeperator()
		{
			EditorGUILayout.Space();
			EditorGUILayout.Space();
			EditorGUILayout.Separator();
			EditorGUILayout.Space();
			EditorGUILayout.Space();
		}

		private void SupportActions()
		{
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