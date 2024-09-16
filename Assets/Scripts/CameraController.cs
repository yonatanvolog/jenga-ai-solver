using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float rotationSpeed = 5.0f;
    [SerializeField] private float minYPosition = 1.0f;
    [SerializeField] private float maxYPosition = 10.0f;
    [SerializeField] private float scrollSpeed = 2.0f;
    [SerializeField] private float minDistance = 5.0f;
    [SerializeField] private float maxDistance = 15.0f;
    [SerializeField] private float zoomDampening = 0.1f;

    [SerializeField] private Vector3 startPosition = new Vector3(0, 5.0f, 0);
    [SerializeField] private Vector3 startRotation = new Vector3(0, 0, 0);

    [SerializeField] private float waveAmplitudeY = 5.0f;
    [SerializeField] private float waveAmplitudeRotationX = 10.0f;
    [SerializeField] private float waveFrequency = 1.0f;

    private float currentY;
    private float currentYPosition;
    private float targetDistance;
    private float currentDistance;

    private Vector3 targetPositionCopy;
    private Quaternion targetRotationCopy;

    private bool isMouseControlEnabled = true;
    private bool isMenuModeEnabled = false;
    private float menuRotationAngle = 0f;

    public void Start()
    {
        if (target != null)
        {
            targetPositionCopy = target.position;
            targetRotationCopy = target.rotation;
        }

        currentYPosition = Mathf.Clamp(startPosition.y, minYPosition, maxYPosition);
        targetDistance = Vector3.Distance(targetPositionCopy, startPosition);
        currentDistance = targetDistance;

        currentY = startRotation.y;

        transform.position = startPosition;
        transform.rotation = Quaternion.Euler(startRotation);

        transform.LookAt(targetPositionCopy);
    }

    void Update()
    {
        if (isMenuModeEnabled)
        {
            menuRotationAngle += (rotationSpeed + 22) * Time.deltaTime * (1 / Time.timeScale);

            float wave = (Mathf.Sin(Time.time * waveFrequency * (1 / Time.timeScale)) + 1) / 2;

            currentY = menuRotationAngle;
            currentYPosition = Mathf.Lerp(minYPosition + 30, minYPosition + 30 + waveAmplitudeY, wave);
            float currentRotationX = Mathf.Lerp(startRotation.x, startRotation.x - waveAmplitudeRotationX, wave);

            UpdateCameraPosition(currentRotationX);
        }
        else if (isMouseControlEnabled && Input.GetMouseButton(1))
        {
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");

            currentY += mouseX * rotationSpeed;
            currentYPosition -= mouseY * rotationSpeed;

            currentYPosition = Mathf.Clamp(currentYPosition, minYPosition, maxYPosition);
        }

        if (isMouseControlEnabled)
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll != 0)
            {
                targetDistance -= scroll * scrollSpeed;
                targetDistance = Mathf.Clamp(targetDistance, minDistance, maxDistance);
            }
        }

        currentDistance = Mathf.Lerp(currentDistance, targetDistance, Time.deltaTime / zoomDampening);

        if (!isMenuModeEnabled)
        {
            UpdateCameraPosition(startRotation.x);
        }
    }

    void UpdateCameraPosition(float currentRotationX)
    {
        Vector3 direction = new Vector3(0, 0, -currentDistance);
        Quaternion rotation = Quaternion.Euler(currentRotationX, currentY, 0);
        Vector3 position = targetPositionCopy + rotation * direction;

        position.y = currentYPosition;

        transform.position = position;
        transform.LookAt(targetPositionCopy);
    }

    public void ToggleMouseControl(bool isEnabled)
    {
        isMouseControlEnabled = isEnabled;
    }

    public void ToggleMenuMode(bool isEnabled)
    {
        isMenuModeEnabled = isEnabled;

        if (isEnabled)
        {
            menuRotationAngle = 0f;
        }
    }
}
