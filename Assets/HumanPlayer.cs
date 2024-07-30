using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanPlayer : MonoBehaviour
{
    public Selector selector;

    public void PerformMove()
    {
        selector.enabled = true;
        // while (!selector.IsPieceSelected())
        // {
        // }
        selector.enabled = false;
    }
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
