using System.Collections;
using UnityEngine;

public class Player : MonoBehaviour, IPlayer
{
    public Selector selector;

    public void StartTurn()
    {
        StartCoroutine(HandleTurn());
    }

    private IEnumerator HandleTurn()
    {
        selector.enabled = true;
        while (!selector.IsPieceSelected())
        {
            yield return null; // Wait for the next frame
        }
        selector.enabled = false;
        EndTurn();
    }

    public void EndTurn()
    {
        GameManager.Instance.EndTurn(); // Notify the GameManager that the player has finished their turn
    }
}