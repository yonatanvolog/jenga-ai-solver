using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform target; // The object to rotate around
    public float rotationSpeed = 5.0f;
    public float minYPosition = 1.0f;
    public float maxYPosition = 10.0f;
    public float scrollSpeed = 2.0f;
    public float minDistance = 5.0f;
    public float maxDistance = 15.0f;
    public float zoomDampening = 0.1f;

    public Vector3 startPosition = new Vector3(0, 5.0f, 0); // Set the initial position
    public Vector3 startRotation = new Vector3(0, 0, 0); // Set the initial rotation

    private float currentY;
    private float currentYPosition;
    private float targetDistance;
    private float currentDistance;

    private Vector3 targetPositionCopy;
    private Quaternion targetRotationCopy;

    private bool isMouseControlEnabled = false; // Tracks if mouse control is enabled
    private bool isMenuModeEnabled = true;    // Tracks if menu mode is enabled
    private float menuRotationAngle = 0f;      // Tracks rotation for menu mode

    // Amplitude and frequency for the wave effect
    public float waveAmplitudeY = 5.0f; // How much the Y position should change
    public float waveAmplitudeRotationX = 10.0f; // How much the X rotation should change
    public float waveFrequency = 1.0f; // Speed of the wave

    public void Start()
    {
        // Create deep copies of the target's position and rotation
        if (target != null)
        {
            targetPositionCopy = target.position;
            targetRotationCopy = target.rotation;
        }

        // Initialize current positions and distances based on start values
        currentYPosition = Mathf.Clamp(startPosition.y, minYPosition, maxYPosition);
        targetDistance = Vector3.Distance(targetPositionCopy, startPosition);
        currentDistance = targetDistance;

        // Set the initial rotation angles
        currentY = startRotation.y;

        // Apply the initial position and rotation
        transform.position = startPosition;
        transform.rotation = Quaternion.Euler(startRotation);

        // Ensure the camera is looking at the target initially
        transform.LookAt(targetPositionCopy);
    }

    void Update()
    {
        // Menu mode rotation with wave effect
        if (isMenuModeEnabled)
        {
            print("timescale :" + Time.timeScale);
            menuRotationAngle += (rotationSpeed + 22) * Time.deltaTime * (1 / Time.timeScale); // Rotate smoothly, adjust rotation speed to menu/human mode

            // Apply the wave effect using the sine function for Y position and X rotation
            float wave = (Mathf.Sin(Time.time * waveFrequency * (1 / Time.timeScale)) + 1) / 2; // Normalize sin to be between 0 and 1

            currentY = menuRotationAngle; // Spin the camera
            currentYPosition = Mathf.Lerp(minYPosition + 30, minYPosition + 30 + waveAmplitudeY, wave); // Adjust Y position
            float currentRotationX = Mathf.Lerp(startRotation.x, startRotation.x - waveAmplitudeRotationX, wave); // Adjust X rotation

            // Update the camera position and rotation based on the wave effect
            UpdateCameraPosition(currentRotationX);
        }
        else if (isMouseControlEnabled && Input.GetMouseButton(1)) // Mouse control for camera
        {
            // Get the mouse movement
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");

            // Calculate the Y rotation
            currentY += mouseX * rotationSpeed;
            currentYPosition -= mouseY * rotationSpeed;

            // Clamp the vertical position
            currentYPosition = Mathf.Clamp(currentYPosition, minYPosition, maxYPosition);
        }

        // Adjust distance from target using scroll wheel
        if (isMouseControlEnabled)
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll != 0)
            {
                targetDistance -= scroll * scrollSpeed;
                targetDistance = Mathf.Clamp(targetDistance, minDistance, maxDistance);
            }
        }

        // Smoothly interpolate the distance for a nice fade effect
        currentDistance = Mathf.Lerp(currentDistance, targetDistance, Time.deltaTime / zoomDampening);

        // Update the camera position when not in menu mode
        if (!isMenuModeEnabled)
        {
            UpdateCameraPosition(startRotation.x);
        }
    }

    void UpdateCameraPosition(float currentRotationX)
    {
        // Calculate the direction from the target to the camera
        Vector3 direction = new Vector3(0, 0, -currentDistance);

        // Apply rotation based on currentY and currentRotationX
        Quaternion rotation = Quaternion.Euler(currentRotationX, currentY, 0);
        Vector3 position = targetPositionCopy + rotation * direction;

        // Apply vertical position adjustment based on currentYPosition
        position.y = currentYPosition;

        // Apply the calculated rotation and position
        transform.position = position;
        transform.LookAt(targetPositionCopy);
    }

    // Method to toggle mouse control on and off
    public void ToggleMouseControl(bool isEnabled)
    {
        isMouseControlEnabled = isEnabled;
    }

    // Method to enable or disable menu mode, which rotates the camera around the target
    public void ToggleMenuMode(bool isEnabled)
    {
        isMenuModeEnabled = isEnabled;

        // Optionally, reset the rotation angle when menu mode is enabled
        if (isEnabled)
        {
            menuRotationAngle = 0f; // Reset rotation for smooth transition
        }
    }
}
