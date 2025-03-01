using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static BotNode;
using static GlobalData;

public class BotNodeGroup : MonoBehaviour
{
    public Sector sector = Sector.N;
    private float defaultAngle = 0f;
    private List<BotNode> nodes = new List<BotNode>();
    public bool log = false;

    private void Awake()
    {
        defaultAngle = transform.localEulerAngles.y;
        nodes.AddRange(GetComponentsInChildren<BotNode>(true));
        for (int i = 0; i < nodes.Count; i++)
        {
            if (nodes[i].gameObject == gameObject)
            {
                nodes.RemoveAt(i);
                break;
            }
        }

        if (FightTimeline.Instance != null)
            FightTimeline.Instance.onReset.AddListener(ResetGroup);
    }

    public void ResetGroup()
    {
        foreach (BotNode node in nodes)
        {
            node.hasMechanic = false;
            node.occupied = false;
        }
        ResetGroupRotation();
    }

    public void ResetGroupRotation()
    {
        transform.localEulerAngles = new Vector3(0, defaultAngle, 0);
    }

    public void RotateGroup(float angle)
    {
        transform.RotateAround(transform.position, Vector3.up, angle);
    }

    public bool DoesSectorHaveNodesAvailable(Sector sector)
    {
        foreach (BotNode node in nodes)
        {
            if (node.hasMechanic && !node.occupied)
            {
                foreach (SectorPriority priority in node.sectorPriorities)
                {
                    if (priority.sector == sector)
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    public int GetHighestPriorityForSector(Sector sector)
    {
        int highestPriority = int.MinValue;
        foreach (BotNode node in nodes)
        {
            if (node.hasMechanic && !node.occupied)
            {
                int currentPriority = node.GetHighestPriorityAvailableForSector(sector);

                if (currentPriority > highestPriority)
                {
                    highestPriority = currentPriority;
                }
            }
        }
        return highestPriority;
    }

    public int GetLowestPriorityForSector(Sector sector)
    {
        int lowestPriority = int.MaxValue;
        foreach (BotNode node in nodes)
        {
            if (node.hasMechanic && !node.occupied)
            {
                int currentPriority = node.GetLowestPriorityAvailableForSector(sector);

                if (currentPriority < lowestPriority)
                {
                    lowestPriority = currentPriority;
                }
            }
        }
        return lowestPriority;
    }

    public BotNode GetHighestPriorityNodeForSector(Sector sector)
    {
        BotNode highestPriorityNode = null;
        int highestPriority = int.MinValue;

        foreach (BotNode node in nodes)
        {
            if (node.hasMechanic && !node.occupied)
            {
                foreach (SectorPriority priority in node.sectorPriorities)
                {
                    if (priority.sector == sector && priority.priority > highestPriority)
                    {
                        if (log)
                            Debug.Log($"[BotNodeGroup.Highest ({gameObject.name})] {node.name} has priority {priority.priority} for sector {sector}");
                        highestPriority = priority.priority;
                        highestPriorityNode = node;
                    }
                }
            }
        }

        if (highestPriorityNode != null)
            highestPriorityNode.occupied = true;

        return highestPriorityNode;
    }

    public BotNode GetLowestPriorityNodeForSector(Sector sector)
    {
        BotNode lowestPriorityNode = null;
        int lowestPriority = int.MaxValue;

        foreach (BotNode node in nodes)
        {
            if (node.hasMechanic && !node.occupied)
            {
                foreach (SectorPriority priority in node.sectorPriorities)
                {
                    if (priority.sector == sector && priority.priority < lowestPriority)
                    {
                        if (log)
                            Debug.Log($"[BotNodeGroup.Lowest ({gameObject.name})] {node.name} has priority {priority.priority} for sector {sector}");
                        lowestPriority = priority.priority;
                        lowestPriorityNode = node;
                    }
                }
            }
        }
        if (lowestPriorityNode != null)
            lowestPriorityNode.occupied = true;
        return lowestPriorityNode;
    }
}
