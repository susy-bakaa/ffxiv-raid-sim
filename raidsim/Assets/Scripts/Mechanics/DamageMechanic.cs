// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using UnityEngine;
using static dev.susybaka.raidsim.Core.GlobalData;

namespace dev.susybaka.raidsim.Mechanics
{
    public class DamageMechanic : FightMechanic
    {
        [Header("Damage Settings")]
        public Damage damage;
        public bool kill = false;
        public string onlyHitCharacterName = string.Empty;

        public override void TriggerMechanic(ActionInfo actionInfo)
        {
            if (!CanTrigger(actionInfo))
                return;

            if (actionInfo.target != null)
            {
                if (!string.IsNullOrEmpty(onlyHitCharacterName))
                {
                    if (!actionInfo.target.characterName.ToLower().Contains(onlyHitCharacterName.ToLower()))
                    {
                        if (log)
                            Debug.Log("DamageMechanic: onlyHitCharacterName check failed, skipping damage.");
                        return;
                    }
                }

                actionInfo.target.ModifyHealth(damage, kill);
                if (log)
                    Debug.Log("DamageMechanic: actionInfo.target.ModifyHealth(damage, kill);");
            }
            else if (actionInfo.source != null)
            {
                if (!string.IsNullOrEmpty(onlyHitCharacterName))
                {
                    if (!actionInfo.source.characterName.ToLower().Contains(onlyHitCharacterName.ToLower()))
                    {
                        if (log)
                            Debug.Log("DamageMechanic: onlyHitCharacterName check failed, skipping damage.");
                        return;
                    }
                }

                actionInfo.source.ModifyHealth(damage, kill);
                if (log)
                    Debug.Log("DamageMechanic: actionInfo.source.ModifyHealth(damage, kill);");
            }
        }
    }
}