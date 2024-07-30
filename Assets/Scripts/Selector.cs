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

    void Update()
    {
        // Highlight
        if (highlight != null)
        {
            highlight.GetComponent<MeshRenderer>().sharedMaterial = originalMaterialHighlight;
            highlight = null;
        }
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (!EventSystem.current.IsPointerOverGameObject() && Physics.Raycast(ray, out raycastHit)) //Make sure you have EventSystem in the hierarchy before using EventSystem
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

                // Start coroutine to destroy the selected object after 1 second
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
        yield return new WaitForSeconds(delay);
        Destroy(selectedObject);
        selection = null; // Clear the selection after destruction
    }
}
