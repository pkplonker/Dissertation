using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class RTUController : MonoBehaviour
{
	private TcpListener tcpListener;

	private TcpClient connectedClient;

	private List<string> receveivedData = new();
	private NetworkStream stream;

	[SerializeField]
	private GameObject test;

	private PropertyChangeHandler propertyChangeHandler;

	private readonly Dictionary<string, IRTUCommandHandler> commandHandlers = new()
	{
		{"ping", new PingHandler()},
		{"property", new PropertyChangeHandler()},
	};

	void Start()
	{
		tcpListener = new TcpListener(IPAddress.Any, 5000);
		tcpListener.Start();
		AwaitConnection();
	}

	private void AwaitConnection()
	{
		Debug.Log("Server started. Waiting for connection...");

		tcpListener.BeginAcceptTcpClient(new AsyncCallback(OnClientConnect), null);
	}

	private void OnClientConnect(IAsyncResult ar)
	{
		connectedClient = tcpListener.EndAcceptTcpClient(ar);
		stream = connectedClient.GetStream();
		Debug.Log("Client connected.");
		BeginReadData();
	}

	private void BeginReadData()
	{
		if (connectedClient != null)
		{
			byte[] buffer = new byte[1024];
			connectedClient.GetStream().BeginRead(buffer, 0, buffer.Length, OnDataReceived, buffer);
		}
	}

	private void OnDataReceived(IAsyncResult ar)
	{
		if (connectedClient != null)
		{
			try
			{
				int bytesRead = connectedClient.GetStream().EndRead(ar);
				byte[] buffer = (byte[]) ar.AsyncState;

				string message = Encoding.ASCII.GetString(buffer, 0, bytesRead);
				if (message == string.Empty)
				{
					AwaitConnection();
				}
				else
				{
					Debug.Log("Data received: " + message);
					receveivedData.Add(message);
					HandleMessage(message);

					BeginReadData();
				}
			}
			catch
			{
				AwaitConnection();
			}
			
		}
	}

	private void HandleMessage(string rawMessage)
	{
		var messages = rawMessage.Split("@").Where(x => !string.IsNullOrEmpty(x));
		foreach (var message in messages)
		{
			try
			{
				var index = message.IndexOf(',');
				string messageType = "ping";
				string payload = string.Empty;
				if (index != -1)
				{
					messageType = message.Substring(0, index);
					payload = message.Substring(index + 1);
				}

				if (commandHandlers.TryGetValue(messageType, out var handler))
				{
					handler.Process(stream, payload);
				}
				else
				{
					Debug.Log($"Unknown command: {messageType}");
				}
			}
			catch (Exception e)
			{
				Debug.LogError($"Failed to parse message: {message} {e.Message}");
			}
		}
	}

	void OnApplicationQuit()
	{
		if (connectedClient != null)
		{
			connectedClient.Close();
			connectedClient = null;
		}

		if (tcpListener != null)
		{
			tcpListener.Stop();
			tcpListener = null;
		}
	}
}