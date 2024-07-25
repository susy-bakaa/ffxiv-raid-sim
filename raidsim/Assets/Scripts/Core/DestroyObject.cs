using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyObject : MonoBehaviour
{
    public float lifetime = 1f;
    public bool log = false;

    void Update()
    {
        if (lifetime > 0f)
        {
            Destroy(gameObject, lifetime);
        }
    }

    public void TriggerDestruction(float delay)
    {
        Destroy(gameObject, delay);
    }

    public void OnDestroy()
    {
        if (log)
            Debug.Log($"{this} was destroyed!");
    }
}