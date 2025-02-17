using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using static GlobalData;

public class DelayedMechanic : FightMechanic
{
    [Header("Delayed Mechanic Settings")]
    public bool startAutomatically = false;
    public float delay = 1f;
    public UnityEvent<ActionInfo> onDelayedTrigger;

    private Coroutine ieTriggerMechanicDelayed = null;

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
            if (ieTriggerMechanicDelayed != null)
                StopCoroutine(ieTriggerMechanicDelayed);

            ieTriggerMechanicDelayed = StartCoroutine(IE_TriggerMechanicDelayed(actionInfo, new WaitForSeconds(delay)));
        }
        else
        {
            onDelayedTrigger.Invoke(actionInfo);
        }
    }

    private IEnumerator IE_TriggerMechanicDelayed(ActionInfo actionInfo, WaitForSeconds wait)
    {
        yield return wait;
        onDelayedTrigger.Invoke(actionInfo);
        ieTriggerMechanicDelayed = null;
    }
}
