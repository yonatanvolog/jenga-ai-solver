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

    // Array of Jenga levels to see in the Inspector
    [System.Serializable]
    public class JengaLevel
    {
        public Transform[] pieces = new Transform[3]; // Fixed size array of 3 pieces
    }

    [SerializeField]
    public JengaLevel[] jengaTowerLevels;

    // Dictionary to track the maximum tilt angles for each piece
    private Dictionary<Transform, float> pieceMaxTiltAngles = new Dictionary<Transform, float>();

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
        ResetCubesStartingAngles();
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
        // Start the coroutine that introduces the delay before parsing the tower
        StartCoroutine(ParseJengaTowerWithDelay());
    }

    private IEnumerator ParseJengaTowerWithDelay()
    {
        // Wait until the next physics update
        yield return new WaitForFixedUpdate();

        // Clear the existing array to ensure it's up-to-date
        jengaTowerLevels = new JengaLevel[jengaTower.transform.childCount];

        if (jengaTower == null)
        {
            Debug.LogError("Jenga tower not assigned.");
            yield break;
        }

        // Iterate over the levels in the Jenga tower
        int levelIndex = 0;
        foreach (Transform level in jengaTower.transform)
        {
            JengaLevel jengaLevel = new JengaLevel();

            int pieceIndex = 0;
            // Iterate over the pieces in the current level
            foreach (Transform piece in level)
            {
                // Set the piece in the calculated index
                jengaLevel.pieces[pieceIndex] = piece;
                pieceIndex++;
            }

            // Add the level to the array
            jengaTowerLevels[levelIndex] = jengaLevel;
            levelIndex++;
        }
    }

    /// <summary>
    /// Get the number of blocks in a specific level of the Jenga tower.
    /// This method receives a level (0 is the top) and returns a value from 0 to 3.
    /// The selection is reversed, similar to the PerformMove method.
    /// </summary>
    public int GetNumOfBlocksInLevel(int level)
    {
        if (level < 0 || level >= jengaTowerLevels.Length)
        {
            Debug.LogError("Invalid level index.");
            return 0;
        }

        // Print the number of blocks in all levels
        for (int i = 0; i < jengaTowerLevels.Length; i++)
        {
            int reversedIndex = jengaTowerLevels.Length - 1 - i;
            int numBlocks = jengaTowerLevels[reversedIndex].pieces.Length;
            Debug.Log($"Level {i} (Reversed Index {reversedIndex}): {numBlocks} blocks");
        }

        // Reverse the level selection
        int reversedLevelIndex = jengaTowerLevels.Length - 1 - level;

        if (reversedLevelIndex < 0 || reversedLevelIndex >= jengaTowerLevels.Length)
        {
            Debug.LogError("Invalid reversed level index.");
            return 0;
        }

        int blockCount = 0;
        foreach (var piece in jengaTowerLevels[reversedLevelIndex].pieces)
        {
            if (piece != null) blockCount++;
        }

        return blockCount;
    }

    private void PerformMove()
    {
        // Validate the levelSelect and pieceSelect values
        if (aiPlayer.levelSelect < 0 || aiPlayer.levelSelect >= jengaTowerLevels.Length)
        {
            Debug.LogError("Invalid levelSelect value.");
            return;
        }

        // Reverse the level selection
        int reversedLevelIndex = jengaTowerLevels.Length - 1 - aiPlayer.levelSelect;

        if (reversedLevelIndex < 0 || reversedLevelIndex >= jengaTowerLevels.Length)
        {
            Debug.LogError("Invalid reversed level index.");
            return;
        }

        JengaLevel levelList = jengaTowerLevels[reversedLevelIndex];

        if (aiPlayer.pieceSelect < AIPlayerAPI.PieceType.Yellow || aiPlayer.pieceSelect > AIPlayerAPI.PieceType.Green)
        {
            Debug.LogError("Invalid pieceSelect value.");
            return;
        }

        int pieceIndex = (int)aiPlayer.pieceSelect;
        if (pieceIndex < 0 || pieceIndex >= levelList.pieces.Length || levelList.pieces[pieceIndex] == null)
        {
            Debug.LogError("Invalid pieceSelect index.");
            return;
        }

        Transform pieceTransform = levelList.pieces[pieceIndex];

        if (pieceTransform == null || pieceTransform.gameObject == null)
        {
            Debug.Log("Piece has already been deleted.");
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
        
        // Print the structure of the Jenga tower after the move
        //StartCoroutine(PrintJengaTowerStructure());

        // Update the internal structure after the move
        //ParseJengaTower();

        // Reset the cubes' starting angles
        ResetCubesStartingAngles();
    }
    
    
    private IEnumerator PrintJengaTowerStructure()
    {
        // Wait until the next physics update
        yield return new WaitForFixedUpdate();

        Debug.Log("Jenga Tower Structure After Move:");
        for (int i = 0; i < jengaTowerLevels.Length; i++)
        {
            Debug.Log($"Level {i}:");
            for (int j = 0; j < jengaTowerLevels[i].pieces.Length; j++)
            {
                if (jengaTowerLevels[i].pieces[j] != null)
                {
                    Debug.Log($"  Piece at index {j}: {jengaTowerLevels[i].pieces[j].name}");
                }
                else
                {
                    Debug.Log($"  Empty at index {j}");
                }
            }
        }
    }
    
    /// <summary>
    /// Reset the maximum tilt angles for all cubes in the tower.
    /// </summary>
    private void ResetCubesStartingAngles()
    {
        pieceMaxTiltAngles.Clear();

        foreach (var level in jengaTowerLevels)
        {
            foreach (var piece in level.pieces)
            {
                if (piece != null)
                {
                    // Initialize the dictionary with current tilt angles (absolute values)
                    float initialTiltAngle = Mathf.Max(Mathf.Abs(piece.eulerAngles.x), Mathf.Abs(piece.eulerAngles.z));
                    pieceMaxTiltAngles[piece] = initialTiltAngle;
                }
            }
        }
    }

    /// <summary>
    /// Get the average of the maximum tilt angles recorded for all cubes.
    /// </summary>
    /// <returns>Average of maximum tilt angles</returns>
    public float GetAverageOfMaxAngles()
    {
        if (pieceMaxTiltAngles.Count == 0) return 0f;

        float totalMaxAngle = 0f;

        foreach (var maxAngle in pieceMaxTiltAngles.Values)
        {
            totalMaxAngle += maxAngle;
        }

        return totalMaxAngle / pieceMaxTiltAngles.Count;
    }

    void FixedUpdate()
    {
        // Update the maximum tilt angles for all pieces in the tower
        foreach (var level in jengaTowerLevels)
        {
            foreach (var piece in level.pieces)
            {
                if (piece != null && pieceMaxTiltAngles.ContainsKey(piece))
                {
                    float currentTiltAngle = Mathf.Max(Mathf.Abs(piece.eulerAngles.x), Mathf.Abs(piece.eulerAngles.z));
                    print("tilt angle: " + currentTiltAngle);
                    pieceMaxTiltAngles[piece] = Mathf.Max(pieceMaxTiltAngles[piece], currentTiltAngle);
                }
            }
        }
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
