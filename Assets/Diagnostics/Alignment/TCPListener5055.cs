using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class ScreenDistanceReceiver : MonoBehaviour
{
    private TcpListener tcpListener;
    private Thread tcpListenerThread;
    private TcpClient connectedTcpClient;
    public int port = 5055; // Port number to listen on

    // Use this for initialization
    void Start()
    {
        // Start TcpServer background thread
        tcpListenerThread = new Thread(new ThreadStart(ListenForIncomingRequests));
        tcpListenerThread.IsBackground = true;
        tcpListenerThread.Start();
    }

    // Update is called once per frame
    void Update()
    {
        if (connectedTcpClient == null)
        {
            return;
        }

        // Get a stream object for reading
        NetworkStream stream = connectedTcpClient.GetStream();
        if (stream.DataAvailable)
        {
            byte[] bytes = new byte[connectedTcpClient.ReceiveBufferSize];
            stream.Read(bytes, 0, connectedTcpClient.ReceiveBufferSize);
            string clientMessage = Encoding.ASCII.GetString(bytes);
            Debug.Log("Received message: " + clientMessage);
        }
    }

    private void ListenForIncomingRequests()
    {
        try
        {
            tcpListener = new TcpListener(IPAddress.Any, port);
            tcpListener.Start();
            Debug.Log("Server is listening on port " + port);
            Byte[] bytes = new Byte[1024];
            while (true)
            {
                using (connectedTcpClient = tcpListener.AcceptTcpClient())
                {
                    // Get a stream object for reading
                    using (NetworkStream stream = connectedTcpClient.GetStream())
                    {
                        int length;
                        // Read incomming stream into byte arrary.
                        while ((length = stream.Read(bytes, 0, bytes.Length)) != 0)
                        {
                            var incommingData = new byte[length];
                            Array.Copy(bytes, 0, incommingData, 0, length);
                            // Convert byte array to string message.
                            string clientMessage = Encoding.ASCII.GetString(incommingData);
                            Debug.Log("client message received as: " + clientMessage);
                        }
                    }
                }
            }
        }
        catch (SocketException socketException)
        {
            Debug.Log("SocketException " + socketException.ToString());
        }
    }

    private void OnApplicationQuit()
    {
        // Stop listening thread
        if (tcpListenerThread != null)
        {
            tcpListenerThread.Abort();
        }

        // Close listener
        if (tcpListener != null)
        {
            tcpListener.Stop();
        }
    }
}
