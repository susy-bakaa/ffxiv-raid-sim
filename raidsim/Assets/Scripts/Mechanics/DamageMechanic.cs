using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GlobalData;

public class DamageMechanic : FightMechanic
{
    [Header("Damage Settings")]
    public Damage damage;
    public bool kill = false;

    public override void TriggerMechanic(ActionInfo actionInfo)
    {
        if (!CanTrigger(actionInfo))
            return;

        if (actionInfo.target != null)
        {
            actionInfo.target.ModifyHealth(damage, kill);
            if (log)
                Debug.Log("DamageMechanic: actionInfo.target.ModifyHealth(damage, kill);");
        } 
        else if (actionInfo.source != null)
        {
            actionInfo.source.ModifyHealth(damage, kill);
            if (log)
                Debug.Log("DamageMechanic: actionInfo.source.ModifyHealth(damage, kill);");
        }
    }
}
