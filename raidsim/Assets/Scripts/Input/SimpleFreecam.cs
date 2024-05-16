using System;
using UnityEngine;

public class SimpleFreecam : MonoBehaviour
{
    public bool active;
    public float normalMovementSpeed = 5f; // Base speed of camera movement
    public float speedAdjustmentStep = 0.5f; // Speed adjustment step for each mouse wheel scroll
    public float maxMovementSpeed = 20f; // Maximum movement speed
    public float minMovementSpeed = 1f; // Minimum movement speed
    public float rotationSpeed = 2f; // Speed of camera rotation

    private float currentMovementSpeed;
    private float defaultMovementSpeed;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked; // Lock the cursor at start
        Cursor.visible = false; // Hide the cursor at start
        currentMovementSpeed = normalMovementSpeed; // Initialize the current movement speed
        defaultMovementSpeed = normalMovementSpeed; // Set the default movement speed
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tilde) || Input.GetKeyDown(KeyCode.BackQuote) || Input.GetKeyDown(KeyCode.ScrollLock))
        {
            active = !active;
        }

        if (!active)
        {
            return;
        }

        // Handle cursor visibility and locking with mouse buttons
        if (Input.GetMouseButtonDown(1))
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        if (Input.GetMouseButtonUp(1))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        // Reset movement speed to default when Control key is pressed
        if (Input.GetButtonDown("ResetSpeed"))
        {
            currentMovementSpeed = defaultMovementSpeed;
        }

        // Adjust movement speed with mouse wheel
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        currentMovementSpeed += scrollInput * speedAdjustmentStep;
        currentMovementSpeed = Mathf.Clamp(currentMovementSpeed, minMovementSpeed, maxMovementSpeed);

        // Camera movement
        float horizontalMovement = Input.GetAxis("Horizontal");
        float verticalMovement = Input.GetAxis("Vertical");
        float flyMovement = Input.GetAxis("Fly");

        Vector3 movement = new Vector3(horizontalMovement, flyMovement, verticalMovement) * currentMovementSpeed * Time.deltaTime;
        transform.Translate(movement);

        // Camera rotation
        if (Input.GetMouseButton(1)) // Rotate camera only when either mouse button is held down
        {
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");

            Vector3 rotation = new Vector3(-mouseY, mouseX, 0f) * rotationSpeed;
            transform.eulerAngles += rotation;
        }
    }
}
