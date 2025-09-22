// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using dev.susybaka.raidsim.Core;
using static dev.susybaka.raidsim.Core.GlobalData;

namespace dev.susybaka.raidsim.Bots
{
    public class BranchingBotTimelineSelector : MonoBehaviour
    {
        [Min(-1)] public int fightTimelineRandomEventResultId = -1;
        public List<UnityEvent<BotTimeline>> availableResults = new List<UnityEvent<BotTimeline>>();
        public bool useIndexMapping = false;
        public List<IndexMapping> indexMapping = new List<IndexMapping>();

        public void Choose(BotTimeline timeline)
        {
            if (fightTimelineRandomEventResultId < 0 || availableResults == null || availableResults.Count <= 0)
            {
                Debug.LogError("Invalid random event index: " + fightTimelineRandomEventResultId);
                return;
            }

            if (FightTimeline.Instance == null)
                return;

            if (FightTimeline.Instance.TryGetRandomEventResult(fightTimelineRandomEventResultId, out int r))
            {
                if (useIndexMapping && indexMapping != null && indexMapping.Count > 0)
                {
                    foreach (IndexMapping mapping in indexMapping)
                    {
                        if (mapping.previousIndex == r)
                        {
                            r = mapping.nextIndex;
                            break;
                        }
                    }
                }

                if (r < 0 || r >= availableResults.Count)
                {
                    Debug.LogError($"[ChooseDynamicBotNode ({gameObject.name})] Resulting index {r} is out of bounds for available dynamic nodes count: {availableResults.Count}");
                    return;
                }

                availableResults[r].Invoke(timeline);
            }
        }
    }
}