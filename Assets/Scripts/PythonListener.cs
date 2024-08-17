using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using System.Threading;

public class PythonListener : MonoBehaviour
{
    Thread thread;
    public int connectionPort = 25001;
    TcpListener server;
    TcpClient client;
    bool running;

    public CommandHandler commandHandler;

    void Start()
    {
        if (commandHandler == null)
        {
            commandHandler = GameObject.FindObjectOfType<CommandHandler>();
        }

        StartListener();
    }

    void StartListener()
    {
        if (thread != null && thread.IsAlive)
        {
            StopListener();
        }

        running = true;
        thread = new Thread(GetData);
        thread.Start();
    }

    void GetData()
    {
        server = new TcpListener(IPAddress.Any, connectionPort);
        server.Start();

        while (running)
        {
            try
            {
                client = server.AcceptTcpClient();
                Thread clientThread = new Thread(new ParameterizedThreadStart(HandleClient));
                clientThread.Start(client);
            }
            catch (SocketException ex)
            {
                Debug.Log("SocketException: " + ex.Message);
                break;
            }
        }

        StopListener(); // Ensure the listener is stopped when exiting the loop
    }

    void HandleClient(object clientObj)
    {
        TcpClient client = (TcpClient)clientObj;
        NetworkStream nwStream = client.GetStream();
        byte[] buffer = new byte[client.ReceiveBufferSize];

        while (running && client.Connected)
        {
            try
            {
                int bytesRead = nwStream.Read(buffer, 0, client.ReceiveBufferSize);
                if (bytesRead > 0)
                {
                    string dataReceived = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();
                    string response = commandHandler.HandleCommand(dataReceived);

                    if (response != null)
                    {
                        byte[] responseBytes = Encoding.UTF8.GetBytes(response);
                        nwStream.Write(responseBytes, 0, responseBytes.Length);
                    }
                }
            }
            catch (SocketException socketEx)
            {
                Debug.Log("SocketException: " + socketEx.Message);
                break;
            }
        }

        client.Close();
    }

    public void StopListener()
    {
        running = false;

        if (client != null)
        {
            client.Close();
            client = null;
        }

        if (server != null)
        {
            server.Stop();
            server = null;
        }

        if (thread != null)
        {
            try
            {
                thread.Abort();  // Forcefully stop the thread
            }
            catch (ThreadAbortException ex)
            {
                Debug.Log("ThreadAbortException: " + ex.Message);
            }
            thread = null;
        }
    }

    void OnApplicationQuit()
    {
        StopListener();
    }

    void OnDisable()
    {
        StopListener();
    }
}
