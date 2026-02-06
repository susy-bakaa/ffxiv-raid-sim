// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using dev.susybaka.raidsim.Core;
using static dev.susybaka.raidsim.Core.GlobalData;

namespace dev.susybaka.raidsim.Mechanics
{
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
            {
                if (FightTimeline.Instance != null)
                    FightTimeline.Instance.onReset.AddListener(TriggerMechanic);
                TriggerMechanic();
            }
        }

        public void AddDelay(float delay)
        {
            this.delay += delay;
        }

        public void SetDelay(float delay)
        {
            this.delay = delay;
        }

        public override void TriggerMechanic(ActionInfo actionInfo)
        {
            if (!CanTrigger(actionInfo))
                return;

            if (log)
                Debug.Log($"[DelayedMechanic ({gameObject.name})] Triggered delayed mechanic");

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
            if (log)
                Debug.Log($"[DelayedMechanic ({gameObject.name})] onDelayedTrigger.Invoke()");
        }

        public override void InterruptMechanic(ActionInfo actionInfo)
        {
            StopAllCoroutines();
            ieTriggerMechanicDelayed = null;
        }
    }
}