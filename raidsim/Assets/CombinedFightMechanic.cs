using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using static ActionController;

public class CombinedFightMechanic : FightMechanic
{
    public UnityEvent<ActionInfo> onTriggerMechanic;

    public override void TriggerMechanic(ActionInfo actionInfo)
    {
        base.TriggerMechanic(actionInfo);

        onTriggerMechanic.Invoke(actionInfo);
    }
}
