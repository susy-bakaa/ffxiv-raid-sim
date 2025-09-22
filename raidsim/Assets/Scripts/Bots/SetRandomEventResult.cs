// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using UnityEngine;
using NaughtyAttributes;
using dev.susybaka.raidsim.Core;

namespace dev.susybaka.raidsim.Events
{
    public class SetRandomEventResult : MonoBehaviour
    {
        [Label("ID"), MinValue(0)]
        public int id = 0;
        [MinValue(0)]
        public int result = 0;

        public void SetResult(int result)
        {
            this.result = result;
            SetResult();
        }

        public void SetResult()
        {
            if (FightTimeline.Instance != null)
            {
                FightTimeline.Instance.SetRandomEventResult(id, result);
            }
        }
    }
}