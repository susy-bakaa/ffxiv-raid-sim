using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BotNodeGroup : MonoBehaviour
{
    private float defaultAngle = 0f;

    private void Awake()
    {
        defaultAngle = transform.eulerAngles.y;
    }

    public void ResetGroupRotation()
    {
        transform.localEulerAngles = new Vector3(0, defaultAngle, 0);
    }

    public void RotateGroup(float angle)
    {
        transform.RotateAround(transform.position, Vector3.up, angle);
    }
}
