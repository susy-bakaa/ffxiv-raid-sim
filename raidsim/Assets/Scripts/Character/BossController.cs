using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossController : MonoBehaviour
{
    Animator animator;
    public CharacterState state { get; private set; }

    //public BotTimeline bossTimeline;
    public Transform target;
    public Transform lookTarget;
    //public BotNode startSpot;
    public float turnSmoothTime;
    private float turnSmoothVelocity;

    float currentSpeed;

    void Awake()
    {
        animator = GetComponent<Animator>();
        state = GetComponent<CharacterState>();
        //bossTimeline.boss = this;
    }

    void OnEnable()
    {
        Init();
    }

    void Update()
    {
        if (animator != null)
        {
            animator.SetBool("Dead", state.dead);
            animator.SetBool("Diamondback", state.HasEffect("Diamondback"));
        }

        if (currentSpeed > 0)
        {
            state.still = false;
        }
        else
        {
            state.still = true;
        }

        if (/*bossTimeline != null && bossTimeline.currentTarget != null &&*/target != null && !state.dead)
        {
            Vector3 vector = target.position - transform.position;//bossTimeline.currentTarget.position - transform.position;
            Vector2 vector2 = new Vector2(vector.x, vector.z);
            Vector2 normalized = vector2.normalized;
            float distanceToTarget = vector.magnitude;

            if (normalized != Vector2.zero)
            {
                float targetAngle = Mathf.Atan2(normalized.x, normalized.y) * 57.29578f;
                float angleDifference = Mathf.Abs(Mathf.DeltaAngle(transform.eulerAngles.y, targetAngle));

                // Only apply smoothing if the angle difference is significant
                if (angleDifference > 1f) // Adjust threshold as needed
                {
                    transform.eulerAngles = Vector3.up * Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
                }
                else
                {
                    // Directly set the angle if it is close enough
                    transform.eulerAngles = Vector3.up * targetAngle;
                    turnSmoothVelocity = 0f; // Reset the smoothing velocity
                }
            }

            float stoppingDistance = 0.1f; // Adjust as needed
            if (distanceToTarget > stoppingDistance)
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
                if (Mathf.Abs(animator.GetFloat("Speed") - currentSpeed) > 0.01f) // Adjust threshold as needed
                {
                    animator.SetFloat("Speed", currentSpeed);
                }
            }
        }
        else
        {
            if (lookTarget)
            {
                Vector3 direction = (lookTarget.position - transform.position).normalized;
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

    public void SetRotationTarget()
    {
        SetRotationTarget((Transform)null);
    }

    public void SetRotationTarget(TargetNode target)
    {
        if (target != null)
        {
            SetRotationTarget(target.transform);
        }
        else
        {
            SetRotationTarget((Transform)null);
        }
    }

    public void SetRotationTarget(Transform target)
    {
        lookTarget = target;
    }

    public void Init()
    {
        //transform.position = new Vector3(Random.value * 3f - 1.5f, 0f, Random.value * 3f - 1.5f);
        //transform.position = new Vector3(Random.Range(-1.5f, 1.5f), 1.1f, Random.Range(-1.5f, 1.5f));
        //transform.eulerAngles = new Vector3(0f, Random.Range(-360f, 360f), 0f);
    }
}
