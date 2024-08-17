using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This class knows what pieces of the jenga tower are in place and selects the piece AiPlayer wants to select (if it exists).
//It saves the structure of the tower, because just like a person would see what piees are in game, this class simulates that.
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

    void Start()
    {
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
        jengaTowerLevels.Clear();

        if (jengaTower == null)
        {
            //Debug.LogError("Jenga tower not assigned.");
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
        //Debug.Log("Jenga Tower Structure:");
        for (int i = 0; i < jengaTowerLevels.Count; i++)
        {
            //Debug.Log($"Level {i}:");
            foreach (var piece in jengaTowerLevels[i].pieces)
            {
                //Debug.Log($"  Piece: {piece.name}");
            }
        }
    }

    private void PerformMove()
    {
        // Validate the levelSelect and pieceSelect values
        if (aiPlayer.levelSelect < 0 || aiPlayer.levelSelect >= jengaTowerLevels.Count)
        {
            //Debug.LogError("Invalid levelSelect value.");
            return;
        }

        JengaLevel levelList = jengaTowerLevels[aiPlayer.levelSelect];
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
        StartCoroutine(HandlePieceMove(pieceTransform));
    }

    private IEnumerator HandlePieceMove(Transform pieceTransform)
    {
        // Change material to highlight
        Renderer renderer = pieceTransform.GetComponent<Renderer>();
        if (renderer != null)
        {
            originalMaterialHighlight = renderer.material;
            renderer.material = highlightMaterial;
        }

        // Wait for highlight time
        if (delayEnabled)
        {
            yield return new WaitForSeconds(highlightTime);
        }

        // Change material to selected
        if (renderer != null)
        {
            originalMaterialSelection = highlightMaterial;
            renderer.material = selectionMaterial;
        }

        // Wait for selected time
        if (delayEnabled)
        {
            yield return new WaitForSeconds(selectedTime);
        }

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
