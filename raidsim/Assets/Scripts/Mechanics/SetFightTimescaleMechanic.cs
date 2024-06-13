using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ActionController;

public class SetFightTimescaleMechanic : FightMechanic
{
    public float newTimeScale = 1f;

    public override void TriggerMechanic(ActionInfo actionInfo)
    {
        FightTimeline.timeScale = newTimeScale;
    }
}
