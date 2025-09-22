// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using System.Collections;
using UnityEngine;
using NaughtyAttributes;
using dev.susybaka.raidsim.StatusEffects;
using static dev.susybaka.raidsim.Core.GlobalData;
using static dev.susybaka.raidsim.StatusEffects.StatusEffectData;

namespace dev.susybaka.raidsim.Mechanics
{
    public class DashMechanic : FightMechanic
    {
        [Header("Dash Settings")]
        public bool lockActions = true;
        public bool lockMovement = true;
        public bool dashToTarget = false;
        public float delay = 0.25f;
        public float duration = 0.5f;
        [ShowIf("dashToTarget")] public Vector3 overrideTargetPosition;
        [ShowIf("dashToTarget")] public Transform overrideTarget;
        [ShowIf("dashToTarget")] public StatusEffectInfo overrideTargetEffect;
        [HideIf("dashToTarget")] public Vector3 dashDirection = new Vector3(0f, 0f, 1f);
        [HideIf("dashToTarget")] public float dashDistance = 10f;
        public LeanTweenType ease = LeanTweenType.easeInOutQuad;
        public Axis moveAxis = new Axis(true, false, true);

        Coroutine ieDelayedMovement;
        LTDescr tween;
        Vector3 targetPosition;
        Vector3 startPosition;

        public override void TriggerMechanic(ActionInfo actionInfo)
        {
            if (!CanTrigger(actionInfo))
                return;

            bool execute = false;

            if (actionInfo.source != null)
            {
                startPosition = actionInfo.source.transform.position;

                if (dashToTarget)
                {
                    if (actionInfo.target != null || overrideTarget != null || overrideTargetPosition != Vector3.zero)
                    {
                        if (overrideTarget != null)
                            targetPosition = overrideTarget.position;
                        else if (overrideTargetPosition != Vector3.zero)
                            targetPosition = overrideTargetPosition;
                        else if (overrideTargetEffect.data != null)
                        {
                            string effectName = overrideTargetEffect.data.statusName;
                            if (overrideTargetEffect.tag > 0)
                                effectName = $"{overrideTargetEffect.data.statusName}_{overrideTargetEffect.tag}";
                            StatusEffect effect = actionInfo.source.GetEffect(effectName);
                            if (effect is StoreLocationEffect storeLocationEffect)
                                targetPosition = storeLocationEffect.location;
                        }
                        else
                            targetPosition = actionInfo.target.transform.position;
                    }
                }
                else
                {
                    targetPosition = actionInfo.source.transform.position + actionInfo.source.transform.forward * dashDirection.magnitude * dashDistance;
                }

                execute = true;
            }

            if (execute)
            {
                if (lockMovement)
                    actionInfo.source.uncontrollable.SetFlag($"{mechanicName}_Dash", true);
                if (lockActions)
                    actionInfo.source.canDoActions.SetFlag($"{mechanicName}_Dash", true);

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
            Vector3 direction = (targetPosition - actionInfo.source.transform.position).normalized;
            Vector3 finalTargetPosition = targetPosition - direction;
            if (!moveAxis.x)
            {
                finalTargetPosition.x = 0f;
                targetPosition.x = 0f;
            }
            if (!moveAxis.y)
            {
                finalTargetPosition.y = 0f;
                targetPosition.y = 0f;
            }
            if (!moveAxis.z)
            {
                finalTargetPosition.z = 0f;
                targetPosition.z = 0f;
            }
            if (duration > 0)
                tween = actionInfo.source.transform.LeanMove(finalTargetPosition, duration).setEase(ease).setOnComplete(() => ResetState(actionInfo));
            else
            {
                actionInfo.source.transform.position = targetPosition;
                ResetState(actionInfo);
            }
        }

        private void ResetState(ActionInfo actionInfo)
        {
            ieDelayedMovement = null;
            if (actionInfo.source != null)
            {
                if (lockMovement)
                    actionInfo.source.uncontrollable.RemoveFlag($"{mechanicName}_Dash");
                if (lockActions)
                    actionInfo.source.canDoActions.RemoveFlag($"{mechanicName}_Dash");
            }
        }
    }
}