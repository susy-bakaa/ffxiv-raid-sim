using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AIController : MonoBehaviour
{
    Animator animator;
    public CharacterState state { get; private set; }

    public BotTimeline botTimeline;
    private BotTimeline wasBotTimeline;
    public BotNode clockSpot;
    public float turnSmoothTime;
    public Vector3 spawnOffset = new Vector3(0f, 1.1f, 0f);
    public bool log;
    public bool freezeMovement = false;
    public bool sliding = false;
    public float slideDistance = 0f;
    public float slideDuration = 0.5f;
    private bool tweening = false;

    private float turnSmoothVelocity;
    private float currentSpeed;
    private float tm;
    private float release;
    private Vector3 targetPosition;
    private Vector3 velocity;
    private bool knockedback = false;
    private bool movementFrozen = false;

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

    void Awake()
    {
        animator = GetComponent<Animator>();
        state = GetComponent<CharacterState>();
        wasBotTimeline = botTimeline;
        botTimeline.bot = this;
    }

    void OnEnable()
    {
        Init();
        knockedback = false;
        freezeMovement = false;
        movementFrozen = false;
        sliding = false;
        tweening = false;
    }

    void Update()
    {
        if (Time.deltaTime > 0)
        {
            animator.SetBool(animatorParameterDead, state.dead);
            animator.SetBool(animatorParameterDiamondback, state.HasEffect("Diamondback"));
            animator.SetBool(animatorParamterSliding, (sliding && knockedback));

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
        }

        tm += FightTimeline.deltaTime;
        if (tm > release || state.dead)
        {
            tm = release + 1f;
            state.uncontrollable.RemoveFlag("knockback");
            knockedback = false;
            tweening = false;
        }
        if (botTimeline != null && botTimeline.currentTarget != null && !state.dead && !state.bound.value && !state.uncontrollable.value && !knockedback)
        {
            Vector3 vector = botTimeline.currentTarget.position - transform.position;
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

            float stoppingDistance = 0.05f; // Adjust as needed
            if (distanceToTarget > stoppingDistance)
            {
                if (sliding && slideDistance > 0f && botTimeline != null && botTimeline.currentTarget != null)
                {
                    tm = 0f;
                    release = slideDuration;
                    state.uncontrollable.SetFlag("knockback", true);
                    targetPosition = botTimeline.currentTarget.position;
                    animator.SetFloat(animatorParameterSpeed, 0f);
                    knockedback = true;
                    return;
                }

                float d = state.currentSpeed * normalized.magnitude;
                transform.Translate(transform.forward * d * FightTimeline.deltaTime, Space.World);
                currentSpeed = 0.5f * normalized.magnitude;
                ClampMovement();
            }
            else
            {
                if (botTimeline.TeleportAfterClose)
                    transform.position = botTimeline.currentTarget.position;
                currentSpeed = 0;
            }

            if (Mathf.Abs(animator.GetFloat(animatorParameterSpeed) - currentSpeed) > 0.01f) // Adjust threshold as needed
            {
                animator.SetFloat(animatorParameterSpeed, currentSpeed);
            }
        }
        else if (!state.dead && !state.bound.value && knockedback)
        {
            animator.SetFloat(animatorParameterSpeed, 0f);
            if (!sliding)
            {
                transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, release);
            }
            else if (sliding && !tweening)
            {
                tweening = true;
                LeanTween.move(gameObject, targetPosition, release);
            }
            ClampMovement();
        }
        else
        {
            currentSpeed = 0;
            animator.SetFloat(animatorParameterSpeed, 0f);
        }
    }

    private void ClampMovement()
    {
        if (FightTimeline.Instance.isCircle)
        {
            // Circular boundary with center at (0,0,0)
            float radius = 0;

            if (FightTimeline.Instance.arenaBounds.x != 0)
            {
                radius = Mathf.Abs(FightTimeline.Instance.arenaBounds.x);
            }
            else if (FightTimeline.Instance.arenaBounds.z != 0)
            {
                radius = Mathf.Abs(FightTimeline.Instance.arenaBounds.z);
            }

            Vector3 horizontalPosition = new Vector3(transform.position.x, 0, transform.position.z); // Ignore Y-axis
            float distanceFromCenter = horizontalPosition.magnitude;

            if (distanceFromCenter > radius)
            {
                // Clamp position to the circle's edge on the horizontal plane
                Vector3 clampedHorizontalPosition = horizontalPosition.normalized * radius;
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
                Mathf.Clamp(transform.position.y, -maxHeight, maxHeight),
                transform.position.z
            );
        }
        else if (!FightTimeline.Instance.isCircle) // Rectangular bounds logic
        {
            Vector3 maxDistance = FightTimeline.Instance.arenaBounds;

            if (maxDistance.x != 0)
            {
                transform.position = new Vector3(
                    Mathf.Clamp(transform.position.x, -Mathf.Abs(maxDistance.x), Mathf.Abs(maxDistance.x)),
                    transform.position.y,
                    transform.position.z
                );
            }
            if (maxDistance.z != 0)
            {
                transform.position = new Vector3(
                    transform.position.x,
                    transform.position.y,
                    Mathf.Clamp(transform.position.z, -Mathf.Abs(maxDistance.z), Mathf.Abs(maxDistance.z))
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

    public void Knockback(Vector3 tp, float duration)
    {
        if (!state.HasEffect("Surecast"))
        {
            tm = 0f;
            release = duration;
            state.uncontrollable.SetFlag("knockback", true);
            targetPosition = transform.position + tp;
            animator.SetFloat(animatorParameterSpeed, 0f);
            knockedback = true;
        }
    }

    public void Init()
    {
        transform.position = new Vector3(spawnOffset.x + Random.Range(-1.5f, 1.5f), spawnOffset.y, spawnOffset.z + Random.Range(-1.5f, 1.5f));
        transform.eulerAngles = new Vector3(0f, Random.Range(-360f, 360f), 0f);
    }

    public void SetAnimator(Animator animator)
    {
        this.animator = animator;
    }

    public void ResetController()
    {
        botTimeline = wasBotTimeline;
        botTimeline.bot = this;
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
        OnEnable();
    }
}