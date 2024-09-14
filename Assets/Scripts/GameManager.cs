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
    
    
    public int currentPlayerIndex;
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

        if ((PlayerType)playerType == PlayerType.HUMAN)
        {
            // Handle human player's turn
            StartCoroutine(HandleHumanTurn(playerIndex));
        }
        else
        {
            // AI player's turn will be handled by Python via commands
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
            aiSelector.UpdateAfterHumanTurn();
        }

        // End the player's turn
        EndTurn();
    }
    
    public void ToggleMenu()
    {
        mainMenuManager.ToggleMainMenu();
    }

    public void StartTurn(int playerIndex)
    {
        if (isPlayerTurn)
            return;

        currentPlayerIndex = playerIndex;
        isPlayerTurn = true;
        // TODO: remove if condition when finished development of game
        if (humanPlayers.Length > 0 || aiPlayers.Length > 0)
        {
            players[currentPlayerIndex].StartTurn(); // Notify the player to start their turn
        }

        StartCoroutine(WaitForPlayerTurn());
    }

    private IEnumerator WaitForPlayerTurn()
    {
        while (isPlayerTurn)
        {
            yield return null; // Wait for the next frame
        }
        NextTurn();
    }

    private void NextTurn()
    {
        currentPlayerIndex = (currentPlayerIndex + 1) % players.Length;
        StartTurn(currentPlayerIndex); // Start the next player's turn
    }

    public void EndTurn()
    {
        isPlayerTurn = false; // End the current player's turn
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
        Debug.Log("The tower has fallen! Restarting the game...");
        isTowerFallen = true;
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
