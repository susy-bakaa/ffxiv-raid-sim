using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyObject : MonoBehaviour
{
    public float lifetime = 1f;

    void Update()
    {
        Destroy(gameObject, lifetime);
    }
}
