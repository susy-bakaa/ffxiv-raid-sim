using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using static GlobalData;

public class BotNode : MonoBehaviour
{
    public string nodeName;
    [Tag] public string targetTag;
    public bool occupied = false;
    public MechanicNode mechanicNode;
    public bool hasMechanic;
    public List<SectorPriority> sectorPriorities = new List<SectorPriority>();
    public List<int> groupPriorities = new List<int>();
    public List<Role> allowedRoles = new List<Role>();

    void Awake()
    {
        nodeName = gameObject.name;    
    }

    void Update()
    {
        if (mechanicNode != null)
        {
            hasMechanic = mechanicNode.isTaken;
        }
    }

    public void UpdateNode()
    {
        if (!string.IsNullOrEmpty(targetTag))
        {
            Transform target = GameObject.FindGameObjectWithTag(targetTag).transform;
            if (target != null)
            {
                transform.position = target.position;
            }
        }
    }

    public int GetHighestPriorityAvailableForSector(Sector sector)
    {
        int highestPriority = int.MinValue;
        foreach (SectorPriority priority in sectorPriorities)
        {
            if (priority.sector == sector && priority.priority > highestPriority)
            {
                highestPriority = priority.priority;
            }
        }
        return highestPriority;
    }

    public int GetLowestPriorityAvailableForSector(Sector sector)
    {
        int lowestPriority = int.MaxValue;
        foreach (SectorPriority priority in sectorPriorities)
        {
            if (priority.sector == sector && priority.priority < lowestPriority)
            {
                lowestPriority = priority.priority;
            }
        }
        return lowestPriority;
    }

    public bool IsSectorAvailable(Sector sector)
    {
        foreach (SectorPriority priority in sectorPriorities)
        {
            if (priority.sector == sector)
            {
                return true;
            }
        }
        return false;
    }

    [System.Serializable]
    public struct SectorPriority
    {
        public Sector sector;
        public int priority;

        public SectorPriority(Sector sector, int priority)
        {
            this.sector = sector;
            this.priority = priority;
        }
    }
}
