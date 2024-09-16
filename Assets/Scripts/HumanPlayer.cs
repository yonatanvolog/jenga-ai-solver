using System.Collections;
using UnityEngine;

public class HumanPlayer : MonoBehaviour
{
    [SerializeField] private Selector selector;

    public IEnumerator HandleTurn()
    {
        selector.enabled = true;
        while (!selector.IsPieceSelected())
        {
            yield return null;
        }
        selector.enabled = false;
    }
}