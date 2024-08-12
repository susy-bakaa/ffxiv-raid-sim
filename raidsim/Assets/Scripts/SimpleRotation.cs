using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleRotation : MonoBehaviour
{
    public Vector3 rotation;

    void Update()
    {
        if (rotation != Vector3.zero)
        {
            transform.Rotate(rotation * Time.deltaTime);
        }
    }
}
