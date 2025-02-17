using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using static GlobalData;

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

        if (FightTimeline.Instance != null)
        {
            // We store the result in the timeline for the AI to use when required in their logic
            FightTimeline.Instance.AddSectorMechanicResult(id, nodeAssignmentsBySector);
        }
    }

    private void AssignMiddleNodes(int minMiddle)
    {
        List<MechanicNode> availableMiddleNodes = new List<MechanicNode>();

        // Collect all available middle nodes from all sectors
        foreach (ArenaSector sector in arenaSectors)
        {
            foreach (MechanicNode node in sector.nodes)
            {
                if (node.isMiddle && !node.isTaken && !availableMiddleNodes.Contains(node))
                {
                    availableMiddleNodes.Add(node);
                }
            }
        }

        availableMiddleNodes = availableMiddleNodes.OrderBy(x => Random.value).ToList();

        Debug.Log($"Available middle nodes: {availableMiddleNodes.Count}");

        // Continue looping until we reach the minimum required middle nodes
        while (middleNodesUsed < minMiddle)
        {
            MechanicNode picked = availableMiddleNodes[Random.Range(0, availableMiddleNodes.Count)]; // Pick a random available node

            bool nodeAssigned = false;

            // Attempt to assign the node to its corresponding sectors
            for (int s = 0; s < arenaSectors.Count; s++)
            {
                ArenaSector sector = arenaSectors[s];

                if (sector.nodes.Contains(picked))
                {
                    // Ensure we don't exceed the allowed middle nodes per sector
                    if (sector.current >= sector.capacity || sector.current >= maxMiddleNodesPerSector)
                        continue;

                    if (picked.isTaken)
                        continue;

                    picked.isTaken = true;
                    nodeAssigned = true;
                    availableMiddleNodes.Remove(picked);
                    totalNodesUsed++;
                    middleNodesUsed++;

                    Debug.Log($"Assigning {picked.gameObject.name} to {sector.name} as the {totalNodesUsed}. node (mid)");

                    if (!nodeAssignmentsBySector.ContainsKey(sector.sector))
                        nodeAssignmentsBySector[sector.sector] = new List<MechanicNode>();

                    nodeAssignmentsBySector[sector.sector].Add(picked);
                    sector.current++;

                    arenaSectors[s] = sector;
                }
            }

            if (nodeAssigned)
                onNodeUsed.Invoke(picked);
        }
    }


    private void AssignRemainingNodes()
    {
        for (int i = 0; i < arenaSectors.Count; i++)
        {
            ArenaSector sector = arenaSectors[i];
            int maxNodes = maxTotalNodes + 1;

            if (sector.isFull)
                continue;

            while (!sector.isFull && totalNodesUsed < maxNodes)
            {
                MechanicNode picked = sector.nodes.GetRandomItem();

                if (picked.isTaken)
                    continue;

                if (middleNodesUsed >= maxMiddleNodes && picked.isMiddle)
                    continue;

                if (picked.isMiddle && sector.current >= maxMiddleNodesPerSector)
                    continue;

                picked.isTaken = true;
                sector.current++;
                totalNodesUsed++;
                if (picked.isMiddle)
                    middleNodesUsed++;

                Debug.Log($"Assigning {picked.gameObject.name} to {sector.name} as the {totalNodesUsed}. node (standard)");

                if (!nodeAssignmentsBySector.ContainsKey(sector.sector))
                    nodeAssignmentsBySector[sector.sector] = new List<MechanicNode>();

                nodeAssignmentsBySector[sector.sector].Add(picked);
                onNodeUsed.Invoke(picked);
            }

            arenaSectors[i] = sector;
        }
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
