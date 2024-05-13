using UnityEngine;

public class SimpleBillboard : MonoBehaviour
{
    private Transform target;

    void Awake()
    {
        if (target == null)
            target = Camera.main.transform;
    }

    void LateUpdate()
    {
        if (target != null)
        {
            transform.LookAt(target);
        }
    }
}