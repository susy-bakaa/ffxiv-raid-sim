using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using static GlobalData;

public class CombinedFightMechanic : FightMechanic
{
    public UnityEvent<ActionInfo> onTriggerMechanic;

    public override void TriggerMechanic(ActionInfo actionInfo)
    {
        if (!CanTrigger(actionInfo))
            return;

        onTriggerMechanic.Invoke(actionInfo);
    }
}
