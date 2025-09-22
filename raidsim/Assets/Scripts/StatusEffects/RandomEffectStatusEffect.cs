// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using System.Collections;
using UnityEngine;
using dev.susybaka.raidsim.Characters;
using static dev.susybaka.raidsim.StatusEffects.StatusEffectData;
using Random = UnityEngine.Random;

namespace dev.susybaka.raidsim.StatusEffects
{
    public class RandomEffectStatusEffect : StatusEffect
    {
        [Header("Function")]
        public StatusEffectInfo effect;
        public float chancePerTick = 0.5f;
        public float increaseChancePerMissedTick = 0.1f;
        public float minDuration = 1f;
        public float maxDuration = 2f;

        private float additionalChancePerTick = 0f;
        private bool hasBeenInflicted = false;
        private Coroutine ieInflicted;

        public override void OnCleanse(CharacterState state)
        {
            base.OnCleanse(state);

            CleanUp(state, false);
        }

        public override void OnExpire(CharacterState state)
        {
            base.OnExpire(state);

            CleanUp(state, true);
        }

        public override void OnTick(CharacterState state)
        {
            base.OnTick(state);

            if (effect.data == null || string.IsNullOrEmpty(effect.name))
                return;

            if (hasBeenInflicted)
                return;

            float finalChance = chancePerTick + additionalChancePerTick;

            if (Random.value <= finalChance)
            {
                additionalChancePerTick = 0f;
                hasBeenInflicted = true;
                float duration = Random.Range(minDuration, maxDuration);
                state.AddEffect(effect.data, state, false, effect.tag, effect.stacks, duration);
                if (ieInflicted == null)
                    ieInflicted = StartCoroutine(IE_Inflicted(new WaitForSeconds(duration)));
            }
            else
            {
                additionalChancePerTick += increaseChancePerMissedTick;
            }
        }

        private IEnumerator IE_Inflicted(WaitForSeconds wait)
        {
            yield return wait;
            hasBeenInflicted = false;
            ieInflicted = null;
        }

        private void CleanUp(CharacterState state, bool expired)
        {
            if (effect.data == null || string.IsNullOrEmpty(effect.name))
                return;

            if (hasBeenInflicted && state.HasAnyVersionOfEffect(effect.data.statusName))
            {
                hasBeenInflicted = false;
                state.RemoveEffect(effect.data, expired, state, effect.tag, effect.stacks);
            }
        }
    }
}