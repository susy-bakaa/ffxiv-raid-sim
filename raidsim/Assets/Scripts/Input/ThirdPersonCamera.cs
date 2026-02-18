// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using dev.susybaka.Shared.UserInterface;
using static dev.susybaka.raidsim.Core.GlobalData;
using static dev.susybaka.raidsim.SaveLoad.KeepCameraRotation;

namespace dev.susybaka.raidsim.Inputs
{
    public class ThirdPersonCamera : MonoBehaviour
    {
        public SimpleFreecam freecam;

        public float mouseSensitivity = 1.5f;
        public float controllerSensitivity = 1.5f;
        public bool invertX = false;
        public bool invertY = false;
        public Transform target;
        public float dstFromTarget = 13f;
        public Vector3 offsetFromTarget = new Vector3(0f, 2f, 0f);
        public Vector2 offsetMinMax = new Vector2(-8f, 8f);
        public Vector2 dstMinMax = new Vector2(5f, 20f); // Minimum and maximum distance for zoom

        public Vector2 pitchMinMax = new Vector2(-40f, 85f);

        public float rotationSmoothTime;
        public InputActionReference controllerCameraBind;
        public InputActionReference controllerCameraZoomBind;

        private Vector2 controllerInput;
        private Vector3 rotationSmoothVelocity;
        private Vector3 currentRotation;
        private Vector3 defaultOffset;

        private float yaw;
        private float pitch;
        private float scrollInput;
        //private bool externalControl = false;

        public bool enableZooming = true;
        public bool enableRotation = true;
        public bool enableMovement = true;

        public Axis autoAdjustAxis = new Axis(false, true, false);

        public UnityEvent<float> onCameraOffsetUpdated;

        private Vector2 cursorPosition;
        private bool cursorPositionSet;
        private float heldTime;
        private float heldTimeThreshold = 0.2f;
        private float lastHeldTime;
        private float lastHeldTimeTreshold = 0.4f;

        private void Awake()
        {
            if (freecam == null)
                freecam = GetComponent<SimpleFreecam>();

            defaultOffset = offsetFromTarget;
        }

        private void OnEnable()
        {
            controllerCameraBind.action.Enable();
            controllerCameraZoomBind.action.Enable();
        }

        private void OnDisable()
        {
            controllerCameraBind.action.Disable();
            controllerCameraZoomBind.action.Disable();
        }

