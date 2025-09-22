// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using System.Collections.Generic;
using UnityEngine;

namespace dev.susybaka.raidsim.Bots
{
    public class BotTimelineChoice : MonoBehaviour
    {
        public List<BotTimeline> availableTimelines = new List<BotTimeline>();
        public bool ChooseDisabled = true;

        public void Choose(BotTimeline timeline)
        {
            if (availableTimelines == null || availableTimelines.Count < 1)
                return;

            if (ChooseDisabled)
            {
                for (int i = 0; i < availableTimelines.Count; i++)
                {
                    if (!availableTimelines[i].bot.gameObject.activeSelf)
                    {
                        availableTimelines[i].bot = timeline.bot;
                        availableTimelines[i].bot.botTimeline = availableTimelines[i];
                        availableTimelines[i].SetReducedWaitTime(0.1f);
                        availableTimelines[i].StartTimeline();
                        return;
                    }
                }
            }
        }
    }
}