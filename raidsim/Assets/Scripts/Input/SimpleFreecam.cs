using UnityEngine;

public class SimpleFreecam : MonoBehaviour
{
    public bool active;
    public float normalMovementSpeed = 5f; // Normal speed of camera movement
    public float fastMovementMultiplier = 2f; // Multiplier for fast movement
    public float slowMovementMultiplier = 0.5f; // Multiplier for slow movement
    public float rotationSpeed = 2f; // Speed of camera rotation

    public bool cursorVisible = false; // Indicates if the cursor is visible

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked; // Lock the cursor at start
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tilde) || Input.GetKeyDown(KeyCode.BackQuote) || Input.GetKeyDown(KeyCode.ScrollLock))
        {
            active = !active;
        }

        // Toggle cursor visibility with space key
        if (Input.GetKey(KeyCode.Space))
        {
            cursorVisible = true;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = cursorVisible;
        }
        else
        {
            cursorVisible = false;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = cursorVisible;
        }

        if (!active)
        {
            return;
        }

        // Speed adjustment
        float speedMultiplier = 1f; // Default speed multiplier

        if (Input.GetAxis("Speed") > 0f)
        {
            speedMultiplier *= fastMovementMultiplier; // Double speed when Shift is held down
        }
        else if (Input.GetAxis("Speed") < 0f)
        {
            speedMultiplier *= slowMovementMultiplier; // Half speed when Control is held down
        }

        // Camera movement
        float horizontalMovement = Input.GetAxis("Horizontal");
        float verticalMovement = Input.GetAxis("Vertical");
        float flyMovement = Input.GetAxis("Fly");

        Vector3 movement = new Vector3(horizontalMovement, flyMovement, verticalMovement) * normalMovementSpeed * speedMultiplier * Time.deltaTime;
        transform.Translate(movement);

        // Camera rotation
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        if (cursorVisible)
        {
            mouseX = 0;
            mouseY = 0;
        }

        Vector3 rotation = new Vector3(-mouseY, mouseX, 0f) * rotationSpeed;
        transform.eulerAngles += rotation;
    }
}