        private void Update()
        {
            if (target == null)
                return;

            if (controllerInput != null)
            {
                controllerInput = controllerCameraBind.action.ReadValue<Vector2>();
            }

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

        public void SetCameraTransform(CameraSaveData data)
        {
            currentRotation = data.rotation.eulerAngles;
            yaw = data.rotation.eulerAngles.y;
            pitch = data.rotation.eulerAngles.x;
        }

        private void HandleCameraRotation()
        {
            if (target == null) // || externalControl
                return;

            bool isMouseInput = Input.GetMouseButton(1) || Input.GetMouseButton(0);
            bool isControllerInput = controllerInput != Vector2.zero;

            // Only synchronize when not actively receiving input to prevent feedback loops
            if (!isMouseInput && !isControllerInput)
            {
                // Synchronize yaw and pitch with the current camera rotation
                yaw = transform.eulerAngles.y;
                // Convert transform.eulerAngles.x to the correct pitch range
                float rawPitch = transform.eulerAngles.x;
                pitch = (rawPitch > 180) ? rawPitch - 360 : rawPitch;

                // Reset smooth damp velocity when not receiving input
                if (rotationSmoothVelocity.sqrMagnitude > 0.01f)
                {
                    rotationSmoothVelocity = Vector3.zero;
                    currentRotation = transform.eulerAngles;
                }
            }

            if (Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(0))
            {
#if PLATFORM_STANDALONE_WIN && !PLATFORM_STANDALONE_LINUX && !UNITY_EDITOR_LINUX
                if (!cursorPositionSet)
                {
                    cursorPosition = CursorControl.GetPosition();
                    cursorPositionSet = true;
                }
                Cursor.lockState = CursorLockMode.Confined;
#else
                Cursor.lockState = CursorLockMode.Confined;
#endif
                if (CursorHandler.Instance != null)
                {
                    CursorHandler.Instance.visible = false;
                }
                else
                {
                    Cursor.visible = false;
                }
            }
            if ((Input.GetMouseButtonUp(1) && !Input.GetMouseButton(0)) || (Input.GetMouseButtonUp(0) && !Input.GetMouseButton(1)))
            {
                Cursor.lockState = CursorLockMode.None;
#if PLATFORM_STANDALONE_WIN && !PLATFORM_STANDALONE_LINUX && !UNITY_EDITOR_LINUX
                if (cursorPositionSet)
                {
                    CursorControl.SetPosition(cursorPosition);
                    cursorPositionSet = false;
                }
#elif UNITY_EDITOR_WIN && !UNITY_EDITOR_LINUX
                if (cursorPositionSet)
                {
                    CursorControl.SetPosition(cursorPosition);
                    cursorPositionSet = false;
                }
#endif
                heldTime = 0;
                lastHeldTime = 0;

                if (CursorHandler.Instance != null)
                {
                    CursorHandler.Instance.visible = true;
                }
                else
                {
                    Cursor.visible = true;
                }
            }
            if (Input.GetMouseButton(1) || Input.GetMouseButton(0))
            {
                if (heldTime >= heldTimeThreshold)
                {
                    heldTime = heldTimeThreshold;
                    Cursor.lockState = CursorLockMode.Locked;
                }
                else
                {
                    heldTime += Time.unscaledDeltaTime;
                }

                if (!invertX)
                    yaw += Input.GetAxis("Mouse X") * mouseSensitivity;
                else
                    yaw -= Input.GetAxis("Mouse X") * mouseSensitivity;
                if (!invertY)
                    pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity;
                else
                    pitch += Input.GetAxis("Mouse Y") * mouseSensitivity;

                // Normalize yaw to prevent overflow
                yaw = Mathf.Repeat(yaw, 360f);
                pitch = Mathf.Clamp(pitch, pitchMinMax.x, pitchMinMax.y);
                currentRotation = Vector3.SmoothDamp(currentRotation, new Vector3(pitch, yaw, 0f), ref rotationSmoothVelocity, rotationSmoothTime, float.PositiveInfinity, Time.unscaledDeltaTime);
                transform.eulerAngles = currentRotation;
            }
            else if (controllerInput != Vector2.zero)
            {
                if (!invertX)
                    yaw += controllerInput.x * controllerSensitivity;
                else
                    yaw -= controllerInput.x * controllerSensitivity;

                if (controllerCameraZoomBind != null)
                {
                    if (!controllerCameraZoomBind.action.IsPressed())
                    {
                        if (!invertY)
                            pitch -= controllerInput.y * controllerSensitivity;
                        else
                            pitch += controllerInput.y * controllerSensitivity;
                    }
                }
                else
                {
                    if (!invertY)
                        pitch -= controllerInput.y * controllerSensitivity;
                    else
                        pitch += controllerInput.y * controllerSensitivity;
                }

                // Normalize yaw to prevent overflow
                yaw = Mathf.Repeat(yaw, 360f);
                pitch = Mathf.Clamp(pitch, pitchMinMax.x, pitchMinMax.y);
                currentRotation = Vector3.SmoothDamp(currentRotation, new Vector3(pitch, yaw, 0f), ref rotationSmoothVelocity, rotationSmoothTime, float.PositiveInfinity, Time.unscaledDeltaTime);
                transform.eulerAngles = currentRotation;
            }

            // Fix cursor state sometimes not resetting if the mouse button is released outside the game window
            // This gives a small grace period where the cursor will reset after the mouse button is released, even if the release event was missed
            if (!isMouseInput)
            {
                lastHeldTime += Time.unscaledDeltaTime;

                if (lastHeldTime >= lastHeldTimeTreshold)
                {
                    Cursor.lockState = CursorLockMode.None;
#if PLATFORM_STANDALONE_WIN && !PLATFORM_STANDALONE_LINUX && !UNITY_EDITOR_LINUX
                    if (cursorPositionSet)
                    {
                        CursorControl.SetPosition(cursorPosition);
                        cursorPositionSet = false;
                    }
#elif UNITY_EDITOR_WIN && !UNITY_EDITOR_LINUX
                if (cursorPositionSet)
                {
                    CursorControl.SetPosition(cursorPosition);
                    cursorPositionSet = false;
                }
#endif
                    heldTime = 0;

                    if (CursorHandler.Instance != null)
                    {
                        CursorHandler.Instance.visible = true;
                    }
                    else
                    {
                        Cursor.visible = true;
                    }
                }
            }
            else if (isMouseInput || Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(0))
            {
                lastHeldTime = 0;
            }
        }

        private void HandleCameraZoom()
        {
            scrollInput = Input.GetAxis("Mouse ScrollWheel");
            bool usedController = false;

            if (controllerCameraZoomBind != null)
            {
                if (controllerCameraZoomBind.action.IsPressed())
                {
                    scrollInput += controllerInput.y;
                    usedController = true;
                }
            }

            if (!usedController)
                dstFromTarget -= scrollInput * mouseSensitivity * 5f;
            else
                dstFromTarget -= (scrollInput * 0.04f) * mouseSensitivity * 5f;
            dstFromTarget = Mathf.Clamp(dstFromTarget, dstMinMax.x, dstMinMax.y);
        }

        private void UpdateCameraPosition()
        {
            if (target == null)
                return;

            transform.position = target.position + offsetFromTarget - transform.forward * dstFromTarget;
        }

        public void UpdateCameraOffset(float value)
        {
            offsetFromTarget.y += value * Time.unscaledDeltaTime;
            offsetFromTarget.y = Mathf.Clamp(offsetFromTarget.y, offsetMinMax.x, offsetMinMax.y);
            if (value >= 9999)
            {
                offsetFromTarget = defaultOffset;
            }
            onCameraOffsetUpdated.Invoke(offsetFromTarget.y);
        }

        public void AutoAdjustCamera(float smoothing)
        {
            if (autoAdjustAxis.None())
                return;

            // Store the original rotation for axes that should not be adjusted
            Vector3 original = transform.eulerAngles;

            // Compute the target rotation based on the target's forward direction
            Quaternion targetRotation = Quaternion.LookRotation(target.forward, Vector3.up);
            Vector3 targetEulerAngles = targetRotation.eulerAngles;

            // Apply auto-adjust based on enabled axes
            currentRotation = transform.eulerAngles; // Start with the current rotation
            if (autoAdjustAxis.x)
                currentRotation.x = Mathf.LerpAngle(currentRotation.x, targetEulerAngles.x, Time.deltaTime * smoothing);
            else
                currentRotation.x = original.x;

            if (autoAdjustAxis.y)
                currentRotation.y = Mathf.LerpAngle(currentRotation.y, targetEulerAngles.y, Time.deltaTime * smoothing);
            else
                currentRotation.y = original.y;

            if (autoAdjustAxis.z)
                currentRotation.z = Mathf.LerpAngle(currentRotation.z, targetEulerAngles.z, Time.deltaTime * smoothing);
            else
                currentRotation.z = original.z;

            // Apply the calculated rotation to the camera
            transform.eulerAngles = currentRotation;
        }

        public void RandomRotate()
        {
            // Fix some slight bugginess
            //yaw = UnityEngine.Random.Range(0, 360);
        }
    }
}