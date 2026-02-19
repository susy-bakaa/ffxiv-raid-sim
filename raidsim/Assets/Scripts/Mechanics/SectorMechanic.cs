// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using NaughtyAttributes;
using dev.susybaka.raidsim.Nodes;
using dev.susybaka.Shared;
using static dev.susybaka.raidsim.Core.GlobalData;

namespace dev.susybaka.raidsim.Mechanics
{
    public class SectorMechanic : FightMechanic
    {
        [Header("Sector Settings")]
        [Label("ID"), MinValue(0)] public int id = 0;
        public List<ArenaSector> arenaSectors = new List<ArenaSector>();
        [MinValue(0)] public int minMiddleNodes = 2;
        [MinValue(0)] public int maxMiddleNodes = 3;
        [MinValue(0)] public int maxMiddleNodesPerSector = 1;
        [MinValue(0)] public int maxTotalNodes = 8;
        [Header("Events")]
        public UnityEvent<MechanicNode> onNodeUsed;

        private int middleNodesUsed = 0;
        private int totalNodesUsed = 0;
        private Dictionary<Sector, List<MechanicNode>> nodeAssignmentsBySector = new Dictionary<Sector, List<MechanicNode>>();

        // Reuse lists to avoid GC
        private readonly List<ArenaSector> _availableSectors = new List<ArenaSector>();
        private readonly List<MechanicNode> _availableMiddleNodes = new List<MechanicNode>();
        private readonly List<MechanicNode> _candidates = new List<MechanicNode>();

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (id < 0)
                id = 0;
            if (maxMiddleNodes < 0)
                maxMiddleNodes = 0;
            if (minMiddleNodes < 0)
                minMiddleNodes = 0;
            if (maxTotalNodes < 0)
                maxTotalNodes = 0;
            if (maxMiddleNodesPerSector < 0)
                maxMiddleNodesPerSector = 0;
            for (int i = 0; i < arenaSectors.Count; i++)
            {
                ArenaSector temp = arenaSectors[i];
                if (string.IsNullOrEmpty(arenaSectors[i].name))
                {
                    temp.name = arenaSectors[i].sector.ToString();
                }
                if (temp.capacity < 0)
                    temp.capacity = 0;
                if (temp.current < 0)
                    temp.current = 0;
                arenaSectors[i] = temp;
            }
        }
#endif

        public override void TriggerMechanic(ActionInfo actionInfo)
        {
            if (!CanTrigger(actionInfo))
                return;

            if (arenaSectors == null || arenaSectors.Count == 0)
                return;

            nodeAssignmentsBySector.Clear();
            middleNodesUsed = 0;
            totalNodesUsed = 0;

            // Ensure the minimum middle nodes are assigned first
            AssignMiddleNodes(minMiddleNodes);

            // Distribute the remaining nodes based on sector rules
            AssignRemainingNodes();
        }

        private void AssignMiddleNodes(int minMiddle)
        {
            _availableMiddleNodes.Clear();
            _availableSectors.Clear();

            // Copy sectors (avoid aliasing the serialized list)
            _availableSectors.AddRange(arenaSectors);

            // Deterministic shuffle (no PickMode here)
            var rngSectors = timeline.random.Stream($"{GetUniqueName()}_{id}_Shuffle_AvailableSectors");
            _availableSectors.ShufflePCG(rngSectors);

            // Collect available middle nodes from all sectors
            for (int i = 0; i < _availableSectors.Count; i++)
            {
                var sector = _availableSectors[i];
                for (int n = 0; n < sector.nodes.Count; n++)
                {
                    var node = sector.nodes[n];
                    if (node.isMiddle && !node.isTaken && !_availableMiddleNodes.Contains(node))
                        _availableMiddleNodes.Add(node);
                }
            }

            // Deterministic shuffle (no PickMode)
            var rngNodes = timeline.random.Stream($"{GetUniqueName()}_{id}_Shuffle_AvailableMiddleNodes");
            _availableMiddleNodes.ShufflePCG(rngNodes);

            if (log)
                Debug.Log($"Available middle nodes: {_availableMiddleNodes.Count}");

            // Pick middle nodes until we reach minimum, using deterministic candidate-picking
            var rngPick = timeline.random.Stream($"{GetUniqueName()}_{id}_PickMiddleNode");

            while (middleNodesUsed < minMiddle)
            {
                // Build candidates that can actually be assigned right now
                _candidates.Clear();

                for (int i = 0; i < _availableMiddleNodes.Count; i++)
                {
                    var node = _availableMiddleNodes[i];
                    if (node.isTaken) continue;

                    // Is there *any* sector that contains this node and has capacity?
                    bool canAssign = false;
                    for (int s = 0; s < _availableSectors.Count; s++)
                    {
                        var sector = _availableSectors[s];
                        if (sector.current >= sector.capacity || sector.current >= maxMiddleNodesPerSector)
                            continue;

                        if (!sector.nodes.Contains(node))
                            continue;

                        canAssign = true;
                        break;
                    }

                    if (canAssign)
                        _candidates.Add(node);
                }

                if (_candidates.Count == 0)
                {
                    if (log)
                        Debug.LogWarning($"[{nameof(SectorMechanic)} ({gameObject.name})] No assignable middle nodes left, stopping early ({middleNodesUsed}/{minMiddle}).");
                    break;
                }

                var picked = _candidates[rngPick.NextInt(0, _candidates.Count)];

                bool nodeAssigned = false;

                // Assign to first eligible sector in the (already shuffled) sector order
                for (int s = 0; s < _availableSectors.Count; s++)
                {
                    ArenaSector sector = _availableSectors[s];

                    if (!sector.nodes.Contains(picked))
                        continue;

                    if (sector.current >= sector.capacity || sector.current >= maxMiddleNodesPerSector)
                        continue;

                    if (picked.isTaken)
                        break;

                    picked.isTaken = true;
                    nodeAssigned = true;

                    // Remove from pool so it can't be picked again
                    _availableMiddleNodes.Remove(picked);

                    totalNodesUsed++;
                    middleNodesUsed++;

                    if (log)
                        Debug.Log($"Assigning {picked.gameObject.name} to {sector.name} as the {totalNodesUsed}. node (mid)");

                    if (!nodeAssignmentsBySector.ContainsKey(sector.sector))
                        nodeAssignmentsBySector[sector.sector] = new List<MechanicNode>();

                    nodeAssignmentsBySector[sector.sector].Add(picked);

                    sector.current++;
                    _availableSectors[s] = sector;

                    break;
                }

                if (nodeAssigned)
                    onNodeUsed.Invoke(picked);
            }

            // Keep same behavior as before: write back sector.current updates
            for (int i = 0; i < arenaSectors.Count; i++)
            {
                // Match by sector enum (safer than relying on list order)
                for (int s = 0; s < _availableSectors.Count; s++)
                {
                    if (arenaSectors[i].sector == _availableSectors[s].sector)
                    {
                        arenaSectors[i] = _availableSectors[s];
                        break;
                    }
                }
            }
        }

