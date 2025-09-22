// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using dev.susybaka.raidsim.Core;
using static dev.susybaka.raidsim.Core.GlobalData;

namespace dev.susybaka.raidsim.Mechanics
{
    public class RandomResultBranchingMechanic : FightMechanic
    {
        FightTimeline fight;

        [Header("Random Result Branching Settings")]
        public List<RandomResultBranchedEvent> events = new List<RandomResultBranchedEvent>();
        public int randomEventId = 0;
        public bool useResultAsListIndex = true;

        private void Start()
        {
            fight = FightTimeline.Instance;
        }

        public override void TriggerMechanic(ActionInfo actionInfo)
        {
            if (!CanTrigger(actionInfo))
                return;

            if (useResultAsListIndex && randomEventId >= 0)
            {
                events[fight.GetRandomEventResult(randomEventId)].m_event.Invoke(actionInfo);
                if (log)
                    Debug.Log($"fight.GetRandomEventResult(randomEventId) {randomEventId} resulted in {fight.GetRandomEventResult(randomEventId)} -> m_event.Invoke(actionInfo) {events[fight.GetRandomEventResult(randomEventId)].name}");
            }
        }

        [System.Serializable]
        public struct RandomResultBranchedEvent
        {
            public string name;
            public UnityEvent<ActionInfo> m_event;
        }
    }
}