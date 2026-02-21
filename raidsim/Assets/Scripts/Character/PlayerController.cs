// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using UnityEngine;
using UnityEngine.InputSystem;
using NaughtyAttributes;
using dev.susybaka.raidsim.Core;
using dev.susybaka.raidsim.Inputs;
using dev.susybaka.raidsim.Nodes;
using dev.susybaka.Shared;

namespace dev.susybaka.raidsim.Characters
{
    public class PlayerController : MonoBehaviour
    {
        public enum CameraAdjustMode { Moving, Always, Never }
        CharacterState state;
        ThirdPersonCamera cameraScript;
        UserInput userInput;
        Rigidbody rb;
        [HideInInspector] public Rigidbody Rigidbody => rb;

        public Vector3 spawnOffset = new Vector3(0f, 1.25f, 0f);
        public float turnSmoothTime;
        private float turnSmoothVelocity;
        private float tm;
        private float release;
        private Animator animator;
        public Transform cameraT;
        public Vector3 targetPosition;
        public Vector3 velocity;
        public Vector3 jump;
        private float currentSpeed;
        private float speedSmoothVelocity;
        private Vector2 storedInput;
        private Vector2 storedInputR;

        public Transform model;
        public BotNode clockSpot;
        public bool enableInput = true;
        public bool legacyMovement = true;
        [HideIf("legacyMovement")] public float backpedalSpeed = 0.5f;
        [HideIf("legacyMovement")] public float turnSpeed = 90f;
        [HideIf("legacyMovement")] public CameraAdjustMode autoAdjustCamera = CameraAdjustMode.Moving;
        [HideIf("legacyMovement")] public float cameraAutoAdjustSpeed = 5f;
        [HideIf("legacyMovement")] public float cameraRotationSpeed = 180f;
        [ShowIf("legacyMovement")] public bool disableCameraPivot = true;
        [ShowIf("legacyMovement")] public bool maintainCameraDistance = true;
        public bool freezeMovement = false;
        public bool sliding = false;
        public float slideDistance = 0f;
        public float slideDuration = 0.5f;
        private bool movementFrozen = false;
        private bool knockedBack = false;
        private bool preventJumping = false;
        private bool preventGravity = false;
        private bool wasPreventGravity = false;
        private bool jumping = false;
        private bool jumpInput = false;
        public bool jumpInputAvailable = true;
        private InputActionReference jumpControllerBind;
        private InputActionReference[] controllerModifierBinds;

        private int animatorParameterDead = Animator.StringToHash("Dead");
        private int animatorParameterSpeed = Animator.StringToHash("Speed");
        private int animatorParameterDiamondback = Animator.StringToHash("Diamondback");
        private int animatorParamterSliding = Animator.StringToHash("Slipping");
        private int animatorParameterTurning = Animator.StringToHash("Turning");
        private int animatorParameterJumping = Animator.StringToHash("Jump");
        private int animatorParameterIsJumping = Animator.StringToHash("Jumping");
        private int animatorParameterIsCasting = Animator.StringToHash("Casting");
        private int animatorParameterIsDashing = Animator.StringToHash("Dashing");
        private int animatorParameterIsBlueCasting = Animator.StringToHash("General_Casting_Blue");
        private int animatorParameterIsSwipe = Animator.StringToHash("Swipe");
        private int animatorParameterIsActionLocked = Animator.StringToHash("ActionLocked");
        private int animatorParameterReset = Animator.StringToHash("Reset");

        private void Awake()
        {
            if (state == null)
            {
                if (TryGetComponent(out CharacterState state))
                {
                    this.state = state;
                }
                else
                {
                    Debug.LogError($"CharacterState script not found for PlayerController ({gameObject.name})!");
                }
            }
            if (cameraT == null)
            {
                cameraT = Camera.main.transform;
                if (cameraT == null)
                    Debug.LogError($"Camera Transform not set for PlayerController ({gameObject.name})!");
            }
            if (cameraScript == null)
            {
                if (cameraT.TryGetComponent(out ThirdPersonCamera cameraScript))
                {
                    this.cameraScript = cameraScript;
                }
                else
                {
                    Debug.LogError($"ThirdPersonCamera script not found for PlayerController ({gameObject.name}) from Camera Transform ({cameraT.gameObject.name})!");
                }
            }
            if (userInput == null)
            {
                userInput = FindObjectOfType<UserInput>();
                if (userInput == null)
                {
                    Debug.LogError($"UserInput script not found for PlayerController ({gameObject.name})!");
                }
            }

            rb = GetComponent<Rigidbody>();
            animator = GetComponent<Animator>();
        }

