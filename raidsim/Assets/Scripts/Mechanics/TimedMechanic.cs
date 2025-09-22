// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using UnityEngine;
using UnityEngine.Events;
using dev.susybaka.Shared;
using static dev.susybaka.raidsim.Core.GlobalData;

namespace dev.susybaka.raidsim.Mechanics
{
    public class TimedMechanic : FightMechanic
    {
        public float delay = 1f;
        public bool activateOnStart;
        public UnityEvent onFinish;
        int id = 0;

        private void Start()
        {
            id = Random.Range(0, 10000);

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
}