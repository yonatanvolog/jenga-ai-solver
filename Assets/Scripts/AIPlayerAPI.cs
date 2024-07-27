using System.Collections.Generic;
using UnityEngine;

public class AIPlayerAPI : MonoBehaviour
{
    public enum PieceType
    {
        Yellow,
        Blue,
        Green
    }

    [System.Serializable]
    public class JengaLevel
    {
        public List<Transform> pieces = new List<Transform>();
    }

    [Header("AI Settings")]
    public int levelSelect = 0;
    public PieceType pieceSelect = PieceType.Yellow;
    public bool performMove = false;

    // Reference to the Jenga tower object
    public GameObject jengaTower;

    // List of Jenga levels
    public List<JengaLevel> jengaTowerLevels = new List<JengaLevel>();

    void Start()
    {
        ParseJengaTower();
    }

    void Update()
    {
        if (performMove)
        {
            PerformMove();
            // Reset variables after performing the move
            levelSelect = 0;
            pieceSelect = PieceType.Yellow;
            performMove = false;
        }
    }

    private void ParseJengaTower()
    {
        jengaTowerLevels.Clear();

        if (jengaTower == null)
        {
            Debug.LogError("Jenga tower not assigned.");
            return;
        }

        foreach (Transform level in jengaTower.transform)
        {
            JengaLevel jengaLevel = new JengaLevel();
            foreach (Transform piece in level)
            {
                jengaLevel.pieces.Add(piece);
            }
            jengaTowerLevels.Add(jengaLevel);
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

    private void PerformMove()
    {
        // Validate the levelSelect and pieceSelect values
        if (levelSelect < 0 || levelSelect >= jengaTowerLevels.Count)
        {
            Debug.LogError("Invalid levelSelect value.");
            return;
        }

        // Get the level and piece GameObjects
        JengaLevel levelList = jengaTowerLevels[levelSelect];
        if (pieceSelect < PieceType.Yellow || pieceSelect > PieceType.Green)
        {
            Debug.LogError("Invalid pieceSelect value.");
            return;
        }

        // The index corresponds to the PieceType enum
        int pieceIndex = (int)pieceSelect;
        if (pieceIndex < 0 || pieceIndex >= levelList.pieces.Count)
        {
            Debug.LogError("Invalid pieceSelect index.");
            return;
        }

        Transform pieceTransform = levelList.pieces[pieceIndex];

        // Check if the piece has already been destroyed
        if (pieceTransform == null || pieceTransform.gameObject == null)
        {
            Debug.Log("Piece has already been deleted.");
            return;
        }

        // Perform the move (e.g., destroy the piece)
        Debug.Log($"Performing move: Removing piece {pieceSelect} from level {levelSelect}");
        Destroy(pieceTransform.gameObject);

        // Update the internal structure after the move
        ParseJengaTower();
    }

    // Call this method to set up the AI and prepare for the next move
    public void SetUpAI(int level, PieceType piece, bool shouldPerformMove)
    {
        levelSelect = level;
        pieceSelect = piece;
        performMove = shouldPerformMove;
    }
}
