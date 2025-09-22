// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using UnityEngine;
using TMPro;
using dev.susybaka.raidsim.Core;
using dev.susybaka.raidsim.Mechanics;
using static dev.susybaka.raidsim.StatusEffects.StatusEffectData;

namespace dev.susybaka.raidsim.UI
{
    public class DebuffPairSelector : MonoBehaviour
    {
        TMP_Dropdown dropdown;

        public StatusEffectInfoArray[] effects;
        public RaidwideDebuffPairsMechanic target;

        private void Start()
        {
            dropdown = GetComponentInChildren<TMP_Dropdown>();
            Select(0);
        }

        private void Update()
        {
            dropdown.interactable = !FightTimeline.Instance.playing;
        }

        public void Select(int value)
        {
            target.playerEffect = effects[value];
        }
    }
}