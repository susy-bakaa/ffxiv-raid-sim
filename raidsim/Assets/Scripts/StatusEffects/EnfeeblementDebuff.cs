// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using UnityEngine;
using dev.susybaka.raidsim.Characters;

namespace dev.susybaka.raidsim.StatusEffects
{
    public class EnfeeblementDebuff : StatusEffect
    {
        [Header("Function")]
        [Tooltip("Whether this debuff should apply the stunned flag, freezing the character in place and also preventing the use of actions.")]
        public bool stunned = true;
        [Tooltip("Whether this debuff should apply the knockback resistant flag, making the character immune to knockbacks.")]
        public bool knockbackResistant = false;
        [Tooltip("Whether this debuff should apply the bind flag, preventing the character from moving.")]
        public bool bind = false;
        [Tooltip("Whether this debuff should apply the invulnerable flag, making the character immune to ALL damage and some negative debuffs.")]
        public bool invulnerable = false;
        [Tooltip("Whether this debuff should apply the untargetable flag, preventing the character from being targeted by others.")]
        public bool untargetable = false;
        [Tooltip("Whether this debuff should apply the uncontrollable flag, preventing the character from being controlled by their controller.")]
        public bool uncontrollable = true;
        [Tooltip("Whether this debuff should prevent the character from using actions.")]
        public bool preventActions = true;
        [Tooltip("Whether this debuff should prevent the character from changing it's target's automatically. Doesn't completely block targeting.")]
        public bool preventTargeting = false;
        public bool silence = false;
        public bool pacify = false;
        public bool amnesia = false;

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
            if (stunned)
                state.stunned.SetFlag(data.statusName, true);
            if (uncontrollable)
                state.uncontrollable.SetFlag(data.statusName, true);
            if (preventActions)
                state.canDoActions.SetFlag(data.statusName, false);
            if (preventTargeting)
                state.canTarget.SetFlag(data.statusName, false);
            if (silence)
                state.silenced.SetFlag(data.statusName, true);
            if (pacify)
                state.pacificied.SetFlag(data.statusName, true);
            if (amnesia)
                state.amnesia.SetFlag(data.statusName, true);
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
            if (stunned)
                state.stunned.RemoveFlag(data.statusName);
            if (uncontrollable)
                state.uncontrollable.RemoveFlag(data.statusName);
            if (preventActions)
                state.canDoActions.RemoveFlag(data.statusName);
            if (preventTargeting)
                state.canTarget.RemoveFlag(data.statusName);
            if (silence)
                state.silenced.RemoveFlag(data.statusName);
            if (pacify)
                state.pacificied.RemoveFlag(data.statusName);
            if (amnesia)
                state.amnesia.RemoveFlag(data.statusName);
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
            if (stunned)
                state.stunned.RemoveFlag(data.statusName);
            if (uncontrollable)
                state.uncontrollable.RemoveFlag(data.statusName);
            if (preventActions)
                state.canDoActions.RemoveFlag(data.statusName);
            if (preventTargeting)
                state.canTarget.RemoveFlag(data.statusName);
            if (silence)
                state.silenced.RemoveFlag(data.statusName);
            if (pacify)
                state.pacificied.RemoveFlag(data.statusName);
            if (amnesia)
                state.amnesia.RemoveFlag(data.statusName);
            base.OnCleanse(state);
        }
    }
}