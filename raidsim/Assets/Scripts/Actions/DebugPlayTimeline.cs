// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using UnityEngine;
using dev.susybaka.raidsim.Core;

namespace dev.susybaka.raidsim.Actions
{
    public class DebugPlayTimeline : MonoBehaviour
    {
        public void Play()
        {
            if (FightTimeline.Instance != null)
            {
                FightTimeline.Instance.StartTimeline();
            }
        }
    }
}