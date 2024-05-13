using UnityEngine;

public class SimpleBillboard : MonoBehaviour
{
    private Transform target;

    void Awake()
    {
        if (target == null)
            target = Camera.main.transform;
    }

    void Update()
    {
        if (target != null)
        {
            transform.LookAt(target);
        }
    }
}