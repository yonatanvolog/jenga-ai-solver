using UnityEngine;
using System.Collections;

public class FallDetectModifier : MonoBehaviour
{
    [SerializeField] private GameObject topCollider;
    [SerializeField] private GameObject leftCollider;
    [SerializeField] private GameObject rightCollider;
    [SerializeField] private GameObject bottomCollider;

    private Transform topTransform;
    private Transform leftTransform;
    private Transform rightTransform;
    private Transform bottomTransform;

    private MeshRenderer topMeshRenderer;
    private MeshRenderer leftMeshRenderer;
    private MeshRenderer rightMeshRenderer;
    private MeshRenderer bottomMeshRenderer;

    [SerializeField] private Vector3 topMoveDirection = Vector3.down;
    [SerializeField] private Vector3 leftMoveDirection = Vector3.left;
    [SerializeField] private Vector3 rightMoveDirection = Vector3.right;
    [SerializeField] private Vector3 bottomMoveDirection = Vector3.up;

    [SerializeField] private float growthFactor = 2.0f;

    private bool isCoroutineRunning = false;

    void Start()
    {
        if (topCollider != null)
        {
            topTransform = topCollider.transform;
            topMeshRenderer = topCollider.GetComponent<MeshRenderer>();
        }
        if (leftCollider != null)
        {
            leftTransform = leftCollider.transform;
            leftMeshRenderer = leftCollider.GetComponent<MeshRenderer>();
        }
        if (rightCollider != null)
        {
            rightTransform = rightCollider.transform;
            rightMeshRenderer = rightCollider.GetComponent<MeshRenderer>();
        }
        if (bottomCollider != null)
        {
            bottomTransform = bottomCollider.transform;
            bottomMeshRenderer = bottomCollider.GetComponent<MeshRenderer>();
        }
    }

    public void SetDistance(float value)
    {
        if (!isCoroutineRunning)
        {
            StartCoroutine(HandleSetDistance(value));
        }
    }

    private IEnumerator HandleSetDistance(float value)
    {
        isCoroutineRunning = true;

        SetMeshRendererEnabled(true);
        yield return StartCoroutine(LerpColliders(value, 0f));
        SetMeshRendererEnabled(false);

        isCoroutineRunning = false;
    }

    private IEnumerator LerpColliders(float value, float duration)
    {
        float time = 0f;

        Vector3 initialTopPosition = topTransform.position;
        Vector3 initialBottomPosition = bottomTransform.position;
        Vector3 initialLeftPosition = leftTransform.position;
        Vector3 initialRightPosition = rightTransform.position;

        Vector3 targetTopPosition = initialTopPosition + (topMoveDirection.normalized * value);
        Vector3 targetBottomPosition = initialBottomPosition + (bottomMoveDirection.normalized * value);
        Vector3 targetLeftPosition = initialLeftPosition + (leftMoveDirection.normalized * value);
        Vector3 targetRightPosition = initialRightPosition + (rightMoveDirection.normalized * value);

        Vector3 initialTopScale = topTransform.localScale;
        Vector3 initialBottomScale = bottomTransform.localScale;

        Vector3 targetTopScale = initialTopScale + new Vector3(0, 0, growthFactor * value);
        Vector3 targetBottomScale = initialBottomScale + new Vector3(0, 0, growthFactor * value);

        while (time < duration)
        {
            float t = time / duration;

            if (topTransform != null)
            {
                topTransform.position = Vector3.Lerp(initialTopPosition, targetTopPosition, t);
                topTransform.localScale = Vector3.Lerp(initialTopScale, targetTopScale, t);
            }

            if (bottomTransform != null)
            {
                bottomTransform.position = Vector3.Lerp(initialBottomPosition, targetBottomPosition, t);
                bottomTransform.localScale = Vector3.Lerp(initialBottomScale, targetBottomScale, t);
            }

            if (leftTransform != null)
            {
                leftTransform.position = Vector3.Lerp(initialLeftPosition, targetLeftPosition, t);
            }

            if (rightTransform != null)
            {
                rightTransform.position = Vector3.Lerp(initialRightPosition, targetRightPosition, t);
            }

            time += Time.deltaTime;
            yield return null;
        }

        if (topTransform != null)
        {
            topTransform.position = targetTopPosition;
            topTransform.localScale = targetTopScale;
        }

        if (bottomTransform != null)
        {
            bottomTransform.position = targetBottomPosition;
            bottomTransform.localScale = targetBottomScale;
        }

        if (leftTransform != null)
        {
            leftTransform.position = targetLeftPosition;
        }

        if (rightTransform != null)
        {
            rightTransform.position = targetRightPosition;
        }
    }

    private void SetMeshRendererEnabled(bool isEnabled)
    {
        if (topMeshRenderer != null)
        {
            topMeshRenderer.enabled = isEnabled;
        }
        if (leftMeshRenderer != null)
        {
            leftMeshRenderer.enabled = isEnabled;
        }
        if (rightMeshRenderer != null)
        {
            rightMeshRenderer.enabled = isEnabled;
        }
        if (bottomMeshRenderer != null)
        {
            bottomMeshRenderer.enabled = isEnabled;
        }
    }

    public void ResetFallDetectors()
    {
        foreach (Transform child in transform)
        {
            MeshRenderer renderer = child.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                renderer.enabled = false;
            }
        }
    }
}
