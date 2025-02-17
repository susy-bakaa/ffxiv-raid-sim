using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using static GlobalData;

public class UnparentObjectMechanic : FightMechanic
{
    [Header("Unparent Settings")]
    public Transform target;
    public Transform newParent;
    public string newParentName;
    public UnityEvent<GameObject> onExecute;

    private void Awake()
    {
        if (newParent == null && !string.IsNullOrEmpty(newParentName))
        {
            newParent = Utilities.FindAnyByName(newParentName).transform;
        }
    }

    public override void TriggerMechanic(ActionInfo actionInfo)
    {
        if (!CanTrigger(actionInfo))
            return;

        if (target != null)
        {
            target.SetParent(newParent);
            onExecute.Invoke(target.gameObject);
        }
    }
}
