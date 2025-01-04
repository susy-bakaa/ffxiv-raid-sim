using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GlobalData;

public class SetFightTimescaleMechanic : FightMechanic
{
    public float newTimeScale = 1f;

    public override void TriggerMechanic(ActionInfo actionInfo)
    {
        if (!CanTrigger(actionInfo))
            return;

        FightTimeline.timeScale = newTimeScale;
    }
}
