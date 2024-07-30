using System.Linq;
using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public HumanPlayer[] humanPlayers; // Array of human player references
    public AiPlayer[] aiPlayers; // Array of AI player references

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
}