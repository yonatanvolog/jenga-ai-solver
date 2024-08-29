using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AiSelector : MonoBehaviour
{
    public Material highlightMaterial;
    public Material selectionMaterial;
    public float highlightTime = 1.0f; // Time to show highlight material
    public float selectedTime = 1.0f;  // Time to show selected material before destruction
    public bool delayEnabled = true; // Field to enable/disable delay

    public AIPlayerAPI aiPlayer;

    private Material originalMaterialHighlight;
    private Material originalMaterialSelection;
    private bool pieceSelected;

    // Reference to the Jenga tower object
    public GameObject jengaTower;

    // List of Jenga levels
    public List<JengaLevel> jengaTowerLevels = new List<JengaLevel>();

    // Reference to the Screenshot component
    private Screenshot screenshot;

    void Start()
    {
        // Find the Screenshot object in the scene
        screenshot = FindObjectOfType<Screenshot>();

        if (screenshot == null)
        {
            Debug.LogError("Screenshot component not found in the scene.");
        }

        ParseJengaTower();
    }

    public class JengaLevel
    {
        public List<Transform> pieces = new List<Transform>();
    }

    private void OnEnable()
    {
        pieceSelected = false;
    }
    
    private void OnDisable()
    {
        pieceSelected = false;
    }

    public bool IsPieceSelected()
    {
        return pieceSelected;
    }

    private void ParseJengaTower()
    {
        // Ensure the list is properly cleared
        jengaTowerLevels.Clear();
    }

    private void ParseJengaTower111()
    {
        // Ensure the list is properly cleared
        jengaTowerLevels.Clear();

        if (jengaTower == null)
        {
            Debug.LogError("Jenga tower not assigned.");
            return;
        }

        // Iterate over the levels in the Jenga tower
        foreach (Transform level in jengaTower.transform)
        {
            JengaLevel jengaLevel = new JengaLevel();

            // Iterate over the pieces in the current level
            foreach (Transform piece in level)
            {
                // Check if the piece is still active and not destroyed
                if (piece != null && piece.gameObject != null && piece.gameObject.activeInHierarchy)
                {
                    jengaLevel.pieces.Add(piece);
                }
            }

            // Only add levels that contain active pieces
            if (jengaLevel.pieces.Count > 0)
            {
                jengaTowerLevels.Add(jengaLevel);
            }
        }

        // Print the structure for debugging
        Debug.Log("Jenga Tower Structure:");
        for (int i = 0; i < jengaTowerLevels.Count; i++)
        {
            Debug.Log($"Level {i}:");
            foreach (var piece in jengaTowerLevels[i].pieces)
            {
                Debug.Log($"  Piece: {piece.name}");
            }
        }
    }



    /// <summary>
    /// Get the number of blocks in a specific level of the Jenga tower.
    /// This method receives a level (0 is the top) and returns a value from 0 to 3.
    /// The selection is reversed, similar to the PerformMove method.
    /// </summary>
    public int GetNumOfBlocksInLevel(int level)
    {
        if (level < 0 || level >= jengaTowerLevels.Count)
        {
            Debug.LogError("Invalid level index.");
            return 0;
        }

        // Print the number of blocks in all levels
        for (int i = 0; i < jengaTowerLevels.Count; i++)
        {
            int reversedIndex = jengaTowerLevels.Count - 1 - i;
            int numBlocks = jengaTowerLevels[reversedIndex].pieces.Count;
            Debug.Log($"Level {i} (Reversed Index {reversedIndex}): {numBlocks} blocks");
        }

        // Reverse the level selection
        int reversedLevelIndex = jengaTowerLevels.Count - 1 - level;

        if (reversedLevelIndex < 0 || reversedLevelIndex >= jengaTowerLevels.Count)
        {
            Debug.LogError("Invalid reversed level index.");
            return 0;
        }

        return jengaTowerLevels[reversedLevelIndex].pieces.Count;
    }


    private void PerformMove()
    {
        // Validate the levelSelect and pieceSelect values
        if (aiPlayer.levelSelect < 0 || aiPlayer.levelSelect >= jengaTowerLevels.Count)
        {
            //Debug.LogError("Invalid levelSelect value.");
            return;
        }

        // Reverse the level selection
        int reversedLevelIndex = jengaTowerLevels.Count - 1 - aiPlayer.levelSelect;

        if (reversedLevelIndex < 0 || reversedLevelIndex >= jengaTowerLevels.Count)
        {
            //Debug.LogError("Invalid reversed level index.");
            return;
        }

        JengaLevel levelList = jengaTowerLevels[reversedLevelIndex];

        if (aiPlayer.pieceSelect < AIPlayerAPI.PieceType.Yellow || aiPlayer.pieceSelect > AIPlayerAPI.PieceType.Green)
        {
            //Debug.LogError("Invalid pieceSelect value.");
            return;
        }

        int pieceIndex = (int)aiPlayer.pieceSelect;
        if (pieceIndex < 0 || pieceIndex >= levelList.pieces.Count)
        {
            //Debug.LogError("Invalid pieceSelect index.");
            return;
        }

        Transform pieceTransform = levelList.pieces[pieceIndex];

        if (pieceTransform == null || pieceTransform.gameObject == null)
        {
            //Debug.Log("Piece has already been deleted.");
            return;
        }

        // Perform the move (e.g., change material and destroy the piece)
        pieceSelected = true;
        HandlePieceMoveQuick(pieceTransform);
        StartCoroutine(TakeScreenshotAfterFrame());
    }

    private IEnumerator TakeScreenshotAfterFrame()
    {
        // Wait until the end of the frame before taking the screenshot
        yield return new WaitForEndOfFrame();

        // Call TakeScreenshot after the frame has been rendered
        if (screenshot != null)
        {
            screenshot.TakeScreenshot();
        }
    }

    private void HandlePieceMoveQuick(Transform pieceTransform)
    {
        // Destroy the piece
        Destroy(pieceTransform.gameObject);

        // Update the internal structure after the move
        ParseJengaTower();
    }

    void Update()
    {
        if (!aiPlayer.performMove)
        {
            return;
        }

        PerformMove();
        aiPlayer.levelSelect = 0;
        aiPlayer.pieceSelect = AIPlayerAPI.PieceType.Yellow;
        aiPlayer.performMove = false;
    }
}
