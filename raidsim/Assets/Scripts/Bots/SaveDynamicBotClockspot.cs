// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using UnityEngine;
using dev.susybaka.raidsim.Nodes;

namespace dev.susybaka.raidsim.Bots
{
    public class SaveDynamicBotClockspot : MonoBehaviour
    {
        public BotNode clockSpotNode = null;

        public void StoreNode(BotNode node)
        {
            clockSpotNode = node;
        }

        public void SetBotClockspot(BotTimeline botTimeline)
        {
            if (botTimeline == null || clockSpotNode == null) 
                return;

            if (botTimeline.bot != null)
            {
                botTimeline.bot.clockSpot = clockSpotNode;
            }
        }
    }
}