        private void AssignRemainingNodes()
        {
            _availableSectors.Clear();
            _availableSectors.AddRange(arenaSectors);

            var rngSectors = timeline.random.Stream($"{GetUniqueName()}_{id}_Shuffle_RemainingSectors");
            _availableSectors.ShufflePCG(rngSectors);

            // Keep original behavior: arenaSectors order becomes randomized for this run
            arenaSectors = new List<ArenaSector>(_availableSectors);

            for (int i = 0; i < arenaSectors.Count; i++)
            {
                ArenaSector sector = arenaSectors[i];
                int maxNodes = maxTotalNodes + 1;

                if (sector.isFull)
                    continue;

                // Per-sector deterministic stream (prevents other RNG uses from affecting this sector)
                var rngSectorPick = timeline.random.Stream($"{GetUniqueName()}_{id}_RemainingPick_{sector.sector}");

                while (!sector.isFull && totalNodesUsed < maxNodes)
                {
                    _candidates.Clear();

                    // Build valid candidates instead of rerolling forever
                    for (int n = 0; n < sector.nodes.Count; n++)
                    {
                        var node = sector.nodes[n];

                        if (node.isTaken)
                            continue;

                        if (middleNodesUsed >= maxMiddleNodes && node.isMiddle)
                            continue;

                        if (node.isMiddle && sector.current >= maxMiddleNodesPerSector)
                            continue;

                        _candidates.Add(node);
                    }

                    if (_candidates.Count == 0)
                        break;

                    MechanicNode picked = _candidates[rngSectorPick.NextInt(0, _candidates.Count)];

                    picked.isTaken = true;
                    sector.current++;
                    totalNodesUsed++;
                    if (picked.isMiddle)
                        middleNodesUsed++;

                    if (log)
                        Debug.Log($"Assigning {picked.gameObject.name} to {sector.name} as the {totalNodesUsed}. node (standard)");

                    if (!nodeAssignmentsBySector.ContainsKey(sector.sector))
                        nodeAssignmentsBySector[sector.sector] = new List<MechanicNode>();

                    nodeAssignmentsBySector[sector.sector].Add(picked);
                    onNodeUsed.Invoke(picked);
                }

                arenaSectors[i] = sector;
            }
        }

        public override void InterruptMechanic(ActionInfo actionInfo)
        {
            middleNodesUsed = 0;
            totalNodesUsed = 0;
            nodeAssignmentsBySector = new Dictionary<Sector, List<MechanicNode>>();

            for (int i = 0; i < arenaSectors.Count; i++)
            {
                ArenaSector temp = arenaSectors[i];
                temp.current = 0;
                for (int j = 0; j < temp.nodes.Count; j++)
                {
                    temp.nodes[j].isTaken = false;
                }
                arenaSectors[i] = temp;
            }
        }

        protected override bool UsesPCG()
        {
            return true;
        }

        [System.Serializable]
        public struct ArenaSector
        {
            public string name;
            public Sector sector;
            public List<MechanicNode> nodes;
            [MinValue(0)] public int capacity;
            [MinValue(0)] public int current;
            public bool isFull => current >= capacity;

            public ArenaSector(string name, Sector sector, int capacity)
            {
                this.name = name;
                this.sector = sector;
                nodes = new List<MechanicNode>();
                this.capacity = capacity;
                current = 0;
            }

            public ArenaSector(string name, Sector sector, int capacity, List<MechanicNode> nodes)
            {
                this.name = name;
                this.sector = sector;
                this.nodes = nodes;
                this.capacity = capacity;
                current = 0;
            }
        }
    }
}