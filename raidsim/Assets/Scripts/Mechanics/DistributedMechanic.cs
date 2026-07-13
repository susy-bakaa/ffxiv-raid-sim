// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using System.Collections.Generic;
using UnityEngine;
using dev.susybaka.raidsim.Core;
using dev.susybaka.raidsim.Nodes;
using static dev.susybaka.raidsim.Core.GlobalData;

namespace dev.susybaka.raidsim.Mechanics
{
    public class DistributedMechanic : FightMechanic
    {
        public List<MechanicNode> nodes = new();
        public List<FightMechanic> mechanics = new();
        [Min(0)] public int count = 8;
        public bool distributeInOrder = false;
        public bool allowWrap = true;

        public override void TriggerMechanic(ActionInfo actionInfo)
        {
            if (!CanTrigger(actionInfo))
            {
                return;
            }

            if (nodes == null || nodes.Count < 1 || mechanics == null || mechanics.Count < 1)
            {
                return;
            }

            if (log)
                Debug.Log($"[DistributedMechanic ({gameObject.name})] Triggered with {nodes.Count} nodes and {mechanics.Count} mechanics, to select {count} options.");

            int actualCount = count;
            
            // If allowWrap is false and count exceeds available nodes, cap the count
            if (!allowWrap && count > nodes.Count)
            {
                actualCount = nodes.Count;
            }

            if (distributeInOrder)
            {
                // Random selection of nodes in ascending order
                List<int> selectedIndices = GetRandomOrderedIndices(nodes.Count, actualCount);

                if (log)
                    Debug.Log($"[DistributedMechanic ({gameObject.name})] Selected indices in order: {string.Join(", ", selectedIndices)}");

                int mechanicIndex = 0;

                foreach (int index in selectedIndices)
                {
                    mechanicIndex = Mathf.Min(mechanicIndex, mechanics.Count - 1);

                    if (log)
                        Debug.Log($"[DistributedMechanic ({gameObject.name})] Triggering mechanic at index {mechanicIndex} ({mechanics[mechanicIndex].gameObject.name}) for node at index {index} ({nodes[index].gameObject.name}).");

                    mechanics[mechanicIndex].TriggerMechanic(nodes[index]);
                    mechanicIndex++;
                }
            }
            else
            {
                int mechanicIndex = 0;

                // Random selection with possible repetition
                for (int i = 0; i < actualCount; i++)
                {
                    int randomIndex = FightTimeline.Instance.random.Pick($"DistributedMechanic_{gameObject.name}_RandomDistribution", nodes.Count, FightTimeline.Instance.GlobalRngMode);
                    mechanicIndex = Mathf.Min(mechanicIndex, mechanics.Count - 1);
                    mechanics[mechanicIndex].TriggerMechanic(nodes[randomIndex]);
                    mechanicIndex++;
                }
            }
        }

        private List<int> GetRandomOrderedIndices(int maxIndex, int count)
        {
            List<int> availableIndices = new();
            for (int i = 0; i < maxIndex; i++)
            {
                availableIndices.Add(i);
            }

            List<int> selectedIndices = new();
            for (int i = 0; i < count && availableIndices.Count > 0; i++)
            {
                int randomPos = FightTimeline.Instance.random.Pick($"DistributedMechanic_{gameObject.name}_OrderedDistribution_{i}", availableIndices.Count, FightTimeline.Instance.GlobalRngMode);
                selectedIndices.Add(availableIndices[randomPos]);
                availableIndices.RemoveAt(randomPos);
            }

            selectedIndices.Sort();
            return selectedIndices;
        }
    }
}