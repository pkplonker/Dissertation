using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using Editor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RTUEditorWindow : EditorWindow
{
	[MenuItem("SH/RTU")]
	public static void ShowWindow()
	{
		GetWindow<RTUEditorWindow>("Editor Client");
	}

	private void OnGUI()
	{
		if (RTUProcessor.IsConnected)
		{
			if (GUILayout.Button("Disconnect from Game"))
			{
				RTUProcessor.Disconnect();
			}
		}
		else
		{
			if (GUILayout.Button("Connect to Game"))
			{
				RTUProcessor.Connect();
			}
		}

		if (GUILayout.Button("Send Test Data to Game"))
		{
			RTUProcessor.SendMessageToGame("Hello, Game!");
		}
		
		if (GUILayout.Button("Scene Test"))
		{
			RTUScene.Show();
		}

		GUILayout.Label(RTUProcessor.IsConnected ? "Connected" : "Disconnected");
	}
}