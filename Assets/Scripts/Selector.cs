using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Selector : MonoBehaviour
{
    public Material highlightMaterial;
    public Material selectionMaterial;
    public float delayBeforeDestroy = 0.5f;
    
    private Material originalMaterialHighlight;
    private Material originalMaterialSelection;
    private Transform highlight;
    private Transform selection;
    private RaycastHit raycastHit;
    private bool pieceSelected;
    private Dictionary<int, string> colorMap;
    public LayerMask selectableLayerMask; // New layer mask for "Selectable" objects
    public CommandDispatcher commandDispatcher;

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

    private void Start()
    {
        // Find and assign CommandDispatcher
        commandDispatcher = FindObjectOfType<CommandDispatcher>();
        if (commandDispatcher == null)
        {
            Debug.LogError("CommandDispatcher component not found in the scene.");
        }
        
        colorMap = new Dictionary<int, string>
        {
            { 0, "y" }, // Map 0 to "y"
            { 1, "b" }, // Map 1 to "b"
            { 2, "g" }  // Map 2 to "g"
        };
    }

    void Update()
    {
        // Highlight
        if (highlight != null)
        {
            highlight.GetComponent<MeshRenderer>().sharedMaterial = originalMaterialHighlight;
            highlight = null;
        }
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        // Using the selectableLayerMask to limit the raycast to the "Selectable" layer
        if (!EventSystem.current.IsPointerOverGameObject() && Physics.Raycast(ray, out raycastHit, Mathf.Infinity, selectableLayerMask))
        {
            highlight = raycastHit.transform;
            if (highlight.CompareTag("Selectable") && highlight != selection)
            {
                if (highlight.GetComponent<MeshRenderer>().material != highlightMaterial)
                {
                    originalMaterialHighlight = highlight.GetComponent<MeshRenderer>().material;
                    highlight.GetComponent<MeshRenderer>().material = highlightMaterial;
                }
            }
            else
            {
                highlight = null;
            }
        }

        // Selection
        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            if (highlight)
            {
                if (selection != null)
                {
                    selection.GetComponent<MeshRenderer>().material = originalMaterialSelection;
                }
                selection = raycastHit.transform;
                if (selection.GetComponent<MeshRenderer>().material != selectionMaterial)
                {
                    originalMaterialSelection = originalMaterialHighlight;
                    selection.GetComponent<MeshRenderer>().material = selectionMaterial;
                }
                highlight = null;

                // Start coroutine to destroy the selected object after a delay
                pieceSelected = true;
                StartCoroutine(DestroySelectedObjectAfterDelay(selection.gameObject, delayBeforeDestroy));
            }
            else
            {
                if (selection)
                {
                    selection.GetComponent<MeshRenderer>().material = originalMaterialSelection;
                    selection = null;
                }
            }
        }
    }

    private IEnumerator DestroySelectedObjectAfterDelay(GameObject selectedObject, float delay)
    {
        string selectedColor;
        int selectedLevel;
        GetSelectedPieceInfo(selectedObject.name, out selectedLevel, out selectedColor);
        commandDispatcher.DispatchFinishedMove(selectedLevel, selectedColor);

        yield return new WaitForSeconds(delay);
        Destroy(selectedObject);
        selection = null; // Clear the selection after destruction
    }

    private void GetSelectedPieceInfo(string selectedObjectName, out int selectedLevel, out string selectedColor)
    {
        // Extract the first character to determine the color
        int pieceIndex = int.Parse(selectedObjectName[0].ToString());
        selectedColor = colorMap[pieceIndex];

        // Extract the first character of the parent name to determine the level
        selectedLevel = int.Parse(selection.parent.name[0].ToString());
    }
}
