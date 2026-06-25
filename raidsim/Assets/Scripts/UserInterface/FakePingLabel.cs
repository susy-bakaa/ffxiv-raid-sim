// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using UnityEngine;
using TMPro;
using dev.susybaka.raidsim.Core;

namespace dev.susybaka.raidsim.UI.Development
{
    public class FakePingLabel : MonoBehaviour
    {
        private TextMeshProUGUI label;

        private void Awake()
        {
            label = GetComponentInChildren<TextMeshProUGUI>();
        }

        private void Update()
        {
            if (label == null)
                return;

            if (FightTimeline.Instance == null || FightTimeline.Instance.useServerTickSimulation == false || FightTimeline.Instance.usePingSimulation == false)
            {
                label.text = "Fake Ping: N/A";
                return;
            }

            label.text = $"Fake Ping: {FightTimeline.Instance.simulatedPingMs.ToString("F0")} ms";
        }
    }
}