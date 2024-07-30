using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public Player[] players; // Array of player references

    public static GameManager Instance;

    private int currentPlayerIndex;
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