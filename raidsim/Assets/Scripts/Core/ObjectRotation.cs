using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectRotation : MonoBehaviour
{
    public bool freezeX;
    public bool freezeY;
    public bool freezeZ;
    public bool setAutomatically = true;
    public Vector3 targetRotation;

    void Awake()
    {
        if (setAutomatically)
        {
            targetRotation = transform.rotation.eulerAngles;
        }
    }

    void Update()
    {
        Quaternion currentRotation = transform.rotation;

        Vector3 currentEulerAngles = currentRotation.eulerAngles;

        if (freezeX)
            currentEulerAngles.x = targetRotation.x;
        if (freezeY)
            currentEulerAngles.y = targetRotation.y;
        if (freezeZ)
            currentEulerAngles.z = targetRotation.z;

        transform.rotation = Quaternion.Euler(currentEulerAngles);
    }
}
