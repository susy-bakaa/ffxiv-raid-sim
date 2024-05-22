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

    public bool enableSpeed = true;
    public bool enableRotation = true;
    public bool enableMovement = true;

    private Vector2 cursorPosition;
    private bool cursorPositionSet;

    void Start()
    {
        currentMovementSpeed = normalMovementSpeed; // Initialize the current movement speed
        defaultMovementSpeed = normalMovementSpeed; // Set the default movement speed
    }

    void Update()
    {
        // Toggle active state
        if (Input.GetKeyDown(KeyCode.Tilde) || Input.GetKeyDown(KeyCode.BackQuote) || Input.GetKeyDown(KeyCode.ScrollLock))
        {
            active = !active;
        }

        // If the camera is not active, do nothing
        if (!active)
        {
            return;
        }

        // Handle cursor visibility and locking with mouse buttons
        if (enableRotation)
        {
            if (Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(0))
            {
#if PLATFORM_STANDALONE_WIN
                if (!cursorPositionSet)
                {
                    cursorPosition = CursorControl.GetPosition();
                    cursorPositionSet = true;
                }
#endif
#if UNITY_EDITOR_WIN
                if (!cursorPositionSet)
                {
                    cursorPosition = CursorControl.GetPosition();
                    cursorPositionSet = true;
                }
#endif
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            if ((Input.GetMouseButtonUp(1) && !Input.GetMouseButton(0)) || (Input.GetMouseButtonUp(0) && !Input.GetMouseButton(1)))
            {
                Cursor.lockState = CursorLockMode.None;
#if PLATFORM_STANDALONE_WIN
                if (cursorPositionSet)
                {
                    CursorControl.SetPosition(cursorPosition);
                    cursorPositionSet = false;
                }
#endif
#if UNITY_EDITOR_WIN
                if (cursorPositionSet)
                {
                    CursorControl.SetPosition(cursorPosition);
                    cursorPositionSet = false;
                }
#endif
                Cursor.visible = true;
            }
        }

        // Reset movement speed to default when Control key is pressed
        if (Input.GetButtonDown("ResetSpeed"))
        {
            currentMovementSpeed = defaultMovementSpeed;
        }

        // Adjust movement speed with mouse wheel
        if (enableSpeed)
        {
            float scrollInput = Input.GetAxis("Mouse ScrollWheel");
            currentMovementSpeed += scrollInput * speedAdjustmentStep;
            currentMovementSpeed = Mathf.Clamp(currentMovementSpeed, minMovementSpeed, maxMovementSpeed);
        }

        // Camera movement
        if (enableMovement)
        {
            float horizontalMovement = Input.GetAxisRaw("Horizontal");
            float verticalMovement = Input.GetAxisRaw("Vertical");
            float flyMovement = Input.GetAxisRaw("Fly"); // Ensure "Fly" axis is defined in Input Manager

            //Debug.Log($"Movement Input: Horizontal {horizontalMovement}, Vertical {verticalMovement}, Fly {flyMovement}");

            Vector3 movement = new Vector3(horizontalMovement, flyMovement, verticalMovement) * currentMovementSpeed * Time.unscaledDeltaTime;

            //Debug.Log($"Vector3 movement: {movement}");

            transform.Translate(movement);
        }

        // Camera rotation
        if (enableRotation)
        {
            if (Input.GetMouseButton(1) || Input.GetMouseButton(0)) // Rotate camera only when either mouse button is held down
            {
                float mouseX = Input.GetAxis("Mouse X");
                float mouseY = Input.GetAxis("Mouse Y");

                Vector3 rotation = new Vector3(-mouseY, mouseX, 0f) * rotationSpeed;
                transform.eulerAngles += rotation;
            }
        }
    }
}