using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using static GlobalData;

public class TimedMechanic : FightMechanic
{
    public float delay = 1f;
    public bool activateOnStart;
    public UnityEvent onFinish;
    int id = 0;

    void Start()
    {
        id = Random.Range(0,10000);

        if (activateOnStart)
        {
            TriggerMechanic(new ActionInfo(null, null, null));
        }
    }

    public override void TriggerMechanic(ActionInfo actionInfo)
    {
        if (!CanTrigger(actionInfo))
            return;

        if (delay > 0f)
        {
            Utilities.FunctionTimer.Create(this, () => this.onFinish.Invoke(), delay, $"TriggerMechanic_{id}_{mechanicName}_activation_delay", false, true);
        }
        else
        {
            onFinish.Invoke();
        }
    }

    public override void InterruptMechanic(ActionInfo actionInfo)
    {
        base.InterruptMechanic(actionInfo);

        Utilities.FunctionTimer.StopTimer($"TriggerMechanic_{id}_{mechanicName}_activation_delay");
    }
}
