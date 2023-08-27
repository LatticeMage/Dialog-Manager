using UnityEngine;

public class DragRotate : MonoBehaviour
{
    private Vector3 lastMousePosition;

    public float rotationSpeed = 0.1f;

    private void Update()
    {
        // Check if the left mouse button is pressed
        if (Input.GetMouseButtonDown(0))
        {
            lastMousePosition = Input.mousePosition;
        }
        // Check if the left mouse button is being held down
        else if (Input.GetMouseButton(0))
        {
            Vector3 delta = Input.mousePosition - lastMousePosition;
            float rotationX = delta.y * -rotationSpeed;
            float rotationY = delta.x * rotationSpeed;

            // Apply rotation
            transform.Rotate(Vector3.up, rotationY, Space.World);
            transform.Rotate(Vector3.right, rotationX, Space.World);

            // Update last mouse position
            lastMousePosition = Input.mousePosition;
        }
    }
}
