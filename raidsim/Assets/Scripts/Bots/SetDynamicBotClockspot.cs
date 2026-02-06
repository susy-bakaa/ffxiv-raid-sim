// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using UnityEngine;
using static dev.susybaka.raidsim.Bots.BotTimeline;

namespace dev.susybaka.raidsim.Bots
{
    public class SetDynamicBotClockspot : MonoBehaviour
    {
        public int targetEventIndex = -1;

        public void SetNode(BotTimeline timeline)
        {
            if (timeline == null || timeline.bot == null || timeline.bot.clockSpot == null)
            {
                Debug.LogWarning("No available timeline or bot to choose node from.");
                return;
            }

            if (targetEventIndex >= 0 && targetEventIndex <= timeline.events.Count)
            {
                BotEvent e = timeline.events[targetEventIndex];
                e.node = timeline.bot.clockSpot.transform;
                timeline.events[targetEventIndex] = e;
            }
            else
            {
                for (int i = 0; i < timeline.events.Count; i++)
                {
                    BotEvent e = timeline.events[i];
                    if (e.dynamic)
                    {
                        e.node = timeline.bot.clockSpot.transform;
                        timeline.events[i] = e;
                        break;
                    }
                }
            }
        }
    }
}