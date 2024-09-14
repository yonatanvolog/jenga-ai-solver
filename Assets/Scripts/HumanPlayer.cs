using System.Collections;
using UnityEngine;

public class HumanPlayer : MonoBehaviour
{
    public Selector selector;
    public CommandDispatcher commandDispatcher;

    private void Start()
    {
        // Find and assign CommandDispatcher
        commandDispatcher = FindObjectOfType<CommandDispatcher>();
        if (commandDispatcher == null)
        {
            Debug.LogError("CommandDispatcher component not found in the scene.");
        }
    }
    
    public IEnumerator HandleTurn()
    {
        selector.enabled = true;
        while (!selector.IsPieceSelected())
        {
            yield return null; // Wait for the next frame
        }
        commandDispatcher.DispatchFinishedMove();
        selector.enabled = false;
    }
}