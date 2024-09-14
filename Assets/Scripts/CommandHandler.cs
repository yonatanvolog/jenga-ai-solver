using UnityEngine;
using System.Threading;

public class CommandHandler : MonoBehaviour
{
    public enum CommandType
    {
        Remove,
        Reset,
        Timescale,
        SetStaticFriction,
        SetDynamicFriction,
        SetScreenshotRes,
        IsFallen,
        SetFallDetectDistance,
        GetNumOfBlocksInLevel,
        GetAverageMaxTiltAngle,
        GetMostMaxTiltAngle, // Added new command for max tilt angle
        PlayerTurn, // Added new command for handling player turn
        RevertStep,
        ToggleMenu, // Added new command for toggling menu
        Unknown
    }

    public AIPlayerAPI aiPlayerAPI;
    public PhysicMaterial jengaPhysicsMaterial;  // Assign this in the Inspector or via script
    private GameManager gameManager;
    private Screenshot screenshot;  // Field to store the Screenshot component
    private FallDetectModifier fallDetectModifier; // Field to store the FallDetectModifier component
    private AiSelector aiSelector; // Field to store the AiSelector component
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

        screenshot = GameObject.FindObjectOfType<Screenshot>();
        if (screenshot == null)
        {
            Debug.LogError("Screenshot component not found in the scene.");
        }

        fallDetectModifier = GameObject.FindObjectOfType<FallDetectModifier>();
        if (fallDetectModifier == null)
        {
            Debug.LogError("FallDetectModifier component not found in the scene.");
        }

        aiSelector = GameObject.FindObjectOfType<AiSelector>();
        if (aiSelector == null)
        {
            Debug.LogError("AiSelector component not found in the scene.");
        }

