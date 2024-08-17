using UnityEngine;
using System.Threading;

public class CommandHandler : MonoBehaviour
{
    public enum CommandType
    {
        Remove,
        Reset,
        Timescale,
        IsFallen,
        Unknown
    }

    public AIPlayerAPI aiPlayerAPI;
    private GameManager gameManager;
    private SynchronizationContext mainThreadContext;

    void Start()
    {
        if (aiPlayerAPI == null)
        {
            aiPlayerAPI = GameObject.FindObjectOfType<AIPlayerAPI>();
        }

        gameManager = GameObject.FindObjectOfType<GameManager>();
        if (gameManager == null)
        {
            Debug.LogError("GameManager not found in the scene.");
        }

        // Capture the synchronization context of the main thread
        mainThreadContext = SynchronizationContext.Current;
    }

    public string HandleCommand(string data)
    {
        CommandType commandType = ParseCommand(data);

        switch (commandType)
        {
            case CommandType.Remove:
                HandleRemoveCommand(data);
                return "ok, play again";
            case CommandType.Reset:
                HandleResetCommand();
                return "ok, play again";
            case CommandType.Timescale:
                HandleTimescale(data);
                return "ok, play again";
            case CommandType.IsFallen:
                return HandleIsFallenCommand();
            case CommandType.Unknown:
                Debug.Log("Unknown command received.");
                return "Unknown command";
        }
        return null;
    }

    private CommandType ParseCommand(string data)
    {
        if (data.StartsWith("remove"))
        {
            return CommandType.Remove;
        }
        else if (data.Equals("reset"))
        {
            return CommandType.Reset;
        }
        else if (data.StartsWith("timescale"))
        {
            return CommandType.Timescale;
        }
        else if (data.Equals("isfallen"))
        {
            return CommandType.IsFallen;
        }
        else
        {
            return CommandType.Unknown;
        }
    }

    private void HandleRemoveCommand(string data)
    {
        string[] parts = data.Split(' ');
        if (parts.Length == 3 && int.TryParse(parts[1], out int level))
        {
            aiPlayerAPI.levelSelect = level;

            switch (parts[2].ToLower())
            {
                case "y":
                    aiPlayerAPI.pieceSelect = AIPlayerAPI.PieceType.Yellow;
                    break;
                case "b":
                    aiPlayerAPI.pieceSelect = AIPlayerAPI.PieceType.Blue;
                    break;
                case "g":
                    aiPlayerAPI.pieceSelect = AIPlayerAPI.PieceType.Green;
                    break;
            }

            aiPlayerAPI.performMove = true;
        }
    }

    private void HandleResetCommand()
    {
        Debug.Log("Reset command received.");

        if (gameManager != null)
        {
            // Post the RestartGame call to the main thread
            mainThreadContext.Post(_ => gameManager.RestartGame(), null);
        }
        else
        {
            Debug.LogError("GameManager is not available to reset the game.");
        }
    }

    private void HandleTimescale(string data)
    {
        string[] parts = data.Split(' ');
        if (parts.Length == 2 && float.TryParse(parts[1], out float timescale))
        {
            Debug.Log($"Setting timescale to {timescale}");

            // Post the timescale adjustment to the main thread
            mainThreadContext.Post(_ => Time.timeScale = timescale, null);
        }
        else
        {
            Debug.LogError("Invalid timescale value received.");
        }
    }

    private string HandleIsFallenCommand()
    {
        if (gameManager != null)
        {
            // Check if the tower has fallen and return the result
            bool isFallen = gameManager.IsTowerFallen();
            return isFallen.ToString().ToLower();
        }
        else
        {
            Debug.LogError("GameManager is not available to check if the tower has fallen.");
            return "false";
        }
    }
}
