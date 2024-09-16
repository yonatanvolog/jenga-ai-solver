using System.Linq;
using UnityEngine;
using System.Collections;
using TMPro;
using Unity.VisualScripting.Antlr3.Runtime;

public class GameManager : MonoBehaviour
{
    public HumanPlayer humanPlayer;
    public HumanPlayer[] humanPlayers; // Array of human player references
    public AiPlayer[] aiPlayers; // Array of AI player references
    public GameObject fallDetectorsParent; // Parent GameObject containing fall detectors
    public bool isTowerFallen = false;
    public IPlayer[] players;
    public static GameManager Instance;
    public AiSelector aiSelector; //CHAT GPT: find this at start and assign
    public TMP_Text playerTurnDisplay;
    public CommandDispatcher commandDispatcher;
    public MainMenuManager mainMenuManager;

    public PlayerType currentPlayerType;
    public int currentPlayerIndex;
    public int currentRoundNum;
    public CameraController cameraController;
    private bool isPlayerTurn;

    // Private field to store the GameManager instance
    private GameManager gameManager;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Save a reference to the GameManager instance
        gameManager = GameManager.Instance;

        // Combine humanPlayers and aiPlayers into players array
        players = humanPlayers.Cast<IPlayer>()
            .Concat(aiPlayers.Cast<IPlayer>())
            .ToArray();

        // Find and assign AiSelector
        aiSelector = FindObjectOfType<AiSelector>();
        if (aiSelector == null)
        {
            Debug.LogError("AiSelector component not found in the scene.");
        }
        
        // Find and assign cameraController
        cameraController = FindObjectOfType<CameraController>();
        if (cameraController == null)
        {
            Debug.LogError("CameraController component not found in the scene.");
        }
        
        // Find and assign CommandDispatcher
        commandDispatcher = FindObjectOfType<CommandDispatcher>();
        if (commandDispatcher == null)
        {
            Debug.LogError("CommandDispatcher component not found in the scene.");
        }

        // Set up fall detectors from the children of fallDetectorsParent
        if (fallDetectorsParent != null)
        {
            foreach (Transform detector in fallDetectorsParent.transform)
            {
                var fallDetector = detector.GetComponent<FallDetector>();
                if (fallDetector != null)
                {
                    fallDetector.OnTowerFall += () => HandleTowerFall(fallDetector);
                }
            }
        }

        //StartTurn(0); // Start with player 0
    }

    // playerType will be converted to PlayerType enum, player index is needed when 2 humans play against each other.
    public void PlayerTurn(int playerType, int playerIndex, int roundNum)
    {
        // Update player turn display
        playerTurnDisplay.text = $"Now playing PlayerType: {((PlayerType)playerType)}, PlayerIndex: {playerIndex + 1}, Round: {roundNum}";
        currentPlayerType = (PlayerType)playerType;
        currentPlayerIndex = playerIndex;
        currentRoundNum = roundNum;

        if ((PlayerType)playerType == PlayerType.HUMAN)
        {
            // Handle human player's turn
            StartCoroutine(HandleHumanTurn(playerIndex));
        }
        else
        {
            // AI player's turn will be handled by Python via commands, Start returns camera to default position
            cameraController.Start();
            Debug.Log("AI Player turn. Waiting for Python command to remove a piece.");
        }
    }

    private IEnumerator HandleHumanTurn(int playerIndex)
    {
        // Wait for the human player to take their turn
        yield return StartCoroutine(humanPlayer.HandleTurn());

        // Update AI selector after the human's turn is completed
        if (aiSelector != null)
        {
            yield return new WaitForSeconds(0.25f);
            aiSelector.UpdateAfterHumanTurn();
        }
    }
    
    public void ToggleMenu()
    {
        mainMenuManager.ToggleMainMenu();
    }

    private void HandleTowerFall(FallDetector fallDetector)
    {
        // Enable the MeshRenderer of the fall detector
        MeshRenderer meshRenderer = fallDetector.GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            meshRenderer.enabled = true;
        }

        // Mark the tower as fallen and handle the event
        Debug.Log("The tower has fallen!");
        isTowerFallen = true;
        playerTurnDisplay.text = $"Player Collaped the tower: PlayerType: {((PlayerType)currentPlayerType)}, PlayerIndex: {currentPlayerIndex + 1}, Round: {currentRoundNum}";
        //gameManager.RestartGame();
    }

    public bool IsTowerFallen()
    {
        return isTowerFallen;
    }

    public void RestartGame()
    {
        // Find the PythonListener and stop it
        PythonListener pythonListener = FindObjectOfType<PythonListener>();
        if (pythonListener != null)
        {
            pythonListener.StopListener();
        }

        // Reload the current scene
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }

    public void ResetIsTowerFallen()
    {
        isTowerFallen = false;
    }
}
