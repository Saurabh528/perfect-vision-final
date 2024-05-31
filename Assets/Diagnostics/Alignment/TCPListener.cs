using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine.Events;

public class TCPListener : MonoBehaviour
{
	Thread receiveThread;

	// the client connected to the TCP server
	TcpClient client;

	// Unity side
	TcpListener server;
	bool serverUp = false;
	[SerializeField]
	int port = 5066;
	public UnityEvent<string> OnReceive;
	public UnityEvent OnConnected;
	string recvStr;
	bool connected = false;
	// Start is called before the first frame update

    // Update is called once per frame
    void Update()
    {
		if (!string.IsNullOrEmpty(recvStr))
		{
			if(OnReceive != null)
				OnReceive.Invoke(recvStr);
			recvStr = null;
		}
		if (connected)
		{
			if (OnConnected != null)
			{
				OnConnected.Invoke();
				OnConnected = null;
			}
		}
    }

	public void InitTCP()
	{
		try
		{
			// local host
			server = new TcpListener(IPAddress.Parse("127.0.0.1"), port);
			server.Start();

			serverUp = true;

			// create a thread to accept client
			receiveThread = new Thread(new ThreadStart(ReceiveData));
			receiveThread.IsBackground = true;
			receiveThread.Start();

		}
		catch (Exception e)
		{
			// usually error occurs if the port is used by other program.
			// a "SocketException: Address already in use" error will show up here
			print(e.ToString());
		}
	}

	// Stop the TCP server
	// Attach this to the OnClick listener of Stop button on TCP UI panel
	public void StopTCP()
	{
		if (!serverUp) return;
		if (client != null) client.Close();
		if(receiveThread != null)
			receiveThread.Abort();

		server.Stop();

		print("Server is off.");

		if (receiveThread.IsAlive) receiveThread.Abort();

		serverUp = false;
	}

	private void ReceiveData()
	{
		try
		{
			// Buffer
			Byte[] bytes = new Byte[1024];

			while (true)
			{
				print("Waiting for a connection...");

				client = server.AcceptTcpClient();
				print("Connected!");
				connected = true;
				// I/O Stream for sending/ receiving to/ from client
				NetworkStream stream = client.GetStream();

				int length;

				while ((length = stream.Read(bytes, 0, bytes.Length)) != 0)
				{
					var incommingData = new byte[length];
					Array.Copy(bytes, 0, incommingData, 0, length);
					recvStr = Encoding.ASCII.GetString(incommingData);
					Debug.Log("Received: " + recvStr);
					//print("Received message: " + clientMessage);

					// SendData(client);

				}
			}
		}
		catch (Exception e)
		{
			print(e.ToString());
		}
	}

	private void OnDestroy()
	{
		StopTCP();
	}

	public void SendString(string str){
		if(client == null){
			Debug.LogError("Send error: Client is not connected.");
			return;
		}

		byte[] data = Encoding.ASCII.GetBytes(str);
		client.GetStream().Write(data, 0, data.Length);
	}
}
