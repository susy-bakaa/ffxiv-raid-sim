// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using System.Collections.Generic;
using dev.susybaka.raidsim.Actions;
using dev.susybaka.raidsim.Targeting;
using dev.susybaka.Shared;
using UnityEngine;
using static dev.susybaka.raidsim.Core.GlobalData;
using static dev.susybaka.raidsim.Core.GlobalData.Flag;

namespace dev.susybaka.raidsim.Characters
{
    public class BossController : MonoBehaviour
    {
        Animator animator;
        public CharacterState state { get; private set; }
        public ActionController actions { get; private set; }
        public PlayerController playerControl { get; private set; }
        public AIController aiControl { get; private set; }
        private Vector3 originalPosition;
        private Quaternion originalRotation;
        private Vector3 originalScale;

        public Flag hasControl = new Flag("hasControl", new List<FlagValue> { new FlagValue("base", true) }, AggregateLogic.AnyTrue);
        private Flag wasHasControl;
        private bool wasControl = true;
        public Transform target;
        private Transform wasTarget;
        public Transform lookTarget;
        private Transform wasLookTarget;
        public bool includeCollider = true;
        public float stoppingDistance = 3f;
        public float turnSmoothTime;
        private float wasTurnSmoothTime;

        public float turningSpeedMultiplier = 300f; // Multiplier to amplify turning speed, adjust as needed
        private float wasTurningSpeedMultiplier;
        public float turningSmoothTime = 0.1f;   // Smoothing time for turning speed (adjust as needed)
        private float wasTurningSmoothTime;

        private float turningVelocity = 0f;    // Smoothing velocity for turning speed

        private float turnSmoothVelocity;
        private float targetRadius = 0f;

        public float acceleration = 1f;  // Adjust for how quickly you want the boss to accelerate
        private float wasAcceleration;
        public float deceleration = 2f;  // Adjust for how quickly you want the boss to decelerate
        private float wasDeceleration;
        public float speedThreshold = 0.05f; // Threshold to lock speeds when close enough
        private float wasSpeedThreshold;

        private float currentSpeed = 0f; // Track the current speed
        private float targetSpeed = 0f;  // The speed we want to reach

        private float previousYRotation = 0f;  // To store the previous frame's Y rotation
        private float turningSpeed = 0f;       // Turning speed to send to the animator

        public bool ignoreCasting = false;
        private bool wasIgnoreCasting;

        private int animatorParameterDead = Animator.StringToHash("Dead");
        private int animatorParameterSpeed = Animator.StringToHash("Speed");
        private int animatorParameterTurning = Animator.StringToHash("Turning");
        private int animatorParameterDiamondback = Animator.StringToHash("Diamondback");
        private int animatorParameterActionLocked = Animator.StringToHash("ActionLocked");

        private void Awake()
        {
            animator = GetComponentInChildren<Animator>();
            state = GetComponent<CharacterState>();
            actions = GetComponent<ActionController>();
            playerControl = GetComponent<PlayerController>();
            aiControl = GetComponent<AIController>();

            wasHasControl = new Flag(hasControl);
            wasControl = !hasControl.value; // On purpose reverse this so one update will trigger immidiately if the boss starts with control
            wasTarget = target;
            wasLookTarget = lookTarget;
            wasTurnSmoothTime = turnSmoothTime;
            wasTurningSpeedMultiplier = turningSpeedMultiplier;
            wasTurningSmoothTime = turningSmoothTime;
            wasAcceleration = acceleration;
            wasDeceleration = deceleration;
            wasSpeedThreshold = speedThreshold;
            wasIgnoreCasting = ignoreCasting;

            originalPosition = transform.position;
            originalRotation = transform.rotation;
            originalScale = transform.localScale;
        }

