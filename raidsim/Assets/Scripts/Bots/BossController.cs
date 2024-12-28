using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using Unity.VisualScripting.Dependencies.NCalc;
using UnityEngine;

public class BossController : MonoBehaviour
{
    Animator animator;
    public CharacterState state { get; private set; }
    public ActionController controller { get; private set; }

    public Transform target;
    public Transform lookTarget;
    public bool includeCollider = true;
    public float stoppingDistance = 3f;
    public float turnSmoothTime;

    public float turningSpeedMultiplier = 300f; // Multiplier to amplify turning speed, adjust as needed
    public float turningSmoothTime = 0.1f;   // Smoothing time for turning speed (adjust as needed)

    private float turningVelocity = 0f;    // Smoothing velocity for turning speed

    private float turnSmoothVelocity;
    private float targetRadius = 0f;

    public float acceleration = 1f;  // Adjust for how quickly you want the boss to accelerate
    public float deceleration = 2f;  // Adjust for how quickly you want the boss to decelerate
    public float speedThreshold = 0.05f; // Threshold to lock speeds when close enough

    private float currentSpeed = 0f; // Track the current speed
    private float targetSpeed = 0f;  // The speed we want to reach

    private float previousYRotation = 0f;  // To store the previous frame's Y rotation
    private float turningSpeed = 0f;       // Turning speed to send to the animator

    public bool ignoreCasting = false;

    private int animatorParameterDead = Animator.StringToHash("Dead");
    private int animatorParameterSpeed = Animator.StringToHash("Speed");
    private int animatorParameterTurning = Animator.StringToHash("Turning");
    private int animatorParameterDiamondback = Animator.StringToHash("Diamondback");
    private int animatorParameterActionLocked = Animator.StringToHash("ActionLocked");

    void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        state = GetComponent<CharacterState>();
        controller = GetComponent<ActionController>();
    }

    void Update()
    {
        if (state == null)
            return;

        if (animator != null && state != null)
        {
            animator.SetBool(animatorParameterDead, state.dead);
            animator.SetBool(animatorParameterDiamondback, state.HasEffect("Diamondback"));
        }

        if (target != null && !state.dead && ((!ignoreCasting && !controller.isCasting && !animator.TryGetBool(animatorParameterActionLocked)) || ignoreCasting))
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
            if (lookTarget != null && !state.dead && ((!ignoreCasting && !controller.isCasting && !animator.TryGetBool(animatorParameterActionLocked)) || ignoreCasting))
            {
                Vector3 direction = (new Vector3(lookTarget.position.x, 0f, lookTarget.position.z) - new Vector3(transform.position.x, 0f, transform.position.z)).normalized;
                Quaternion lookRotation = Quaternion.LookRotation(direction);
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

    public void SetAnimator(Animator animator)
    {
        this.animator = animator;
    }

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
}
