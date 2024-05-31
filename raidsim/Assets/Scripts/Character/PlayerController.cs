using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    CharacterState state;

    public Vector3 maxDistance;
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
            animator.SetBool("Dead", state.dead);
            animator.SetBool("Diamondback", state.HasEffect("Diamondback"));

            if (currentSpeed > 0)
            {
                state.still = false;
            }
            else
            {
                state.still = true;
            }
        }

        tm += Time.deltaTime;
        if (tm > release)
        {
            state.uncontrollable = false;
        }
        if (!state.uncontrollable && !state.dead && !state.bound && enableInput)
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
            float target = state.speed * normalized.magnitude;
            if (normalized != Vector2.zero)
            {
                currentSpeed = Mathf.SmoothDamp(currentSpeed, target, ref speedSmoothVelocity, 0.05f);
                transform.Translate(transform.forward * currentSpeed * Time.deltaTime, Space.World);
                ClampMovement();
            }
            else
            {
                currentSpeed = 0f;
                transform.Translate(transform.forward * currentSpeed * Time.deltaTime, Space.World);
                ClampMovement();
            }
            float value = (state.HasEffect("Sprint") ? 1f : 0.5f) * normalized.magnitude;

            if (Time.timeScale <= 0f)
            {
                value = 0f;
            }

            animator.SetFloat("Speed", value);
        }
        else if (!state.dead && !state.bound && enableInput)
        {
            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, release);
        }
    }

    public void ClampMovement()
    {
        if (maxDistance.x != 0)
        {
            if (transform.position.x > Mathf.Abs(maxDistance.x))
            {
                transform.position = new Vector3(Mathf.Abs(maxDistance.x), transform.position.y, transform.position.z);
            }
            else if (transform.position.x < (-1 * maxDistance.x))
            {
                transform.position = new Vector3((-1 * maxDistance.x), transform.position.y, transform.position.z);
            }
        }
        if (maxDistance.y != 0)
        {
            if (transform.position.y > Mathf.Abs(maxDistance.y))
            {
                transform.position = new Vector3(transform.position.x, Mathf.Abs(maxDistance.y), transform.position.z);
            }
            else if (transform.position.y < (-1 * maxDistance.y))
            {
                transform.position = new Vector3(transform.position.x, (-1 * maxDistance.y), transform.position.z);
            }
        }
        if (maxDistance.z != 0)
        {
            if (transform.position.z > Mathf.Abs(maxDistance.z))
            {
                transform.position = new Vector3(transform.position.x, transform.position.y, Mathf.Abs(maxDistance.z));
            }
            else if (transform.position.z < (-1 * maxDistance.z))
            {
                transform.position = new Vector3(transform.position.x, transform.position.y, (-1 * maxDistance.z));
            }
        }
    }

    /*public void PhyDamage()
    {
        if (phyResis_timer > 0f | resis_timer > 0f)
        {
            text.SetActive(true);
        }
    }

    public void MagicDamage()
    {
        if (magicResis_timer > 0f | resis_timer > 0f)
        {
            text.SetActive(true);
        }
    }

    public void PhyResisDown(float sec)
    {
        phyResis_timer = sec;
    }

    public void MagicResisDown(float sec)
    {
        magicResis_timer = sec;
    }

    public void ResisDown(float sec)
    {
        resis_timer = sec;
    }

    public void Stack()
    {
        stack++;
        if (stack >= 2)
        {
            text.SetActive(true);
        }
    }*/

    public void Knockback(Vector3 tp, float duration)
    {
        if (!state.HasEffect("Surecast"))
        {
            tm = 0f;
            release = duration;
            state.uncontrollable = true;
            targetPosition = transform.position + tp;
            animator.SetFloat("Speed", 0f);
        }
    }

    public void Init()
    {
        transform.position = new Vector3(0f, 1.25f, 0f);
        transform.eulerAngles = new Vector3(0f, Random.Range(0, 360), 0f);
        cameraT.gameObject.GetComponent<ThirdPersonCamera>().RandomRotate();
    }
}
