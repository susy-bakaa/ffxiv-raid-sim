using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleMovement : MonoBehaviour
{
    public Vector3 target = Vector3.zero;
    public float speed = 0.1f;
    public bool loop = true;
    private Vector3 originalPosition;
    private bool movingToTarget = true;

    private void Awake()
    {
        originalPosition = transform.localPosition;
    }

    private void Update()
    {
        if (movingToTarget)
        {
            transform.localPosition = Vector3.MoveTowards(transform.localPosition, target, speed * Time.deltaTime);
            if (transform.localPosition == target)
            {
                movingToTarget = false;
            }
        }
        else if (loop)
        {
            transform.localPosition = Vector3.MoveTowards(transform.localPosition, originalPosition, speed * Time.deltaTime);
            if (transform.localPosition == originalPosition)
            {
                movingToTarget = true;
            }
        }
    }
}
