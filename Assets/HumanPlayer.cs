using System.Collections;
using UnityEngine;

public class HumanPlayer : MonoBehaviour, IPlayer
{
    public Selector selector;

    public void StartTurn()
    {
        Debug.Log("Human player turn");
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
        Debug.Log("Human ended turn");

        GameManager.Instance.EndTurn(); // Notify the GameManager that the player has finished their turn
    }
}