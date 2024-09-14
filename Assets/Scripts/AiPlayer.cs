using System.Collections;
using UnityEngine;

public class AiPlayer : MonoBehaviour, IPlayer
{
    public AiSelector selector;
    /// <summary>
    ///TODO: add AIPlayerAPI aiPlayer referece that will use the TCP listener
    /// </summary>

    public void StartTurn()
    {
        Debug.Log("AI player turn");
        StartCoroutine(HandleTurn());
    }

    private IEnumerator HandleTurn()
    {
        selector.enabled = true;
        while (!selector.IsPieceSelected())
        {
            // TODO: remove AIPlayerAPI aiPlayer reference from AiSelector and  add it here, depending on the choosing
            // of the listener, notify selector here:
            // selector.SetSelectedPiece(0,g), selector already implements choosing by level and color

            yield return null; // Wait for the next frame
        }
        selector.enabled = false;
        EndTurn();
    }

    public void EndTurn()
    {
        //GameManager.Instance.EndTurn(); // Notify the GameManager that the player has finished their turn
    }
}