        private void OnEnable()
        {
            freezeMovement = false;
            movementFrozen = false;

            if (userInput == null)
            {
                userInput = FindObjectOfType<UserInput>();
                if (userInput == null)
                {
                    Debug.LogError($"UserInput script not found for PlayerController ({gameObject.name})!");
                }
            }

            if (jumpControllerBind == null)
            {
                if (userInput != null)
                {
                    for (int i = 0; i < userInput.keys.Count; i++)
                    {
                        if (userInput.keys[i].name == "Jump" && userInput.keys[i].controllerBind != null)
                        {
                            jumpControllerBind = userInput.keys[i].controllerBind;
                        }
                    }
                }
            }

            if (controllerModifierBinds == null || controllerModifierBinds.Length <= 0)
            {
                if (userInput != null)
                {
                    controllerModifierBinds = userInput.controllerModifierKeys.ToArray();
                }
            }

            if (jumpControllerBind != null)
            {
                jumpControllerBind.action.Enable();
                jumpControllerBind.action.performed += ctx => { if (jumpInputAvailable) { jumpInput = true; } else { jumpInput = false; } };
                jumpControllerBind.action.canceled += ctx => jumpInput = false;
            }

            if (controllerModifierBinds != null && controllerModifierBinds.Length > 0)
            {
                for (int i = 0; i < controllerModifierBinds.Length; i++)
                {
                    controllerModifierBinds[i].action.Enable();
                    controllerModifierBinds[i].action.performed += ctx => preventJumping = true;
                    controllerModifierBinds[i].action.canceled += ctx => preventJumping = false;
                }
            }

            Init();
        }

        private void OnDisable()
        {
            if (jumpControllerBind != null)
            {
                jumpControllerBind.action.performed -= ctx => { if (jumpInputAvailable) { jumpInput = true; } else { jumpInput = false; } };
                jumpControllerBind.action.canceled -= ctx => jumpInput = false;
                jumpControllerBind.action.Disable();
            }

            if (controllerModifierBinds != null && controllerModifierBinds.Length > 0)
            {
                for (int i = 0; i < controllerModifierBinds.Length; i++)
                {
                    controllerModifierBinds[i].action.performed -= ctx => preventJumping = true;
                    controllerModifierBinds[i].action.canceled -= ctx => preventJumping = false;
                    controllerModifierBinds[i].action.Disable();
                }
            }
        }

