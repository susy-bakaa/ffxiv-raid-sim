using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowTransform : MonoBehaviour
{
    public Transform target;
    public bool x;
    public bool y;
    public bool z;

    void Update()
    {
        if (x && y && z)
        {
            transform.position = target.position;
        }
        else
        {
            if (x)
            {
                transform.position = new Vector3(target.position.x, transform.position.y, transform.position.z);
            }
            if (y)
            {
                transform.position = new Vector3(transform.position.x, target.position.y, transform.position.z);
            }
            if (z)
            {
                transform.position = new Vector3(transform.position.x, transform.position.y, target.position.z);
            }
        }
    }
}
