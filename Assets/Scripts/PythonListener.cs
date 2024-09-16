using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using System.Threading;

public class PythonListener : MonoBehaviour
{
    private Thread thread;
    [SerializeField] private int connectionPort = 25001;
    private TcpListener server;
    private TcpClient client;
    private NetworkStream nwStream;
    private bool running;

    [SerializeField] private CommandHandler commandHandler;

    void Start()
    {
        if (commandHandler == null)
        {
            commandHandler = FindObjectOfType<CommandHandler>();
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
        thread = new Thread(GetData)
        {
            IsBackground = true
        };
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
                if (server.Pending())
                {
                    client = server.AcceptTcpClient();
                    nwStream = client.GetStream();
                    Thread clientThread = new Thread(HandleClient)
                    {
                        IsBackground = true
                    };
                    clientThread.Start(client);
                }
                else
                {
                    Thread.Sleep(50);
                }
            }
            catch (SocketException ex)
            {
                Debug.Log("SocketException: " + ex.Message);
                break;
            }
        }

        StopListener();
    }

    void HandleClient(object clientObj)
    {
        TcpClient client = (TcpClient)clientObj;
        nwStream = client.GetStream();
        byte[] buffer = new byte[client.ReceiveBufferSize];

        while (running && client.Connected)
        {
            try
            {
                if (nwStream.DataAvailable)
                {
                    int bytesRead = nwStream.Read(buffer, 0, client.ReceiveBufferSize);
                    if (bytesRead > 0)
                    {
                        string dataReceived = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();
                        string response = commandHandler.HandleCommand(dataReceived);

                        if (!string.IsNullOrEmpty(response))
                        {
                            byte[] responseBytes = Encoding.UTF8.GetBytes(response);
                            nwStream.Write(responseBytes, 0, responseBytes.Length);
                        }
                    }
                }
                else
                {
                    Thread.Sleep(50);
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

    public void SendData(string message, int maxRetryAttempts = 3, int retryDelayMilliseconds = 500)
    {
        int attempts = 0;

        while (attempts < maxRetryAttempts)
        {
            try
            {
                using (TcpClient sendClient = new TcpClient("127.0.0.1", 25002))
                {
                    NetworkStream sendStream = sendClient.GetStream();
                    if (sendStream.CanWrite)
                    {
                        byte[] dataToSend = Encoding.UTF8.GetBytes(message);
                        sendStream.Write(dataToSend, 0, dataToSend.Length);
                        sendStream.Flush();
                        Debug.Log("Message sent to Python: " + message);

                        byte[] responseBuffer = new byte[sendClient.ReceiveBufferSize];
                        int bytesRead = sendStream.Read(responseBuffer, 0, sendClient.ReceiveBufferSize);
                        string response = Encoding.UTF8.GetString(responseBuffer, 0, bytesRead);
                        Debug.Log("Response from Python: " + response);
                    }

                    break;
                }
            }
            catch (SocketException socketEx)
            {
                attempts++;
                Debug.Log($"SocketException: {socketEx.Message}. Attempt {attempts} of {maxRetryAttempts}");
                if (attempts >= maxRetryAttempts)
                {
                    Debug.Log("Max retry attempts reached. Failed to send data.");
                    break;
                }

                Thread.Sleep(retryDelayMilliseconds);
            }
            catch (Exception ex)
            {
                Debug.Log($"Exception: {ex.Message}");
                break;
            }
        }
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

        if (thread != null && thread.IsAlive)
        {
            try
            {
                thread.Join(1000);
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
