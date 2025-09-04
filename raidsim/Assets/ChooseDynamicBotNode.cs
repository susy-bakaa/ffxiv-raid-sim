using System.Collections.Generic;
using UnityEngine;
using dev.susybaka.raidsim.Nodes;
using dev.susybaka.raidsim.Core;
using static dev.susybaka.raidsim.Bots.BotTimeline;
using UnityEngine.Serialization;

namespace dev.susybaka.raidsim.Bots
{
    public class ChooseDynamicBotNode : MonoBehaviour
    {
        public BotNodeGroup nodeGroup;
        public List<BotNode> availableNodes = new List<BotNode>();
        public int fightTimelineEventRandomResultId = -1;
        [FormerlySerializedAs("targetIndex")] public int targetEventIndex = -1;

        private void Start()
        {
            if (nodeGroup != null)
            {
                availableNodes = nodeGroup.Nodes;
            }
        }

        public void ChooseNode(BotTimeline timeline)
        {
            if (availableNodes == null || availableNodes.Count == 0)
            {
                Debug.LogWarning("No available nodes to choose from.");
                return;
            }

            int r = 0;

            if (FightTimeline.Instance != null && FightTimeline.Instance.TryGetRandomEventResult(fightTimelineEventRandomResultId, out r))
            {
                if (targetEventIndex >= 0 && targetEventIndex <= timeline.events.Count)
                {
                    BotEvent e = timeline.events[targetEventIndex];
                    e.node = availableNodes[r].transform;
                    timeline.events[targetEventIndex] = e;
                }
                else
                {
                    for (int i = 0; i < timeline.events.Count; i++)
                    {
                        BotEvent e = timeline.events[i];
                        if (e.dynamic)
                        {
                            e.node = availableNodes[r].transform;
                            timeline.events[i] = e;
                            break;
                        }
                    }
                }
            }
        }
    }
}