        private void Update()
        {
            if (state == null)
                return;

            if (hasControl.value != wasControl)
            {
                wasControl = hasControl.value;

                // Set boss controlled flag for both player and AI controllers
                if (hasControl.value)
                {
                    if (playerControl != null)
                        playerControl.bossControlled.SetFlag($"bossController_{gameObject.name}", true);
                    if (aiControl != null)
                        aiControl.bossControlled.SetFlag($"bossController_{gameObject.name}", true);
                }
                else
                {
                    if (playerControl != null)
                        playerControl.bossControlled.SetFlag($"bossController_{gameObject.name}", false);
                    if (aiControl != null)
                        aiControl.bossControlled.SetFlag($"bossController_{gameObject.name}", false);
                }
            }

            if (!hasControl.value)
            {
                // Ensure no turning value remains in the animator when the boss is not under control,
                // As player or AI controllers do not use the turning value, and it can cause animation issues if left set.
                animator.SetFloat(animatorParameterTurning, 0f);
                return;
            }

            if (animator != null && state != null)
            {
                animator.SetBool(animatorParameterDead, state.dead);
                animator.SetBool(animatorParameterDiamondback, state.HasEffect("Diamondback"));
            }
            else
            {
                if (Utilities.RateLimiter(57))
                {
                    if (animator == null)
                        animator = GetComponentInChildren<Animator>();
                    if (state == null)
                        state = GetComponent<CharacterState>();
                    if (actions == null)
                        actions = GetComponent<ActionController>();
                }
            }

            if (target != null && !state.dead && !state.stunned.value && !state.bound.value && ((!ignoreCasting && !actions.isCasting && !animator.TryGetBool(animatorParameterActionLocked)) || ignoreCasting))
            {
                Vector3 vector = target.position - transform.position;
                Vector2 vector2 = new Vector2(vector.x, vector.z);
                Vector2 normalized = vector2.normalized;
                float distanceToTarget = vector.magnitude;

                float totalStoppingDistance = stoppingDistance + targetRadius;

                if (normalized != Vector2.zero)
                {
                    float targetAngle = Mathf.Atan2(normalized.x, normalized.y) * Mathf.Rad2Deg;
                    float angleDifference = Mathf.Abs(Mathf.DeltaAngle(transform.eulerAngles.y, targetAngle));

                    if (angleDifference > 1f) // Adjust threshold as needed
                    {
                        transform.eulerAngles = Vector3.up * Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);

                        // Calculate turning speed (directional value -1 to 1)
                        float currentYRotation = transform.eulerAngles.y;
                        float deltaY = Mathf.DeltaAngle(previousYRotation, currentYRotation); // Difference between last frame's and current Y rotation
                        float rawTurningSpeed = Mathf.Clamp(deltaY * turningSpeedMultiplier / 180f, -1f, 1f); // Amplify and normalize to range -1 to 1

                        // Smooth the turning speed
                        turningSpeed = Mathf.SmoothDamp(turningSpeed, rawTurningSpeed, ref turningVelocity, turningSmoothTime);

                        previousYRotation = currentYRotation; // Update previous rotation
                    }
                    else
                    {
                        transform.eulerAngles = Vector3.up * targetAngle;
                        turnSmoothVelocity = 0f;
                        turningSpeed = 0f;  // No turning
                    }
                }

                // Set targetSpeed based on distance to target
                if (distanceToTarget > totalStoppingDistance)
                {
                    targetSpeed = state.currentSpeed; // Full speed ahead
                }
                else
                {
                    targetSpeed = 0; // Stop when within range
                    target = null;
                }

                // Smooth acceleration or deceleration
                if (Mathf.Abs(currentSpeed - targetSpeed) < speedThreshold)
                {
                    // Lock the speed to targetSpeed when close enough
                    currentSpeed = targetSpeed;
                }
                else if (currentSpeed < targetSpeed)
                {
                    // Accelerate smoothly
                    currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, acceleration * Time.deltaTime);
                }
                else if (currentSpeed > targetSpeed)
                {
                    // Decelerate smoothly
                    currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, deceleration * Time.deltaTime);
                }

                // Move the boss
                if (currentSpeed > 0)
                {
                    Vector3 moveVector = transform.forward * currentSpeed * Time.deltaTime;
                    transform.Translate(moveVector, Space.World);
                }

