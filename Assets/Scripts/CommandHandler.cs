using UnityEngine;
using System.Threading;

public class CommandHandler : MonoBehaviour
{
    private enum CommandType
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
        GetMostMaxTiltAngle,
        PlayerTurn,
        RevertStep,
        ToggleMenu,
        Unknown
    }

    [SerializeField] private AIPlayerAPI aiPlayerAPI;
    [SerializeField] private PhysicMaterial jengaPhysicsMaterial;
    private GameManager gameManager;
    private Screenshot screenshot;
    private FallDetectModifier fallDetectModifier;
    private AiSelector aiSelector;
    private SynchronizationContext mainThreadContext;

    void Start()
    {
        if (aiPlayerAPI == null)
        {
            aiPlayerAPI = FindObjectOfType<AIPlayerAPI>();
        }

        gameManager = FindObjectOfType<GameManager>();
        if (gameManager == null)
        {
            Debug.LogError("GameManager not found in the scene.");
        }

        screenshot = FindObjectOfType<Screenshot>();
        if (screenshot == null)
        {
            Debug.LogError("Screenshot component not found in the scene.");
        }

        fallDetectModifier = FindObjectOfType<FallDetectModifier>();
        if (fallDetectModifier == null)
        {
            Debug.LogError("FallDetectModifier component not found in the scene.");
        }

        aiSelector = FindObjectOfType<AiSelector>();
        if (aiSelector == null)
        {
            Debug.LogError("AiSelector component not found in the scene.");
        }

        mainThreadContext = SynchronizationContext.Current;
    }

    public string HandleCommand(string data)
    {
        CommandType commandType = ParseCommand(data);
        string response = "ACK";

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
            case CommandType.GetMostMaxTiltAngle:
                mainThreadContext.Send(_ => response = HandleGetMostMaxTiltAngle(), null);
                break;
            case CommandType.PlayerTurn:
                HandlePlayerTurnCommand(data);
                break;
            case CommandType.RevertStep:
                mainThreadContext.Send(_ => response = HandleRevertStep(), null); 
                break;
            case CommandType.ToggleMenu:
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
        if (data.StartsWith("remove")) return CommandType.Remove;
        if (data.Equals("reset")) return CommandType.Reset;
        if (data.StartsWith("timescale")) return CommandType.Timescale;
        if (data.StartsWith("staticfriction")) return CommandType.SetStaticFriction;
        if (data.StartsWith("dynamicfriction")) return CommandType.SetDynamicFriction;
        if (data.StartsWith("set_screenshot_res")) return CommandType.SetScreenshotRes;
        if (data.Equals("isfallen")) return CommandType.IsFallen;
        if (data.StartsWith("set_fall_detect_distance")) return CommandType.SetFallDetectDistance;
        if (data.StartsWith("get_num_of_blocks_in_level")) return CommandType.GetNumOfBlocksInLevel;
        if (data.StartsWith("get_average_max_tilt_angle")) return CommandType.GetAverageMaxTiltAngle;
        if (data.StartsWith("get_most_max_tilt_angle")) return CommandType.GetMostMaxTiltAngle;
        if (data.StartsWith("player_turn")) return CommandType.PlayerTurn;
        if (data.StartsWith("revert_step")) return CommandType.RevertStep;
        if (data.Equals("toggle_menu")) return CommandType.ToggleMenu;

        return CommandType.Unknown;
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

    private string HandleGetMostMaxTiltAngle()
    {
        float maxTiltAngle = aiSelector != null ? aiSelector.GetMaxOfMaxAngles() : 0f;
        Debug.Log($"Most max tilt angle: {maxTiltAngle}");
        return maxTiltAngle.ToString();
    }

    private void HandlePlayerTurnCommand(string data)
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

    private void HandleToggleMenuCommand()
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
