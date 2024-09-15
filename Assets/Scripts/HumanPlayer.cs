using System.Collections;
using UnityEngine;

public class HumanPlayer : MonoBehaviour
{
    public Selector selector;
    
    public IEnumerator HandleTurn()
    {
        selector.enabled = true;
        while (!selector.IsPieceSelected())
        {
            yield return null; // Wait for the next frame
        }
        selector.enabled = false;
    }
}