        private void Update()
        {
            if (preventGravity != wasPreventGravity)
            {
                wasPreventGravity = preventGravity;
                if (rb != null)
                {
                    rb.useGravity = !preventGravity;
                    rb.velocity = Vector3.zero;
                }
            }

            if (Time.timeScale > 0f)
            {
                animator.SetBool(animatorParameterDead, state.dead);
                animator.SetBool(animatorParameterDiamondback, state.HasEffect("Diamondback"));
                animator.SetBool(animatorParamterSliding, (sliding && knockedBack));

                if (CanMove())
                    return;

                if (currentSpeed > 0)
                {
                    state.still = false;
                }
                else
                {
                    state.still = true;
                }

                if (jumpInput && !preventJumping)
                {
                    jumpInputAvailable = false;
                    Utilities.FunctionTimer.Create(this, () => jumpInputAvailable = true, 1f, "jump_input_delay", false, true);
                }
            }

            tm += FightTimeline.deltaTime;
            if (tm > release || (state.dead && !sliding))
            {
                tm = release + 1f;
                state.uncontrollable.RemoveFlag("knockback");
                velocity = Vector3.zero;
                knockedBack = false;
                preventGravity = false; // Prevent gravity might need to be turned into a multi state flag later but for now only knockbacks use it
            }
            if (!state.uncontrollable.value && !state.dead && !state.bound.value && enableInput)
            {
                Vector2 input = Vector2.zero;
                Vector2 inputR = Vector2.zero;

                if ((userInput.GetButtonDown("Jump") || jumpInput) && !jumping && !preventJumping)
                {
                    jumpInput = false;
                    jumping = true;
                    model.LeanMoveLocal(jump, 0.25f).setEase(LeanTweenType.easeOutQuad).setOnComplete(() => Utilities.FunctionTimer.Create(this, () => model.LeanMoveLocal(Vector3.zero, 0.2f).setEase(LeanTweenType.easeInQuad).setOnComplete(() => jumping = false), 0.15f, "player_jump_fall_delay", false, true));
                    animator.SetTrigger(animatorParameterJumping);
                }

                // Input handling
                if (!jumping)
                {
                    input = new Vector2(userInput.GetAxis("Strafe"), userInput.GetAxis("Vertical"));
                    inputR = new Vector2(userInput.GetAxis("Horizontal"), 0f);

                    storedInput = input;
                    storedInputR = inputR;
                }
                else
                {
                    input = storedInput;
                    inputR = storedInputR;
                }

                // Handle combined mouse button movement
                if (Input.GetMouseButton(0) && Input.GetMouseButton(1) && input.y >= 0)
                {
                    input.y = 1;
                }
                // Turn character rotation input into strafing when rotating with the camera
                if (Input.GetMouseButton(1) && !legacyMovement)
                {
                    input = new Vector2(input.x + inputR.x, input.y);
                    inputR = Vector2.zero;
                }

                if (disableCameraPivot && legacyMovement)
                {
                    input = new Vector2(inputR.x + input.x, input.y);
                }

                Vector2 normalizedInput = input.normalized;
                Vector2 normalizedInputR = inputR.normalized;

                if (Time.timeScale <= 0f)
                {
                    normalizedInput = Vector2.zero;
                }

                // Smooth speed adjustment
                float targetSpeed = state.currentSpeed * normalizedInput.magnitude;
                if (normalizedInputR != Vector2.zero)
                {
                    targetSpeed = state.currentSpeed * normalizedInputR.magnitude;
                }
                currentSpeed = Mathf.SmoothDamp(currentSpeed, targetSpeed, ref speedSmoothVelocity, 0.05f);

                // Movement handling
                if (legacyMovement)
                {
                    model.localEulerAngles = new Vector3(0f, 0f, 0f);

                    float speedModifier = 1f;

                    // "Legacy" movement
                    if (normalizedInput.y < 0 && (userInput.GetAxisButton("Strafe") && !userInput.usingController) && !disableCameraPivot)
                    {
                        // Move backwards with reduced speed if disable camera pivot is not true
                        speedModifier = backpedalSpeed;
                        model.localEulerAngles = new Vector3(0, -180f, 0);
                    }

                    if (normalizedInput != Vector2.zero || normalizedInputR != Vector2.zero)
                    {
                        float d = 0f;
                        if (disableCameraPivot || userInput.GetAxisButton("Strafe"))
                            d = Mathf.Atan2(normalizedInput.x, normalizedInput.y) * Mathf.Rad2Deg + cameraT.eulerAngles.y;
                        else
                            d = Mathf.Atan2(normalizedInputR.x, normalizedInput.y) * Mathf.Rad2Deg + cameraT.eulerAngles.y;

                        transform.eulerAngles = Vector3.up * d;
                    }

                    if (normalizedInput != Vector2.zero || normalizedInputR != Vector2.zero)
                    {
                        if (ShouldSlide())
                        {
                            return;
                        }

                        currentSpeed = Mathf.SmoothDamp(currentSpeed, targetSpeed, ref speedSmoothVelocity, 0.05f);
                        transform.Translate(transform.forward * currentSpeed * speedModifier * FightTimeline.deltaTime, Space.World);
                        ClampMovement();
                    }
                    else
                    {
                        currentSpeed = 0f;
                        transform.Translate(transform.forward * currentSpeed * FightTimeline.deltaTime, Space.World);
                        ClampMovement();
                    }

                    animator.SetFloat(animatorParameterTurning, 0f);
                }
                else
                {
                    model.localEulerAngles = new Vector3(0f, 0f, 0f);

                    float speedModifier = 1f;
                    bool turning = false;

                    // "Standard" movement
                    if (normalizedInput.y < 0)
                    {
                        // Move backwards with reduced speed
                        speedModifier = backpedalSpeed;
                    }

                    if (normalizedInput != Vector2.zero)
                    {
                        if (ShouldSlide())
                        {
                            return;
                        }

                        Vector3 moveDirection = (transform.forward * normalizedInput.y * speedModifier) + (transform.right * normalizedInput.x);
                        transform.Translate(moveDirection * currentSpeed * FightTimeline.deltaTime, Space.World);
                        ClampMovement();

                        if (input.x != 0f && !turning)
                        {
                            float animationRotation = input.x;

                            if (input.y > 0)
                                animationRotation *= 0.5f;

                            animator.SetFloat(animatorParameterTurning, animationRotation);
                            turning = true;
                        }
                    }

                    // Mouse-based rotation
                    if (Input.GetMouseButton(1))
                    {
                        // Rotate character to face the same way as the camera on the y-axis
                        Vector3 cameraForward = cameraT.forward;
                        cameraForward.y = 0; // Keep only the horizontal direction
                        transform.forward = cameraForward;
                    }
                    else if (Mathf.Abs(inputR.x) > 0.01f) // Character rotation with inputR.x
                    {
                        transform.Rotate(0, inputR.x * turnSpeed * FightTimeline.deltaTime, 0);
                        if (!turning)
                        {
                            float animationRotation = inputR.x * 0.5f;
                            animator.SetFloat(animatorParameterTurning, animationRotation);
                            turning = true;
                        }
                    }
                    else if (!turning)
                    {
                        animator.SetFloat(animatorParameterTurning, 0f);
                    }

                    // Auto camera adjustment
                    if (!Input.GetMouseButton(0) && !Input.GetMouseButton(1))
                    {
                        if (autoAdjustCamera == CameraAdjustMode.Always || (autoAdjustCamera == CameraAdjustMode.Moving && normalizedInput != Vector2.zero))
                        {
                            cameraScript.AutoAdjustCamera(cameraAutoAdjustSpeed);
                        }
                    }
                }

                // Animation updates
                float animationValue = ((state.HasEffect("Sprint") || state.HasEffect("Smudge")) ? 1f : 0.5f) * normalizedInput.magnitude;
                if (normalizedInputR != Vector2.zero && normalizedInput.y >= 0f && legacyMovement && !userInput.usingController)
                {
                    animationValue = ((state.HasEffect("Sprint") || state.HasEffect("Smudge")) ? 1f : 0.5f) * normalizedInputR.magnitude;
                }

                if ((legacyMovement && !disableCameraPivot && (userInput.GetAxisButton("Strafe") && !userInput.usingController)) || !legacyMovement)
                {
                    if (normalizedInput.y < 0)
                        animationValue *= -1f;
                }

                if (FightTimeline.deltaTime <= 0f)
                {
                    animationValue = 0f;
                }

                animator.SetFloat(animatorParameterSpeed, animationValue);
            }
            else if ((!state.dead || sliding) && !state.bound.value && enableInput && knockedBack)
            {
                animator.SetFloat(animatorParameterSpeed, 0f);
                transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, release);
                ClampMovement();
            }
            else
            {
                velocity = Vector3.zero;
                animator.SetFloat(animatorParameterSpeed, 0f);
            }
        }

