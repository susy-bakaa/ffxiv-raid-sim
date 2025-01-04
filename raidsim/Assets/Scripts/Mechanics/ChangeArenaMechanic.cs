using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GlobalData;

public class ChangeArenaMechanic : FightMechanic
{
    public Vector3 newBounds;
    public SN<bool> isCircle;

    public override void TriggerMechanic(ActionInfo actionInfo)
    {
        if (!CanTrigger(actionInfo))
            return;

        if (newBounds == Vector3.zero && isCircle == null)
        {
            FightTimeline.Instance.ResetArena();
        }
        else
        {
            FightTimeline.Instance.ChangeArena(newBounds, isCircle);
        }
    }

    public override void InterruptMechanic(ActionInfo actionInfo)
    {
        base.InterruptMechanic(actionInfo);

        FightTimeline.Instance.ResetArena();
    }
}