// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using UnityEngine;
using UnityEngine.Events;
using dev.susybaka.raidsim.Characters;

namespace dev.susybaka.raidsim.StatusEffects
{
    [RequireComponent(typeof(StatusEffect))]
    public class GetStatusEffectSource : MonoBehaviour
    {
        StatusEffect statusEffect;
        CharacterState sourceCharacter;

        public bool runOnStart = true;
        public UnityEvent<CharacterState> onFound;

        private void Start()
        {
            statusEffect = null;
            sourceCharacter = null;

            if (runOnStart)
                Find();
        }

        public void Initialize()
        {
            Find();
        }

        public CharacterState Find()
        {
            if (statusEffect == null)
                statusEffect = GetComponent<StatusEffect>();

            if (statusEffect == null)
            {
                Debug.LogError("GetStatusEffectSource requires a StatusEffect component on the same GameObject.");
                return null;
            }

            sourceCharacter = statusEffect.Source;

            if (sourceCharacter != null)
            {
                onFound.Invoke(sourceCharacter);
            }
            else
            {
                Debug.LogWarning("StatusEffect source character was null.");
            }

            return sourceCharacter;
        }
    }
}