// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using UnityEngine;
using dev.susybaka.raidsim.Characters;
using dev.susybaka.Shared;
using static dev.susybaka.raidsim.Core.GlobalData;

namespace dev.susybaka.raidsim.StatusEffects
{
    public class CleansableDebuff : StatusEffect
    {
        [Header("Function")]
        public StatusEffectData[] cleanedBy;
        public int tags = 0;
        public bool esunable = true;
        public bool killsOnExpire = false;

        private int id = 0;

        public void Reset()
        {
            damage = new Damage(100, true, true, Damage.DamageType.unique, Damage.ElementalAspect.unaspected, Damage.PhysicalAspect.none, Damage.DamageApplicationType.percentageFromMax, string.Empty);
        }

        public override void OnUpdate(CharacterState state)
        {
            uniqueTag = tags;
            for (int i = 0; i < cleanedBy.Length; i++)
            {
                if (state.HasEffect(cleanedBy[i].statusName))
                {
                    state.RemoveEffect(data, false, state, tags, stacks);
                    return;
                }
            }
            base.OnUpdate(state);
        }

        public override void OnExpire(CharacterState state)
        {
            id = Random.Range(0, 10000);

            if (killsOnExpire)
            {
                // We need to add a small delay to the health modification or else the fly text for the debuff appears twice, this is a simple unnoticable fix for it.
                Utilities.FunctionTimer.Create(state, () => state.ModifyHealth(damage, true), 0.1f, $"{data.statusName}_{id}_killsOnExpire_ModifyHealth_Delay", false, true);
            }
            base.OnExpire(state);
        }
    }
}