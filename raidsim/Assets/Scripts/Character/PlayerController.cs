using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    CharacterState state;

    public float ySpawnOffset = 1.25f;
    public float turnSmoothTime;
    private float turnSmoothVelocity;
    private float tm;
    private float release;
    private Animator animator;
    public Transform cameraT;
    public Vector3 targetPosition;
    public Vector3 velocity;
    private float currentSpeed;
    private float speedSmoothVelocity;

    public GameObject bubbleShield;
    public BotNode clockSpot;
    public bool enableInput = true;
    public bool legacyMovement = true;
    public bool freezeMovement = false;
    private bool movementFrozen = false;
    private bool knockedBack = false;

    private int animatorParameterDead = Animator.StringToHash("Dead");
    private int animatorParameterSpeed = Animator.StringToHash("Speed");
    private int animatorParameterDiamondback = Animator.StringToHash("Diamondback");

    void Awake()
    {
        if (state == null)
        {
            if (TryGetComponent(out CharacterState state))
            {
                this.state = state;
            }
            else
            {
                Debug.LogError($"CharacterState script not found for PlayerController ({this})!");
            }
        }

        animator = GetComponent<Animator>();
    }

    void OnEnable()
    {
        freezeMovement = false;
        movementFrozen = false;
        Init();
    }

    /*void Start()
    {
        sp_on = true;
        kb_on = true;
        sprint = false;
        knockback = false;
        sp_timer = 0f;
        kb_timer = 0f;
        stack = 0;
        controllable = true;
    }*/

    void Update()
    {
        if (Time.timeScale > 0f)
        {
            animator.SetBool(animatorParameterDead, state.dead);
            animator.SetBool(animatorParameterDiamondback, state.HasEffect("Diamondback"));

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
            velocity = Vector3.zero;
            knockedBack = false;
        }
        if (!state.uncontrollable.value && !state.dead && !state.bound.value && enableInput)
        {
            Vector2 vector = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            Vector2 normalized = vector.normalized;

            if (Time.timeScale <= 0f)
            {
                normalized = Vector2.zero;
            }

            if (normalized != Vector2.zero)
            {
                float d = Mathf.Atan2(normalized.x, normalized.y) * 57.29578f + cameraT.eulerAngles.y;
                transform.eulerAngles = Vector3.up * d;
            }
            float target = state.currentSpeed * normalized.magnitude;
            if (normalized != Vector2.zero)
            {
                currentSpeed = Mathf.SmoothDamp(currentSpeed, target, ref speedSmoothVelocity, 0.05f);
                transform.Translate(transform.forward * currentSpeed * FightTimeline.deltaTime, Space.World);
                ClampMovement();
            }
            else
            {
                currentSpeed = 0f;
                transform.Translate(transform.forward * currentSpeed * FightTimeline.deltaTime, Space.World);
                ClampMovement();
            }
            float value = (state.HasEffect("Sprint") ? 1f : 0.5f) * normalized.magnitude;

            if (FightTimeline.deltaTime <= 0f)
            {
                value = 0f;
            }

            animator.SetFloat(animatorParameterSpeed, value);
        }
        else if (!state.dead && !state.bound.value && enableInput && knockedBack)
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
            knockedBack = true;
        }
    }

    public void Init()
    {
        transform.position = new Vector3(0f, ySpawnOffset, 0f);
        transform.eulerAngles = new Vector3(0f, Random.Range(0, 360), 0f);
        cameraT.gameObject.GetComponent<ThirdPersonCamera>().RandomRotate();
    }
}
