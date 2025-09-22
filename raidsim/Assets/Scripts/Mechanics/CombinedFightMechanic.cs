// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using UnityEngine;
using UnityEngine.Events;
using static dev.susybaka.raidsim.Core.GlobalData;

namespace dev.susybaka.raidsim.Mechanics
{
    public class CombinedFightMechanic : FightMechanic
    {
        [Header("Combined Fight Mechanic Settings")]
        public UnityEvent<ActionInfo> onTriggerMechanic;

        public override void TriggerMechanic(ActionInfo actionInfo)
        {
            if (!CanTrigger(actionInfo))
                return;

            onTriggerMechanic.Invoke(actionInfo);
        }
    }
}