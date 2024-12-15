using System;
using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    public SimpleFreecam freecam;

    public float mouseSensitivity = 1.5f;
    public Transform target;
    public float dstFromTarget = 13f;
    public Vector2 dstMinMax = new Vector2(5f, 20f); // Minimum and maximum distance for zoom

    public Vector2 pitchMinMax = new Vector2(-40f, 85f);

    public float rotationSmoothTime;

    private Vector3 rotationSmoothVelocity;
    private Vector3 currentRotation;

    private float yaw;
    private float pitch;
    private float scrollInput;

    public bool enableZooming = true;
    public bool enableRotation = true;
    public bool enableMovement = true;

    private Vector2 cursorPosition;
    private bool cursorPositionSet;
#if UNITY_STANDALONE_LINUX
    private float heldTime;
    private float heldTimeThreshold = 0.2f;
#endif
    void Awake()
    {
        if (freecam == null)
            freecam = GetComponent<SimpleFreecam>();
    }

    void Update()
    {
        if (target == null)
            return;

        if (!freecam.active)
        {
            if (enableRotation)
                HandleCameraRotation();
            if (enableZooming)
                HandleCameraZoom();
            if (enableMovement)
                UpdateCameraPosition();
        }
    }

    void HandleCameraRotation()
    {
        if (target == null)
            return;

        if (Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(0))
        {
#if PLATFORM_STANDALONE_WIN
            if (!cursorPositionSet)
            {
                cursorPosition = CursorControl.GetPosition();
                cursorPositionSet = true;
            }
#elif UNITY_EDITOR_WIN
            if (!cursorPositionSet)
            {
                cursorPosition = CursorControl.GetPosition();
                cursorPositionSet = true;
            }
//#elif UNITY_STANDALONE_LINUX
//            Cursor.lockState = CursorLockMode.Locked;
#else
            Cursor.lockState = CursorLockMode.Confined;
#endif
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
#elif UNITY_EDITOR_WIN
            if (cursorPositionSet)
            {
                CursorControl.SetPosition(cursorPosition);
                cursorPositionSet = false;
            }
#elif UNITY_STANDALONE_LINUX
            heldTime = 0;
#endif
            Cursor.visible = true;
        }
        if (Input.GetMouseButton(1) || Input.GetMouseButton(0))
        {
#if UNITY_STANDALONE_LINUX
            
            if (heldTime >= heldTimeThreshold)
            {
                heldTime = heldTimeThreshold;
                Cursor.lockState = CursorLockMode.Locked;
            }
            else
            {
                heldTime += Time.unscaledDeltaTime;
            }
#endif
            yaw += Input.GetAxis("Mouse X") * mouseSensitivity;
            pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity;
            pitch = Mathf.Clamp(pitch, pitchMinMax.x, pitchMinMax.y);
            currentRotation = Vector3.SmoothDamp(currentRotation, new Vector3(pitch, yaw), ref rotationSmoothVelocity, rotationSmoothTime, float.PositiveInfinity, Time.unscaledDeltaTime);
            transform.eulerAngles = currentRotation;
        }
    }

    void HandleCameraZoom()
    {
        scrollInput = Input.GetAxis("Mouse ScrollWheel");
        dstFromTarget -= scrollInput * mouseSensitivity * 5f; // Adjust the zoom sensitivity as needed
        dstFromTarget = Mathf.Clamp(dstFromTarget, dstMinMax.x, dstMinMax.y);
    }

    void UpdateCameraPosition()
    {
        if (target == null)
            return;

        transform.position = target.position + new Vector3(0f, 2f, 0f) - transform.forward * dstFromTarget;
    }

    public void RandomRotate()
    {
        // Fix some slight bugginess
        //yaw = UnityEngine.Random.Range(0, 360);
    }
}
