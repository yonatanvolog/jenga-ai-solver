using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform target; // The object to rotate around
    public float rotationSpeed = 5.0f;
    public float distanceFromTarget = 10.0f;
    public float minYPosition = 1.0f;
    public float maxYPosition = 10.0f;
    public float startingYPosition = 5.0f;
    public float rotationXatBottom = 30.0f;
    public float rotationXatTop = -30.0f;
    public float scrollSpeed = 2.0f;
    public float minDistance = 5.0f;
    public float maxDistance = 15.0f;
    public float zoomDampening = 0.1f;

    private float currentY = 0.0f;
    private float currentYPosition;
    private float targetDistance;
    private float currentDistance;

    void Start()
    {
        // Initialize camera position and distance
        currentYPosition = Mathf.Clamp(startingYPosition, minYPosition, maxYPosition);
        targetDistance = distanceFromTarget;
        currentDistance = distanceFromTarget;
        UpdateCameraPosition();
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
        // Calculate X rotation based on current Y position with exponential curve
        float t = (currentYPosition - minYPosition) / (maxYPosition - minYPosition);
        t = Mathf.Pow(t, 2.5f); // Exponential adjustment, higher exponent for more curve
        float xRotation = Mathf.Lerp(rotationXatBottom, rotationXatTop, t);
        
        // Adjust xRotation to be 0 at startingYPosition
        if (currentYPosition == startingYPosition)
        {
            xRotation = 0;
        }

        // Calculate the direction to the target
        Vector3 direction = new Vector3(0, 0, -currentDistance);
        Quaternion rotation = Quaternion.Euler(xRotation, currentY, 0);
        Vector3 position = target.position + rotation * direction;
        position.y = currentYPosition;

        // Apply the calculated rotation and position
        transform.position = position;
        transform.LookAt(target);
    }
}
