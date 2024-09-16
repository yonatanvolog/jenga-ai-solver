using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Selector : MonoBehaviour
{
    [SerializeField] private Material highlightMaterial;
    [SerializeField] private Material selectionMaterial;
    [SerializeField] private float delayBeforeDestroy = 0.5f;
    [SerializeField] private LayerMask selectableLayerMask;
    [SerializeField] private CommandDispatcher commandDispatcher;

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

    private void Start()
    {
        if (commandDispatcher == null)
        {
            commandDispatcher = FindObjectOfType<CommandDispatcher>();
        }
    }

    void Update()
    {
        if (highlight != null)
        {
            highlight.GetComponent<MeshRenderer>().sharedMaterial = originalMaterialHighlight;
            highlight = null;
        }

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

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
        GetSelectedPieceInfo(selectedObject.name, out int selectedLevel, out string selectedColor);
        commandDispatcher.DispatchFinishedMove(selectedLevel, selectedColor);

        yield return new WaitForSeconds(delay);
        Destroy(selectedObject);
        selection = null;
    }

    private void GetSelectedPieceInfo(string selectedObjectName, out int selectedLevel, out string selectedColor)
    {
        int pieceIndex = int.Parse(selectedObjectName[0].ToString());
        selectedColor = ColorMapping.GetColor(pieceIndex);

        selectedLevel = int.Parse(selection.parent.name[0].ToString());
    }
}
