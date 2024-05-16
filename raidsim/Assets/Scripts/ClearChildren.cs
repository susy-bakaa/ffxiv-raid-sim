using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClearChildren : MonoBehaviour
{
    public bool onAwake = true;

    void Awake()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    }
}
