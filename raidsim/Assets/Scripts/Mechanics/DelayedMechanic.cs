using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using static GlobalData;

public class DelayedMechanic : FightMechanic
{
    public bool startAutomatically = false;
    public float delay = 1f;
    public UnityEvent<ActionInfo> onDelayedTrigger;

    private void Start()
    {
        if (startAutomatically)
            TriggerMechanic();
    }

    public override void TriggerMechanic(ActionInfo actionInfo)
    {
        if (!CanTrigger(actionInfo))
            return;

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
