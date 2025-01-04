using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GlobalData;

public class GapCloserMechanic : FightMechanic
{
    [Header("Gap Closer Settings")]
    public bool lockMovement = true;
    public bool lockActions = true;
    public float duration = 0.5f;
    public float delay = 0.25f;
    public float maxMelee = 1.5f;
    public LeanTweenType ease = LeanTweenType.easeInOutQuad;
    public Axis moveAxis = new Axis(true, false, true);

    Coroutine ieDelayedMovement;
    LTDescr tween;
    Vector3 startPosition;

    public override void TriggerMechanic(ActionInfo actionInfo)
    {
        if (!CanTrigger(actionInfo))
            return;

        if (actionInfo.source != null && actionInfo.target != null)
        {
            startPosition = actionInfo.source.transform.position;

            if (actionInfo.target.targetController != null && actionInfo.target.targetController.self != null)
            {
                if (lockMovement)
                    actionInfo.source.uncontrollable.SetFlag($"{mechanicName}_GapCloser", true);
                if (lockActions)
                    actionInfo.source.canDoActions.SetFlag($"{mechanicName}_GapCloser", true);
                if (delay > 0)
                {
                    if (ieDelayedMovement == null)
                        ieDelayedMovement = StartCoroutine(DelayedMovement(actionInfo, new WaitForSeconds(delay)));
                }
                else
                {
                    MoveToTarget(actionInfo);
                }
            }
        }
    }

    public override void InterruptMechanic(ActionInfo actionInfo)
    {
        StopAllCoroutines();
        ieDelayedMovement = null;
        tween.reset();
        if (actionInfo.source != null)
        {
            actionInfo.source.transform.position = startPosition;
        }
        ResetState(actionInfo);
    }

    private IEnumerator DelayedMovement(ActionInfo actionInfo, WaitForSeconds wait)
    {
        yield return wait;
        MoveToTarget(actionInfo);
    }

    private void MoveToTarget(ActionInfo actionInfo)
    {
        Vector3 direction = (actionInfo.target.targetController.self.transform.position - actionInfo.source.transform.position).normalized;
        Vector3 targetPosition = actionInfo.target.targetController.self.transform.position - direction * (actionInfo.target.targetController.self.hitboxRadius + maxMelee);
        if (!moveAxis.x)
            targetPosition.x = 0f;
        if (!moveAxis.y)
            targetPosition.y = 0f;
        if (!moveAxis.z)
            targetPosition.z = 0f;
        tween = actionInfo.source.transform.LeanMove(targetPosition, duration).setEase(ease).setOnComplete(() => ResetState(actionInfo));
    }

    private void ResetState(ActionInfo actionInfo)
    {
        ieDelayedMovement = null;
        if (actionInfo.source != null)
        {
            if (lockMovement)
                actionInfo.source.uncontrollable.RemoveFlag($"{mechanicName}_GapCloser");
            if (lockActions)
                actionInfo.source.canDoActions.RemoveFlag($"{mechanicName}_GapCloser");
        }
    }
}
