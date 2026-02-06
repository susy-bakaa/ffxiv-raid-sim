// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using UnityEngine;
using UnityEngine.Events;
using dev.susybaka.raidsim.Characters;

namespace dev.susybaka.raidsim.StatusEffects
{
    [RequireComponent(typeof(StatusEffect))]
    public class GetStatusEffectDuration : MonoBehaviour
    {
        StatusEffect statusEffect;
        float duration;

        public bool runOnStart = true;
        public UnityEvent<float> onFound;

        private void Start()
        {
            statusEffect = null;
            duration = 0f;

            if (runOnStart)
                Find();
        }

        public void Initialize()
        {
            Find();
        }

        public float Find()
        {
            if (statusEffect == null)
                statusEffect = GetComponent<StatusEffect>();

            if (statusEffect == null)
            {
                Debug.LogError("GetStatusEffectDuration requires a StatusEffect component on the same GameObject.");
                return 0f;
            }

            duration = statusEffect.duration;

            if (duration > 0f)
            {
                onFound.Invoke(duration);
            }
            else
            {
                Debug.LogWarning("StatusEffect duration was zero.");
            }

            return duration;
        }
    }
}