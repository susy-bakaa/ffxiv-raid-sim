using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    CharacterState state;
    ActionController controller;

    //public float walkSpeed = 6.3f;
    //public float runSpeed = 9.45f;
    //private bool controllable;
    public float turnSmoothTime;
    private float turnSmoothVelocity;
    private float tm;
    private float release;
    //public bool sp_on;
    //public bool kb_on;
    //private bool sprint;
    //private bool knockback;
    //public float sp_timer;
    //public float kb_timer;
    //public GameObject kb_image;
    //public GameObject sp_image;
    private Animator animator;
    public Transform cameraT;
    //public int stack;
    public Vector3 targetPosition;
    public Vector3 velocity;
    private float currentSpeed;
    private float speedSmoothVelocity;
    //public GameObject text;
    //public float magicResis_timer;
    //public float phyResis_timer;
    //public float resis_timer;
    //public GameObject vulImage;
    //public GameObject phyVulImage;
    //public GameObject magicVulImage;

    public GameObject bubbleShield;

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
        if (controller == null)
        {
            if (TryGetComponent(out ActionController controller))
            {
                this.controller = controller;
            }
            else
            {
                Debug.LogError($"ActionController script not found for PlayerController ({this})!");
            }
        }

        if (KeyBind.Keys == null)
        {
            KeyBind.Keys = new Dictionary<string, KeyBind>
            {
                { "SprintKey", new KeyBind(KeyCode.Alpha1, false, false, false) },
                { "SwiftcastKey", new KeyBind(KeyCode.Alpha2, false, false, false) },
                { "SurecastKey", new KeyBind(KeyCode.Alpha3, false, false, false) },
                { "DiamondbackKey", new KeyBind(KeyCode.Alpha4, false, false, false) },
                { "MightyguardKey", new KeyBind(KeyCode.Alpha5, false, false, false) },
                { "WhitewindKey", new KeyBind(KeyCode.Alpha6, false, false, false) }
            };
        }

        animator = GetComponent<Animator>();
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
        if (Input.GetKeyDown(KeyCode.F1))
        {
            state.ModifyHealth(-1000);
        }

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

        tm += Time.deltaTime;
        if (tm > release)
        {
            state.uncontrollable = false;
        }
        if (!state.uncontrollable && !state.dead && !state.bound)
        {
            Vector2 vector = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            Vector2 normalized = vector.normalized;
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
            }
            else
            {
                currentSpeed = 0f;
                transform.Translate(transform.forward * currentSpeed * Time.deltaTime, Space.World);
            }
            float value = (state.HasEffect("Sprint") ? 1f : 0.5f) * normalized.magnitude;
            animator.SetFloat("Speed", value);
        }
        else if (!state.dead && !state.bound)
        {
            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, release);
        }

        if (BindedKey(KeyBind.Keys["SurecastKey"])) // Add check for cooldown bruh
        {
            controller.PerformAction("Surecast");
            //state.AddEffect("Surecast");
        }
        if (BindedKey(KeyBind.Keys["SprintKey"])) // Add check for cooldown bruh
        {
            controller.PerformAction("Sprint");
            //state.AddEffect("Sprint");
        }
        if (BindedKey(KeyBind.Keys["DiamondbackKey"])) // Add check for cooldown bruh
        {
            controller.PerformAction("Diamondback");
            //state.AddEffect("Diamondback");
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

    public bool BindedKey(KeyBind keyBind)
    {
        return (keyBind.keyCode != KeyCode.None && (Input.GetKeyDown(keyBind.keyCode) & Input.GetKey(KeyCode.LeftShift) == keyBind.shift & Input.GetKey(KeyCode.LeftAlt) == keyBind.alt & Input.GetKey(KeyCode.LeftControl) == keyBind.control)) || (keyBind.mouseButton != -1 && (Input.GetMouseButton(keyBind.mouseButton) & Input.GetKey(KeyCode.LeftShift) == keyBind.shift & Input.GetKey(KeyCode.LeftAlt) == keyBind.alt & Input.GetKey(KeyCode.LeftControl) == keyBind.control));
    }

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
        transform.position = new Vector3(0f, 0f, 0f);
        transform.eulerAngles = new Vector3(0f, Random.Range(0, 360), 0f);
        //sp_timer = 0f;
        //kb_timer = 0f;
        //sp_on = true;
        //kb_on = true;
        //stack = 0;
        //text.SetActive(false);
        //magicResis_timer = 0f;
        //phyResis_timer = 0f;
        //resis_timer = 0f;
        cameraT.gameObject.GetComponent<ThirdPersonCamera>().RandomRotate();
    }
}
