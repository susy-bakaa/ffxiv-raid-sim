// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using UnityEngine;
using dev.susybaka.raidsim.Characters;
using static dev.susybaka.raidsim.Core.GlobalData;

namespace dev.susybaka.raidsim.StatusEffects
{
    public class DamageOvertimeDebuff : StatusEffect
    {
        public bool instantDeath = false;
        public bool stacksMultiplyDamage = false;

        public void Reset()
        {
            damage = new Damage(-1000, true, string.Empty);
        }

        public override void OnTick(CharacterState state)
        {
            if (stacksMultiplyDamage)
            {
                int stacks = 1;

                if (this.stacks > 1)
                    stacks = this.stacks;

                state.ModifyHealth(new Damage(damage, Mathf.RoundToInt(damage.value * stacks), data.negative), instantDeath, data.hidden);
            }
            else
            {
                state.ModifyHealth(new Damage(damage, data.negative), instantDeath, data.hidden);
            }

            base.OnTick(state);
        }
    }
}