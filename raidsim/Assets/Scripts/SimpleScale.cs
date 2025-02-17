using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleScale : MonoBehaviour
{
    public Vector3 scale;
    public bool setScale = false;
    public float duration = 0.5f;

    private bool scaled;

    void Update()
    {
        if (scaled)
            return;

        if (!setScale)
        {
            if (scale != Vector3.zero)
            {
                scaled = true;
                transform.LeanScale(scale, duration);
            }
        }
        else
        {
            scaled = true;
            transform.localScale = scale;
        }
    }
}
