// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using System.Collections;
using UnityEngine;
using NaughtyAttributes;
using static dev.susybaka.raidsim.Core.GlobalData;

namespace dev.susybaka.raidsim.Mechanics
{
    public class GapCloserMechanic : FightMechanic
    {
        [Header("Gap Closer Settings")]
        public bool lockMovement = true;
        public bool lockActions = true;
        public bool fakeKnockback = false;
        [ShowIf("fakeKnockback")] public bool canBeResisted = false;
        public float duration = 0.5f;
        public float delay = 0.25f;
        public float maxMelee = 1.5f;
        public LeanTweenType ease = LeanTweenType.easeInOutQuad;
        public Axis moveAxis = new Axis(true, false, true);
        public Transform overrideTarget;

        Coroutine ieDelayedMovement;
        LTDescr tween;
        Vector3 startPosition;

        public override void TriggerMechanic(ActionInfo actionInfo)
        {
            if (!CanTrigger(actionInfo))
                return;

            if (fakeKnockback)
            {
                bool resisted = (actionInfo.source.knockbackResistant.value && canBeResisted) || actionInfo.source.bound.value || actionInfo.source.dead;

                if (resisted)
                    return;
            }

            if (actionInfo.source != null && (actionInfo.target != null || overrideTarget != null))
            {
                startPosition = actionInfo.source.transform.position;

                if ((actionInfo.target?.targetController != null && actionInfo.target?.targetController?.self != null) || overrideTarget != null)
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
            if (tween != null)
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
            Transform target = overrideTarget != null ? overrideTarget : actionInfo.target?.targetController?.self?.transform;
            float hitboxRadius = 0f;
            if (actionInfo.target != null && actionInfo.target.targetController != null && actionInfo.target.targetController.self != null)
                hitboxRadius = actionInfo.target.targetController.self.hitboxRadius;

            Vector3 originalPosition = actionInfo.source.transform.position;
            Vector3 direction = (target.position - actionInfo.source.transform.position).normalized;
            Vector3 targetPosition = target.position - direction * (hitboxRadius + maxMelee);
            if (!moveAxis.x)
                targetPosition.x = 0f;
            if (!moveAxis.y)
                targetPosition.y = 0f;
            if (!moveAxis.z)
                targetPosition.z = 0f;
            tween = actionInfo.source.transform.LeanMove(targetPosition, duration).setEase(ease).setOnUpdate((float _) => { if (actionInfo.source.dead) { LeanTween.cancel(tween.id); actionInfo.source.transform.position = originalPosition; } }).setOnComplete(() => ResetState(actionInfo));
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
}