                // Set animator variables
                if (animator != null)
                {
                    if (Mathf.Abs(animator.GetFloat(animatorParameterSpeed) - currentSpeed) > 0.01f)
                    {
                        animator.SetFloat(animatorParameterSpeed, currentSpeed);
                    }
                    animator.SetFloat(animatorParameterTurning, turningSpeed); // Update turning value in the animator
                }
            }
            else
            {
                // No target, decelerate to a stop
                targetSpeed = 0;
                currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, deceleration * Time.deltaTime);

                // Even when the boss is not moving, check for rotation and calculate turning speed
                if (lookTarget != null && !state.dead && ((!ignoreCasting && !actions.isCasting && !animator.TryGetBool(animatorParameterActionLocked)) || ignoreCasting))
                {
                    Vector3 direction = (new Vector3(lookTarget.position.x, 0f, lookTarget.position.z) - new Vector3(transform.position.x, 0f, transform.position.z)).normalized;
                    Quaternion lookRotation = Quaternion.identity;
                    // This gets rid of the Unity: 'Look rotation viewing vector is zero' log message spam
                    if (direction != Vector3.zero)
                    {
                        lookRotation = Quaternion.LookRotation(direction);
                    }
                    transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, turnSmoothTime * Time.deltaTime);

                    // Calculate turning speed (directional value -1 to 1)
                    float currentYRotation = transform.eulerAngles.y;
                    float deltaY = Mathf.DeltaAngle(previousYRotation, currentYRotation); // Difference between last frame's and current Y rotation
                    float rawTurningSpeed = Mathf.Clamp(deltaY * turningSpeedMultiplier / 180f, -1f, 1f); // Amplify and normalize to range -1 to 1

                    // Smooth the turning speed
                    turningSpeed = Mathf.SmoothDamp(turningSpeed, rawTurningSpeed, ref turningVelocity, turningSmoothTime);

                    previousYRotation = currentYRotation; // Update previous rotation
                }
                else
                {
                    turningSpeed = 0f; // No turning when there's no look target
                }

                if (animator != null)
                {
                    animator.SetFloat(animatorParameterSpeed, currentSpeed);
                    animator.SetFloat(animatorParameterTurning, turningSpeed); // Update turning when stationary
                }
            }
        }

        public void ResetController()
        {
            target = wasTarget;
            lookTarget = wasLookTarget;
            turningSmoothTime = wasTurningSmoothTime;
            turningSpeedMultiplier = wasTurningSpeedMultiplier;
            turnSmoothTime = wasTurnSmoothTime;
            acceleration = wasAcceleration;
            deceleration = wasDeceleration;
            speedThreshold = wasSpeedThreshold;
            ignoreCasting = wasIgnoreCasting;
            hasControl = new Flag(wasHasControl);
            // Only reset position here if this controller has control and there are no player or AI controllers,
            // to avoid conflicts with player/AI control resets
            if (hasControl.value && aiControl == null && playerControl == null)
            {
                transform.position = originalPosition;
                transform.rotation = originalRotation;
                transform.localScale = originalScale;
            }
        }

        public void SetAnimator(Animator animator)
        {
            this.animator = animator;
        }

        public Animator GetAnimator() { return this.animator; }

        public void SetTarget()
        {
            SetTarget((Transform)null);
        }


        public void SetTarget(TargetNode targetNode)
        {
            if (targetNode != null)
            {
                SetTarget(targetNode.transform);
            }
            else
            {
                SetTarget((Transform)null);
            }
        }

        public void SetTarget(Transform target)
        {
            this.target = target;

            if (includeCollider && target != null)
            {
                targetRadius = 0f; // Reset radius

                if (target.TryGetComponent(out Collider targetCollider))
                {
                    if (targetCollider is SphereCollider sphereCollider)
                    {
                        targetRadius = sphereCollider.radius * sphereCollider.transform.localScale.x;
                    }
                    else if (targetCollider is CapsuleCollider capsuleCollider)
                    {
                        targetRadius = capsuleCollider.radius * capsuleCollider.transform.localScale.x;
                    }
                }
            }
            else
            {
                targetRadius = 0f; // No collider, so no radius offset
            }
        }

        public void SetRotationTarget()
        {
            SetRotationTarget((Transform)null);
        }

        public void SetRotationTarget(TargetNode targetNode)
        {
            if (targetNode != null)
            {
                SetRotationTarget(targetNode.transform);
            }
            else
            {
                SetRotationTarget((Transform)null);
            }
        }

        public void SetRotationTarget(Transform lookTarget)
        {
            this.lookTarget = lookTarget;
        }

        public void SetTurnSmoothTime(float value)
        {
            turnSmoothTime = value;
        }

        public void SetStoppingDistance(float distance)
        {
            stoppingDistance = distance;
        }

        public void SetLookRotation(TargetController targetController)
        {
            if (targetController != null && targetController.currentTarget != null)
            {
                SetLookRotation(targetController.currentTarget);
            }
        }

        public void SetLookRotation(TargetNode targetNode)
        {
            if (targetNode != null)
            {
                SetLookRotation(targetNode.transform);
            }
        }

        public void SetLookRotation(Transform target)
        {
            Vector3 eulers = transform.eulerAngles;
            transform.LookAt(target);
            transform.eulerAngles = new Vector3(eulers.x, transform.eulerAngles.y, eulers.z);
        }

        public void SetLookRotation()
        {
            if (lookTarget != null || target != null)
            {
                Transform lookAt = null;

                if (lookTarget != null)
                    lookAt = lookTarget;
                else if (target != null)
                    lookAt = target;

                Vector3 eulers = transform.eulerAngles;
                transform.LookAt(lookAt);
                transform.eulerAngles = new Vector3(eulers.x, transform.eulerAngles.y, eulers.z);
            }
        }
    }
}