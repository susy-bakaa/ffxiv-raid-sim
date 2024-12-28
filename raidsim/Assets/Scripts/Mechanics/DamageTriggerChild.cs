using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageTriggerChild : MonoBehaviour
{
    private DamageTrigger m_parent;

    private void Awake()
    {
        m_parent = GetComponentInParent<DamageTrigger>();
    }

    private void OnTriggerEnter(Collider other)
    {
        m_parent.OnTriggerEnter(other);
    }

    private void OnTriggerStay(Collider other)
    {
        m_parent.OnTriggerStay(other);
    }

    private void OnTriggerExit(Collider other)
    {
        m_parent.OnTriggerExit(other);
    }
}