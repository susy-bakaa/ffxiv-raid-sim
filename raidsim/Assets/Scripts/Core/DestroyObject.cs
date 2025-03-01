using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DestroyObject : MonoBehaviour
{
    CharacterState state;

    public float lifetime = 1f;
    public bool disableInstead = false;
    public bool disabledOnStart = false;
    public bool log = false;

    public UnityEvent onDestroy;

    private float life;
    private float triggerLife;

    private bool triggered;

    void Awake()
    {
        if (state == null)
            state = GetComponent<CharacterState>();

        life = lifetime;
    }

    void Start()
    {
        if (disableInstead && disabledOnStart)
        {
            DisableObjectInternal();
            if (FightTimeline.Instance != null)
                FightTimeline.Instance.onReset.AddListener(() => { DisableObjectInternal(); ResetState(); });
        }
        else if (disableInstead && !disabledOnStart)
        {
            if (FightTimeline.Instance != null)
                FightTimeline.Instance.onReset.AddListener(ResetState);
        }
    }

    void Update()
    {
        if (triggered)
            return;

        float deltaTime;

        if (FightTimeline.Instance != null)
            deltaTime = FightTimeline.deltaTime;
        else
            deltaTime = Time.deltaTime;

        if (triggerLife != 0f)
        {
            triggerLife -= deltaTime;

            if (triggerLife <= 0f)
            {
                triggerLife = 0f;
                if (!disableInstead)
                {
                    Destroy(gameObject);
                }
                else
                {
                    DisableObject();
                }
            }
        }

        if (lifetime > 0f)
        {
            life -= deltaTime;

            if (life <= 0f)
            {
                life = 0f;
                if (!disableInstead)
                {
                    Destroy(gameObject);
                }
                else
                {
                    DisableObject();
                }
            }
        }
    }

    public void TriggerDestruction(float delay)
    {
        triggerLife = delay;
    }

    public void OnDestroy()
    {
        triggered = true;
        onDestroy.Invoke();
        if (log)
            Debug.Log($"{this} was destroyed!");
    }

    private void DisableObject()
    {
        triggered = true;
        DisableObjectInternal();
    }

    public void ResetState()
    {
        triggered = false;
        life = lifetime;
        triggerLife = 0f;
    }

    private void DisableObjectInternal()
    {
        onDestroy.Invoke();
        if (state == null)
            gameObject.SetActive(false);
        else
            state.ToggleState(false);
    }
}