        // Capture the synchronization context of the main thread
        mainThreadContext = SynchronizationContext.Current;
    }

    public string HandleCommand(string data)
    {
        CommandType commandType = ParseCommand(data);
        string response = "ACK"; // Default response

        switch (commandType)
        {
            case CommandType.Remove:
                HandleRemoveCommand(data);
                break;
            case CommandType.Reset:
                HandleResetCommand();
                break;
            case CommandType.Timescale:
                HandleTimescale(data);
                break;
            case CommandType.SetStaticFriction:
                HandleSetStaticFriction(data);
                break;
            case CommandType.SetDynamicFriction:
                HandleSetDynamicFriction(data);
                break;
            case CommandType.SetScreenshotRes:
                HandleSetScreenshotRes(data);
                break;
            case CommandType.IsFallen:
                response = HandleIsFallenCommand();
                break;
            case CommandType.SetFallDetectDistance:
                HandleSetFallDetectDistance(data);
                break;
            case CommandType.GetNumOfBlocksInLevel:
                mainThreadContext.Send(_ => response = HandleGetNumOfBlocksInLevel(data), null);
                break;
            case CommandType.GetAverageMaxTiltAngle:
                mainThreadContext.Send(_ => response = HandleGetAverageMaxTiltAngle(), null);
                break;
            case CommandType.GetMostMaxTiltAngle: // New case for max tilt angle
                mainThreadContext.Send(_ => response = HandleGetMostMaxTiltAngle(), null);
                break;
            case CommandType.PlayerTurn: // Added new case for handling player turn
                HandlePlayerTurnCommand(data);
                break;
            case CommandType.RevertStep:
                mainThreadContext.Send(_ => response = HandleRevertStep(), null); 
                break;
            case CommandType.ToggleMenu: // Added case for toggling menu
                HandleToggleMenuCommand();
                break;
            case CommandType.Unknown:
                Debug.Log("Unknown command received.");
                response = "Unknown command";
                break;
        }

        return response;
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
        else if (data.StartsWith("staticfriction"))
        {
            return CommandType.SetStaticFriction;
        }
        else if (data.StartsWith("dynamicfriction"))
        {
            return CommandType.SetDynamicFriction;
        }
        else if (data.StartsWith("set_screenshot_res"))
        {
            return CommandType.SetScreenshotRes;
        }
        else if (data.Equals("isfallen"))
        {
            return CommandType.IsFallen;
        }
        else if (data.StartsWith("set_fall_detect_distance"))
        {
            return CommandType.SetFallDetectDistance;
        }
        else if (data.StartsWith("get_num_of_blocks_in_level"))
        {
            return CommandType.GetNumOfBlocksInLevel;
        }
        else if (data.StartsWith("get_average_max_tilt_angle"))
        {
            return CommandType.GetAverageMaxTiltAngle;
        }
        else if (data.StartsWith("get_most_max_tilt_angle")) // New parsing for max tilt angle
        {
            return CommandType.GetMostMaxTiltAngle;
        }
        else if (data.StartsWith("player_turn")) // Parsing for the player turn command
        {
            return CommandType.PlayerTurn;
        }
        else if (data.StartsWith("revert_step"))
        {
            return CommandType.RevertStep;
        }
        else if (data.Equals("toggle_menu")) // Parsing for the toggle_menu command
        {
            return CommandType.ToggleMenu;
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
            mainThreadContext.Post(_ => Time.timeScale = timescale, null);
        }
        else
        {
            Debug.LogError("Invalid timescale value received.");
        }
    }

    private void HandleSetStaticFriction(string data)
    {
        string[] parts = data.Split(' ');
        if (parts.Length == 2 && float.TryParse(parts[1], out float staticFriction))
        {
            Debug.Log($"Setting static friction to {staticFriction}");
            mainThreadContext.Post(_ => jengaPhysicsMaterial.staticFriction = staticFriction, null);
        }
        else
        {
            Debug.LogError("Invalid static friction value received.");
        }
    }

    private void HandleSetDynamicFriction(string data)
    {
        string[] parts = data.Split(' ');
        if (parts.Length == 2 && float.TryParse(parts[1], out float dynamicFriction))
        {
            Debug.Log($"Setting dynamic friction to {dynamicFriction}");
            mainThreadContext.Post(_ => jengaPhysicsMaterial.dynamicFriction = dynamicFriction, null);
        }
        else
        {
            Debug.LogError("Invalid dynamic friction value received.");
        }
    }

    private void HandleSetScreenshotRes(string data)
    {
        string[] parts = data.Split(' ');
        if (parts.Length == 2 && int.TryParse(parts[1], out int width))
        {
            Debug.Log($"Setting screenshot resolution width to {width}");
            mainThreadContext.Post(_ => screenshot.SetFinalWidth(width), null);
        }
        else
        {
            Debug.LogError("Invalid screenshot width value received.");
        }
    }

    private string HandleIsFallenCommand()
    {
        bool hasFallen = gameManager != null && gameManager.IsTowerFallen();
        return hasFallen.ToString().ToLower();
    }

    private void HandleSetFallDetectDistance(string data)
    {
        string[] parts = data.Split(' ');
        if (parts.Length == 2 && float.TryParse(parts[1], out float value))
        {
            Debug.Log($"Setting fall detect distance to {value}");
            mainThreadContext.Post(_ => fallDetectModifier.SetDistance(value), null);
        }
        else
        {
            Debug.LogError("Invalid distance value received.");
        }
    }

    private string HandleGetNumOfBlocksInLevel(string data)
    {
        string[] parts = data.Split(' ');
        if (parts.Length == 2 && int.TryParse(parts[1], out int level))
        {
            int numOfBlocks = aiSelector != null ? aiSelector.GetNumOfBlocksInLevel(level) : 0;
            Debug.Log($"Number of blocks in level {level}: {numOfBlocks}");
            return numOfBlocks.ToString();
        }
        else
        {
            Debug.LogError("Invalid level value received.");
            return "Invalid level value";
        }
    }

    private string HandleGetAverageMaxTiltAngle()
    {
        float averageTiltAngle = aiSelector != null ? aiSelector.GetAverageOfMaxAngles() : 0f;
        Debug.Log($"Average max tilt angle: {averageTiltAngle}");
        return averageTiltAngle.ToString();
    }

    private string HandleGetMostMaxTiltAngle() // Handler for most max tilt angle
    {
        float maxTiltAngle = aiSelector != null ? aiSelector.GetMaxOfMaxAngles() : 0f;
        Debug.Log($"Most max tilt angle: {maxTiltAngle}");
        return maxTiltAngle.ToString();
    }

    private void HandlePlayerTurnCommand(string data) // New handler for player turn command
    {
        string[] parts = data.Split(' ');
        if (parts.Length == 4 && int.TryParse(parts[1], out int playerType) &&
            int.TryParse(parts[2], out int playerIndex) && int.TryParse(parts[3], out int roundNumber))
        {
            Debug.Log($"Handling player turn: PlayerType={playerType}, PlayerIndex={playerIndex}, Round={roundNumber}");
            mainThreadContext.Post(_ => gameManager.PlayerTurn(playerType, playerIndex, roundNumber), null);
        }
        else
        {
            Debug.LogError("Invalid player_turn command format or values.");
        }
    }

    private void HandleToggleMenuCommand() // Handler for toggle_menu command
    {
        if (gameManager != null)
        {
            Debug.Log("Toggling menu.");
            mainThreadContext.Post(_ => gameManager.ToggleMenu(), null);
        }
        else
        {
            Debug.LogError("GameManager is not available to toggle the menu.");
        }
    }

    private string HandleRevertStep() 
    {
        if (aiSelector != null)
        {
            aiSelector.RevertStep();
            return "Step reverted.";
        }
        else
        {
            return "AiSelector component not found.";
        }
    }
}
