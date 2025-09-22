// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using UnityEngine;
using dev.susybaka.raidsim.Characters;

namespace dev.susybaka.raidsim.StatusEffects
{
    public class DiamondbackBuff : StatusEffect
    {
        [Header("Function")]
        public float damageReduction = 0.1f;

        public override void OnApplication(CharacterState state)
        {
            if (uniqueTag > 0)
                state.AddDamageReduction(damageReduction, $"{data.statusName}_{uniqueTag}");
            else
                state.AddDamageReduction(damageReduction, data.statusName);
            state.bound.SetFlag(data.statusName, true);
            state.knockbackResistant.SetFlag(data.statusName, true);
            state.canDoActions.SetFlag(data.statusName, false);
            base.OnApplication(state);
        }

        public override void OnExpire(CharacterState state)
        {
            if (uniqueTag > 0)
                state.RemoveDamageReduction($"{data.statusName}_{uniqueTag}");
            else
                state.RemoveDamageReduction(data.statusName);
            state.bound.RemoveFlag(data.statusName);
            state.knockbackResistant.RemoveFlag(data.statusName);
            state.canDoActions.RemoveFlag(data.statusName);
            base.OnExpire(state);
        }
    }
}