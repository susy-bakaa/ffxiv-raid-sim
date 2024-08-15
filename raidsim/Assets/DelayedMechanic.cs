using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using static ActionController;

public class DelayedMechanic : FightMechanic
{
    public float delay = 1f;
    public UnityEvent<ActionInfo> onDelayedTrigger;

    public override void TriggerMechanic(ActionInfo actionInfo)
    {
        base.TriggerMechanic(actionInfo);

        if (delay > 0f)
        {
            StopAllCoroutines();
            StartCoroutine(TriggerMechanicDelayed(actionInfo));
        }
        else
        {
            onDelayedTrigger.Invoke(actionInfo);
        }
    }

    private IEnumerator TriggerMechanicDelayed(ActionInfo actionInfo)
    {
        yield return new WaitForSeconds(delay);
        onDelayedTrigger.Invoke(actionInfo);
    }
}