        private void ClampMovement()
        {
            if (FightTimeline.Instance.isCircle)
            {
                // Circular boundary with center at arenaOffset
                float radius = 0;

                if (FightTimeline.Instance.arenaBounds.x != 0)
                {
                    radius = Mathf.Abs(FightTimeline.Instance.arenaBounds.x);
                }
                else if (FightTimeline.Instance.arenaBounds.z != 0)
                {
                    radius = Mathf.Abs(FightTimeline.Instance.arenaBounds.z);
                }

                Vector3 center = FightTimeline.Instance.arenaOffset;
                Vector3 horizontalPosition = new Vector3(transform.position.x, 0, transform.position.z); // Ignore Y-axis
                Vector3 horizontalCenter = new Vector3(center.x, 0, center.z);
                float distanceFromCenter = (horizontalPosition - horizontalCenter).magnitude;

                if (distanceFromCenter > radius)
                {
                    // Clamp position to the circle's edge on the horizontal plane
                    Vector3 direction = (horizontalPosition - horizontalCenter).normalized;
                    Vector3 clampedHorizontalPosition = horizontalCenter + direction * radius;
                    transform.position = new Vector3(
                        clampedHorizontalPosition.x,
                        transform.position.y, // Preserve current height
                        clampedHorizontalPosition.z
                    );
                }
            }

            // Clamp height (Y-axis) separately, regardless of circular or rectangular bounds
            float maxHeight = Mathf.Abs(FightTimeline.Instance.arenaBounds.y);
            if (maxHeight > 0)
            {
                transform.position = new Vector3(
                    transform.position.x,
                    Mathf.Clamp(transform.position.y, -maxHeight + FightTimeline.Instance.arenaOffset.y, maxHeight + FightTimeline.Instance.arenaOffset.y),
                    transform.position.z
                );
            }
            if (!FightTimeline.Instance.isCircle) // Rectangular bounds logic
            {
                Vector3 maxDistance = FightTimeline.Instance.arenaBounds;
                Vector3 offset = FightTimeline.Instance.arenaOffset;

                if (maxDistance.x != 0)
                {
                    transform.position = new Vector3(
                        Mathf.Clamp(transform.position.x, -Mathf.Abs(maxDistance.x) + offset.x, Mathf.Abs(maxDistance.x) + offset.x),
                        transform.position.y,
                        transform.position.z
                    );
                }
                if (maxDistance.z != 0)
                {
                    transform.position = new Vector3(
                        transform.position.x,
                        transform.position.y,
                        Mathf.Clamp(transform.position.z, -Mathf.Abs(maxDistance.z) + offset.z, Mathf.Abs(maxDistance.z) + offset.z)
                    );
                }
            }
        }

