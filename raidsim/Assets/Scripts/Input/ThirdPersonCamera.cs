using System;
using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    public SimpleFreecam freecam;

    public float mouseSensitivity = 1.5f;

    public Transform target;
    public float dstFromTarget = 13f;

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
        if (!Input.GetKey(KeyCode.Space) && !freecam.active)
        {
            yaw += Input.GetAxis("Mouse X") * mouseSensitivity;
            pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity;
            pitch = Mathf.Clamp(pitch, pitchMinMax.x, pitchMinMax.y);
            currentRotation = Vector3.SmoothDamp(currentRotation, new Vector3(pitch, yaw), ref rotationSmoothVelocity, rotationSmoothTime);
            transform.eulerAngles = currentRotation;
            transform.position = target.position + new Vector3(0f, 2f, 0f) - transform.forward * dstFromTarget;
        }
    }

    public void RandomRotate()
    {
        yaw = UnityEngine.Random.Range(0, 360);
    }
}