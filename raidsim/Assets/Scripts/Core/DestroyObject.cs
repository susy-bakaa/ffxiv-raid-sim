using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyObject : MonoBehaviour
{
    CharacterState state;

    public float lifetime = 1f;
    public bool disableInstead = false;
    public bool log = false;

    private float life;
    private float triggerLife;

    void Awake()
    {
        if (state == null)
            state = GetComponent<CharacterState>();

        life = lifetime;
    }

    void Update()
    {
        if (triggerLife != 0f)
        {
            triggerLife -= FightTimeline.deltaTime;

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
            life -= FightTimeline.deltaTime;

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
        if (log)
            Debug.Log($"{this} was destroyed!");
    }

    private void DisableObject()
    {
        if (state == null)
            gameObject.SetActive(false);
        else
            state.ToggleState(false);
    }
}