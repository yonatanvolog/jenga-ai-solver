using System.Linq;
using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public HumanPlayer[] humanPlayers; // Array of human player references
    public AiPlayer[] aiPlayers; // Array of AI player references
    public GameObject fallDetectorsParent; // Parent GameObject containing fall detectors

    public IPlayer[] players;
    public static GameManager Instance;

    public int currentPlayerIndex;
    private bool isPlayerTurn;
    
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
                    fallDetector.OnTowerFall += HandleTowerFall;
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
        players[currentPlayerIndex].StartTurn(); // Notify the player to start their turn

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

    private void HandleTowerFall()
    {
        // Restart the game when the tower falls
        Debug.Log("The tower has fallen! Restarting the game...");
        RestartGame();
    }

    public void RestartGame()
    {
        // Implement your restart logic here
        // For example, reload the scene or reset the game state

        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }
}