        private bool CanMove()
        {
            if (freezeMovement)
            {
                currentSpeed = 0;
                animator.SetFloat(animatorParameterSpeed, 0f);
                state.still = true;
                ClampMovement();
            }

            if (movementFrozen != freezeMovement)
            {
                movementFrozen = freezeMovement;
                if (freezeMovement)
                {
                    state.bound.SetFlag("freezeMovement", true);
                }
                else
                {
                    state.bound.RemoveFlag("freezeMovement");
                }
            }

            if (freezeMovement)
                return true;

            return false;
        }

        private bool ShouldSlide()
        {
            if (sliding && slideDistance > 0f)
            {
                knockedBack = true;
                Knockback(transform.forward * slideDistance, slideDuration, 0f, true);
                return true;
            }

            return false;
        }

        public void Knockback(Vector3 tp, float duration, float height, bool gravity)
        {
            if (FightTimeline.Instance != null && FightTimeline.Instance.disableKnockbacks)
                return;

            if (!state.HasEffect("Surecast"))
            {
                tm = 0f;
                release = duration;
                state.uncontrollable.SetFlag("knockback", true);
                targetPosition = transform.position + tp;
                animator.SetFloat(animatorParameterSpeed, 0f);
                knockedBack = true;
                preventGravity = !gravity;

                if (height > 0f) // Only do the vertical animation if there is a height component to the knockback
                {
                    float upDuration = duration * 0.4f; // 40% of total duration for going up
                    float downDuration = duration * 0.6f; // 60% of total duration for coming down
                    float peakDelay = upDuration * 0.6f; // Small delay at peak (60% into up phase)
                    
                    Vector3 up = new Vector3(0f, transform.position.y + height, 0f);
                    model.LeanMoveLocal(up, upDuration).setEase(LeanTweenType.easeOutQuad).setOnComplete(() => 
                        Utilities.FunctionTimer.Create(this, () => 
                            model.LeanMoveLocal(Vector3.zero, downDuration).setEase(LeanTweenType.easeInQuad), 
                            peakDelay, "player_knockback_fall_delay", false, true));
                }
            }
        }

        public void Init()
        {
            Vector3 finalSpawnOffset = spawnOffset;

            if (FightTimeline.Instance != null)
                finalSpawnOffset += FightTimeline.Instance.arenaOffset;

            transform.position = finalSpawnOffset;
            transform.eulerAngles = new Vector3(0f, Random.Range(0, 360), 0f);
            //cameraT.gameObject.GetComponent<ThirdPersonCamera>().RandomRotate();
        }

        public void SetAnimator(Animator animator)
        {
            this.animator = animator;
        }

        public void ResetController()
        {
            enableInput = true;
            freezeMovement = false;
            movementFrozen = false;
            knockedBack = false;
            sliding = false;
            jumping = false;
            jumpInput = false;
            jumpInputAvailable = true;
            preventGravity = false;
            wasPreventGravity = false;
            if (rb != null)
                rb.useGravity = true;
            tm = 0f;
            release = 0f;
            storedInput = Vector2.zero;
            storedInputR = Vector2.zero;
            Init();
            if (animator != null)
            {
                animator.SetBool(animatorParameterDead, false);
                animator.SetBool(animatorParameterIsCasting, false);
                animator.SetBool(animatorParameterDiamondback, false);
                animator.SetBool(animatorParameterDiamondback, false);
                animator.SetBool(animatorParamterSliding, false);
                animator.SetFloat(animatorParameterSpeed, 0f);
                animator.SetFloat(animatorParameterTurning, 0f);
                animator.ResetTrigger(animatorParameterJumping);
                animator.SetBool(animatorParameterIsJumping, false);
                animator.SetBool(animatorParameterIsDashing, false);
                animator.ResetTrigger(animatorParameterIsBlueCasting);
                animator.ResetTrigger(animatorParameterIsSwipe);
                animator.SetBool(animatorParameterIsActionLocked, false);
                animator.SetTrigger(animatorParameterReset);
            }
        }
    }
}
