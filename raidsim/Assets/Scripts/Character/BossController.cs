using System.Collections;
using System.Collections.Generic;
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

    private float turnSmoothVelocity;
    private float currentSpeed;
    private float targetRadius = 0f;

    void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        state = GetComponent<CharacterState>();
        controller = GetComponent<ActionController>();
    }

    void Update()
    {
        if (animator != null)
        {
            animator.SetBool("Dead", state.dead);
            animator.SetBool("Diamondback", state.HasEffect("Diamondback"));
        }

        if (currentSpeed > 0 && !controller.isCasting)
        {
            state.still = false;
        }
        else
        {
            state.still = true;
        }

        if (target != null && !state.dead && !controller.isCasting)
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
                }
                else
                {
                    transform.eulerAngles = Vector3.up * targetAngle;
                    turnSmoothVelocity = 0f;
                }
            }

            if (distanceToTarget > totalStoppingDistance)
            {
                float d = state.currentSpeed * normalized.magnitude;
                transform.Translate(transform.forward * d * FightTimeline.deltaTime, Space.World);
                currentSpeed = 0.5f * normalized.magnitude;
            }
            else
            {
                currentSpeed = 0;
                target = null;
            }

            if (animator != null)
            {
                if (Mathf.Abs(animator.GetFloat("Speed") - currentSpeed) > 0.01f)
                {
                    animator.SetFloat("Speed", currentSpeed);
                }
            }
        }
        else
        {
            if (lookTarget)
            {
                Vector3 direction = (new Vector3(lookTarget.position.x, 0f, lookTarget.position.z) - new Vector3(transform.position.x, 0f, transform.position.z)).normalized;
                Quaternion lookRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, turnSmoothTime * Time.deltaTime);
            }

            currentSpeed = 0;
            if (animator != null)
            {
                animator.SetFloat("Speed", 0f);
            }
        }
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
}
