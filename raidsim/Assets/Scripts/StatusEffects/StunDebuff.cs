// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using UnityEngine;
using dev.susybaka.raidsim.Characters;

namespace dev.susybaka.raidsim.StatusEffects
{
    public class StunDebuff : StatusEffect
    {
        [Header("Function")]
        public bool knockbackResistant = false;
        public bool bind = false;
        public bool invulnerable = false;
        public bool untargetable = false;

        public override void OnApplication(CharacterState state)
        {
            if (invulnerable)
                state.invulnerable.SetFlag(data.statusName, true);
            if (bind)
                state.bound.SetFlag(data.statusName, true);
            if (knockbackResistant)
                state.knockbackResistant.SetFlag(data.statusName, true);
            if (untargetable)
                state.untargetable.SetFlag(data.statusName, true);
            state.stunned.SetFlag(data.statusName, true);
            state.uncontrollable.SetFlag(data.statusName, true);
            state.canDoActions.SetFlag(data.statusName, false);
            base.OnApplication(state);
        }

        public override void OnExpire(CharacterState state)
        {
            if (invulnerable)
                state.invulnerable.RemoveFlag(data.statusName);
            if (bind)
                state.bound.RemoveFlag(data.statusName);
            if (knockbackResistant)
                state.knockbackResistant.RemoveFlag(data.statusName);
            if (untargetable)
                state.untargetable.RemoveFlag(data.statusName);
            state.stunned.RemoveFlag(data.statusName);
            state.uncontrollable.RemoveFlag(data.statusName);
            state.canDoActions.RemoveFlag(data.statusName);
            base.OnExpire(state);
        }

        public override void OnCleanse(CharacterState state)
        {
            if (invulnerable)
                state.invulnerable.RemoveFlag(data.statusName);
            if (bind)
                state.bound.RemoveFlag(data.statusName);
            if (knockbackResistant)
                state.knockbackResistant.RemoveFlag(data.statusName);
            if (untargetable)
                state.untargetable.RemoveFlag(data.statusName);
            state.stunned.RemoveFlag(data.statusName);
            state.uncontrollable.RemoveFlag(data.statusName);
            state.canDoActions.RemoveFlag(data.statusName);
            base.OnCleanse(state);
        }
    }
}