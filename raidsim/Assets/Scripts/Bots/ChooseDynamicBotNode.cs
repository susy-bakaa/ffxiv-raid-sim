// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using System;
using System.Collections.Generic;
using dev.susybaka.raidsim.Characters;
using dev.susybaka.raidsim.Core;
using dev.susybaka.raidsim.Nodes;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using static dev.susybaka.raidsim.Bots.BotTimeline;
using static dev.susybaka.raidsim.Core.GlobalData;

namespace dev.susybaka.raidsim.Bots
{
    public class ChooseDynamicBotNode : MonoBehaviour
    {
        public BotNodeGroup nodeGroup;
        public List<BotNode> availableNodes = new List<BotNode>();
        [ShowIf(nameof(useIndexMapping))] public List<IndexMapping> indexMapping = new List<IndexMapping>();
        [ShowIf(nameof(_showSecondIndexMapping))] public List<IndexMapping> indexMapping2 = new List<IndexMapping>();
        [HideIf(nameof(_hideFighttimelineEventStuff)), Min(-1)] public int fightTimelineEventRandomResultId = -1;
        [Min(-1)] public int fightTimelineEventRandomResultId2 = -1;
        [FormerlySerializedAs("targetIndex"), HideIf(nameof(multipleTargetEvents))] public int targetEventIndex = -1;
        [ShowIf(nameof(multipleTargetEvents))] public List<int> targetEventIndices = new List<int>();
        public bool multipleTargetEvents = false;
        public bool obtainNodesOnStart = true;
        public bool useIndexMapping = false;
        public bool chooseBasedOnBotGroup = false;
        public UnityEvent<BotNode> onNodeChosen;

        private bool _showSecondIndexMapping => useIndexMapping && fightTimelineEventRandomResultId2 > -1;
        private bool _hideFighttimelineEventStuff => chooseBasedOnBotGroup;

        private void Start()
        {
            if (nodeGroup != null && obtainNodesOnStart)
            {
                availableNodes = nodeGroup.Nodes;
            }

            if (chooseBasedOnBotGroup)
            {
                fightTimelineEventRandomResultId = -1;
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

            if (!chooseBasedOnBotGroup)
            {
                if (FightTimeline.Instance != null && FightTimeline.Instance.TryGetRandomEventResult(fightTimelineEventRandomResultId, out r))
                {
                    ChooseNodeInternal(timeline, r);
                }
            }
            else
            {
                if (timeline.bot.TryGetComponent(out CharacterState character))
                {
                    ChooseNodeInternal(timeline, character.group);
                }
            }
        }

        private void ChooseNodeInternal(BotTimeline timeline, int r)
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

                if (fightTimelineEventRandomResultId2 > -1 && FightTimeline.Instance.TryGetRandomEventResult(fightTimelineEventRandomResultId2, out int r2))
                {
                    if (indexMapping2 != null && indexMapping2.Count > 0)
                    {
                        foreach (IndexMapping mapping in indexMapping2)
                        {
                            if (mapping.previousIndex == r2)
                            {
                                r += mapping.nextIndex;
                                break;
                            }
                        }
                    }
                }
            }
            else if (fightTimelineEventRandomResultId2 > -1 && FightTimeline.Instance.TryGetRandomEventResult(fightTimelineEventRandomResultId2, out int r2))
            {
                r += r2;
            }

            if (!multipleTargetEvents)
            {
                if (targetEventIndex >= 0 && targetEventIndex <= timeline.events.Count)
                {
                    BotEvent e = timeline.events[targetEventIndex];
                    e.node = availableNodes[r]?.transform;
                    timeline.events[targetEventIndex] = e;
                    onNodeChosen?.Invoke(availableNodes[r]);
                }
                else
                {
                    for (int i = 0; i < timeline.events.Count; i++)
                    {
                        BotEvent e = timeline.events[i];
                        if (e.dynamic)
                        {
                            e.node = availableNodes[r]?.transform;
                            timeline.events[i] = e;
                            onNodeChosen?.Invoke(availableNodes[r]);
                            break;
                        }
                    }
                }
            }
            else
            {
                if (targetEventIndices != null && targetEventIndices.Count > 0)
                {
                    foreach (int index in targetEventIndices)
                    {
                        if (index >= 0 && index < timeline.events.Count)
                        {
                            BotEvent e = timeline.events[index];
                            e.node = availableNodes[r]?.transform;
                            timeline.events[index] = e;
                            onNodeChosen?.Invoke(availableNodes[r]);
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < timeline.events.Count; i++)
                    {
                        BotEvent e = timeline.events[i];
                        if (e.dynamic && e.node == null)
                        {
                            e.node = availableNodes[r]?.transform;
                            timeline.events[i] = e;
                            onNodeChosen?.Invoke(availableNodes[r]);
                        }
                    }
                }
            }
        }
    }
}