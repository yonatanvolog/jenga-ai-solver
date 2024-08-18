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

    void Start()
    {
        // Initialize current positions and distances based on start values
        currentYPosition = Mathf.Clamp(startPosition.y, minYPosition, maxYPosition);
        targetDistance = Vector3.Distance(target.position, startPosition);
        currentDistance = targetDistance;

        // Set the initial rotation angles
        currentY = startRotation.y;

        // Apply the initial position and rotation
        transform.position = startPosition;
        transform.rotation = Quaternion.Euler(startRotation);

        // Ensure the camera is looking at the target initially
        transform.LookAt(target);
    }

    void Update()
    {
        // Check if the right mouse button is held down
        if (Input.GetMouseButton(1))
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
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            targetDistance -= scroll * scrollSpeed;
            targetDistance = Mathf.Clamp(targetDistance, minDistance, maxDistance);
        }

        // Smoothly interpolate the distance for a nice fade effect
        currentDistance = Mathf.Lerp(currentDistance, targetDistance, Time.deltaTime / zoomDampening);

        // Update the camera position
        UpdateCameraPosition();
    }

    void UpdateCameraPosition()
    {
        // Calculate the direction from the target to the camera
        Vector3 direction = new Vector3(0, 0, -currentDistance);

        // Apply rotation based on currentY
        Quaternion rotation = Quaternion.Euler(startRotation.x, currentY, 0);
        Vector3 position = target.position + rotation * direction;

        // Apply vertical position adjustment based on currentYPosition
        position.y = currentYPosition;

        // Apply the calculated rotation and position
        transform.position = position;
        transform.LookAt(target);
    }
}
