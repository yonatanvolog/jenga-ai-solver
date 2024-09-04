using System.Linq;
using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public HumanPlayer[] humanPlayers; // Array of human player references
    public AiPlayer[] aiPlayers; // Array of AI player references
    public GameObject fallDetectorsParent; // Parent GameObject containing fall detectors
    public bool isTowerFallen = false;
    public IPlayer[] players;
    public static GameManager Instance;

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

        StartTurn(0); // Start with player 0
    }

    public void StartTurn(int playerIndex)
    {
        if (isPlayerTurn)
            return;

        currentPlayerIndex = playerIndex;
        isPlayerTurn = true;
        //TODO: remove if condition when finished development of game
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
}
