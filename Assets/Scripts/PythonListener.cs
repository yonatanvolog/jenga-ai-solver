using System;
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
    NetworkStream nwStream;  // Store the network stream for reuse
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
        thread.IsBackground = true;  // Mark thread as background to exit with the application
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
                if (server.Pending())  // Check if there is a pending connection
                {
                    client = server.AcceptTcpClient();
                    nwStream = client.GetStream();  // Save the stream for sending data later
                    Thread clientThread = new Thread(new ParameterizedThreadStart(HandleClient));
                    clientThread.IsBackground = true;  // Ensure this thread exits when the application closes
                    clientThread.Start(client);
                }
                else
                {
                    Thread.Sleep(50);  // Sleep briefly to reduce CPU load when idle
                }
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
        nwStream = client.GetStream();
        byte[] buffer = new byte[client.ReceiveBufferSize];

        while (running && client.Connected)
        {
            try
            {
                if (nwStream.DataAvailable)  // Only read when data is available
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
                    Thread.Sleep(50);  // Sleep briefly when no data is available to reduce CPU usage
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

    // Method to send data to Python
    public void SendData(string message, int maxRetryAttempts = 3, int retryDelayMilliseconds = 500)
    {
        int attempts = 0;

        while (attempts < maxRetryAttempts)
        {
            try
            {
                using (TcpClient sendClient = new TcpClient("127.0.0.1", 25002))  // Connect to port 25002 for sending data
                {
                    NetworkStream sendStream = sendClient.GetStream();
                    if (sendStream.CanWrite)
                    {
                        byte[] dataToSend = Encoding.UTF8.GetBytes(message);
                        sendStream.Write(dataToSend, 0, dataToSend.Length);
                        sendStream.Flush();  // Ensure all data is sent immediately
                        Debug.Log("Message sent to Python: " + message);

                        // Optional: Read the response from Python
                        byte[] responseBuffer = new byte[sendClient.ReceiveBufferSize];
                        int bytesRead = sendStream.Read(responseBuffer, 0, sendClient.ReceiveBufferSize);
                        string response = Encoding.UTF8.GetString(responseBuffer, 0, bytesRead);
                        Debug.Log("Response from Python: " + response);
                    }

                    // Exit if successful
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

                // Wait for a bit before trying again
                Thread.Sleep(retryDelayMilliseconds);
            }
            catch (Exception ex)
            {
                Debug.Log($"Exception: {ex.Message}");
                break;  // For non-socket exceptions, we should not retry
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
                thread.Join(1000);  // Wait for up to 1 second for the thread to finish
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
