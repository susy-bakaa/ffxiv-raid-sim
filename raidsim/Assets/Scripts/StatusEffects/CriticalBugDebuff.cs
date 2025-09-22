// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using System.Collections.Generic;
using UnityEngine;
using dev.susybaka.raidsim.Characters;
using dev.susybaka.raidsim.Core;
using dev.susybaka.raidsim.Mechanics;

namespace dev.susybaka.raidsim.StatusEffects
{
    public class CriticalBugDebuff : StatusEffect
    {
        [Header("Function")]
        public GameObject spawnObjectPrefab;
        public StatusEffectData inflictsStatusEffect;
        public List<StatusEffectData> cleansStatusEffects = new List<StatusEffectData>();

        public override void OnExpire(CharacterState state)
        {
            GameObject spawned = Instantiate(spawnObjectPrefab, state.transform.position, state.transform.rotation, FightTimeline.Instance.mechanicParent);
            if (spawned.TryGetComponent(out DamageTrigger damageTrigger))
            {
                damageTrigger.owner = state;
            }
            state.AddEffect(inflictsStatusEffect, state);
            if (cleansStatusEffects != null && cleansStatusEffects.Count > 0)
            {
                for (int i = 0; i < cleansStatusEffects.Count; i++)
                {
                    if (state.HasEffect(cleansStatusEffects[i].statusName))
                        state.RemoveEffect(cleansStatusEffects[i].statusName, false, state);
                }
            }
            base.OnExpire(state);
        }
    }
}