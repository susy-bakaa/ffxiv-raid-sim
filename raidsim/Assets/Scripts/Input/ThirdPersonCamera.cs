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

    void Awake()
    {
        if (freecam == null)
            freecam = GetComponent<SimpleFreecam>();
    }

    void Update()
    {
        if (!freecam.active)
        {
            HandleCameraRotation();
            HandleCameraZoom();
            UpdateCameraPosition();
        }
    }

    void HandleCameraRotation()
    {
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

        if (Input.GetMouseButton(1))
        {
            yaw += Input.GetAxis("Mouse X") * mouseSensitivity;
            pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity;
            pitch = Mathf.Clamp(pitch, pitchMinMax.x, pitchMinMax.y);
            currentRotation = Vector3.SmoothDamp(currentRotation, new Vector3(pitch, yaw), ref rotationSmoothVelocity, rotationSmoothTime);
            transform.eulerAngles = currentRotation;
        }
    }

    void HandleCameraZoom()
    {
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        dstFromTarget -= scrollInput * mouseSensitivity * 5f; // Adjust the zoom sensitivity as needed
        dstFromTarget = Mathf.Clamp(dstFromTarget, dstMinMax.x, dstMinMax.y);
    }

    void UpdateCameraPosition()
    {
        transform.position = target.position + new Vector3(0f, 2f, 0f) - transform.forward * dstFromTarget;
    }

    public void RandomRotate()
    {
        yaw = UnityEngine.Random.Range(0, 360);
    }
}
