using System.Collections.Generic;
using UnityEngine;
using dev.susybaka.raidsim.Core;
using static dev.susybaka.raidsim.Core.GlobalData;
using static dev.susybaka.raidsim.Nodes.BotNode;

namespace dev.susybaka.raidsim.Nodes
{
    public class BotNodeGroup : MonoBehaviour
    {
        public Sector sector = Sector.N;
        public int group = 0; // 0 is unassigned, 1 is group 1, 2 is group 2 etc.
        public List<Role> allowedRoles = new List<Role>();
        private float defaultAngle = 0f;
        private List<BotNode> nodes = new List<BotNode>();
        private List<BotNodeGroup> childGroups = new List<BotNodeGroup>();
        public bool log = false;

        private void Awake()
        {
            defaultAngle = transform.localEulerAngles.y;
            nodes.AddRange(GetComponentsInChildren<BotNode>(true));
            childGroups.AddRange(GetComponentsInChildren<BotNodeGroup>(true));
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

        public void SetGroupRotation(float y)
        {
            transform.localEulerAngles = new Vector3(0, y, 0);
        }

        public void CopyRotation(Transform source)
        {
            transform.eulerAngles = new Vector3(0, source.eulerAngles.y, 0);
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

        public BotNode GetNearestNode(Vector3 position, bool ignoreOccupancy = true, bool ignoreMechanics = true)
        {
            BotNode nearestNode = null;
            float nearestDistance = float.MaxValue;

            foreach (BotNode node in nodes)
            {
                // Skip if occupancy is not ignored and node is occupied
                if (!ignoreOccupancy && node.occupied)
                    continue;

                // Skip if mechanics are not ignored and node lacks mechanic
                if (!ignoreMechanics && !node.hasMechanic)
                    continue;

                // If we reach here, node is valid under the given rules
                (nearestDistance, nearestNode) = CalculateDistance(true, nearestDistance, nearestNode, position, node);
            }
            if (!ignoreOccupancy)
            {
                if (nearestNode != null)
                    nearestNode.occupied = true;
            }

            return nearestNode;
        }

        public BotNode GetFurthestNode(Vector3 position, bool ignoreOccupancy = true, bool ignoreMechanics = true)
        {
            BotNode furthestNode = null;
            float furthestDistance = 0f;

            foreach (BotNode node in nodes)
            {
                // Skip if occupancy is not ignored and node is occupied
                if (!ignoreOccupancy && node.occupied)
                    continue;

                // Skip if mechanics are not ignored and node lacks mechanic
                if (!ignoreMechanics && !node.hasMechanic)
                    continue;

                // If we reach here, node is valid under the given rules
                (furthestDistance, furthestNode) = CalculateDistance(false, furthestDistance, furthestNode, position, node);
            }
            if (!ignoreOccupancy)
            {
                if (furthestNode != null)
                    furthestNode.occupied = true;
            }

            return furthestNode;
        }

        private (float distance, BotNode result) CalculateDistance(bool close, float currentDistance, BotNode currentBest, Vector3 position, BotNode node)
        {
            float _distance = Vector3.Distance(position, node.transform.position);
            bool isBetter = close ? _distance < currentDistance : _distance > currentDistance;

            return isBetter ? (_distance, node) : (currentDistance, currentBest);
        }

        public BotNode GetGroupNode(int group, bool ignoreOccupancy = true, bool ignoreMechanics = true)
        {
            return GetRoleGroupNodeInternal(Role.unassigned, group, ignoreOccupancy, ignoreMechanics);
        }

        public BotNode GetRoleNode(Role role, bool ignoreOccupancy = true, bool ignoreMechanics = true)
        {
            return GetRoleGroupNodeInternal(role, -1, ignoreOccupancy, ignoreMechanics);
        }

        public BotNode GetRoleNodeForGroup(Role role, int group, bool ignoreOccupancy = true, bool ignoreMechanics = true)
        {
            return GetRoleGroupNodeInternal(role, group, ignoreOccupancy, ignoreMechanics);
        }

        private BotNode GetRoleGroupNodeInternal(Role role, int group, bool ignoreOccupancy, bool ignoreMechanics)
        {
            foreach (BotNode node in nodes)
            {
                if (IsValidNode(node, role, group, ignoreOccupancy, ignoreMechanics))
                {
                    if (log)
                        Debug.Log($"[BotNodeGroup.GetRoleNode ({gameObject.name})] Found node {node.name} for role {role}");

                    if (!ignoreOccupancy)
                        node.occupied = true;

                    return node;
                }
            }

            if (log)
                Debug.LogWarning($"[BotNodeGroup.GetRoleNode ({gameObject.name})] No available node found for role {role}");

            return null;
        }

        private bool IsValidNode(BotNode node, Role role, int group, bool ignoreOccupancy, bool ignoreMechanics)
        {
            if (role != Role.unassigned && !node.allowedRoles.Contains(role))
                return false;

            if (group >= 1 && !node.groupPriorities.Contains(group))
                return false;

            if (!ignoreOccupancy && !ignoreMechanics)
            {
                if (!node.hasMechanic || node.occupied)
                    return false;
            }
            else if (!ignoreOccupancy && node.occupied)
            {
                return false;
            }
            else if (!ignoreMechanics && !node.hasMechanic)
            {
                return false;
            }

            return true;
        }

        public BotNode GetRoleNodeForGroupFromChildren(Role role, int group, bool ignoreOccupancy = true, bool ignoreMechanics = true)
        {
            if (childGroups == null || childGroups.Count <= 0)
            {
                if (log)
                    Debug.LogWarning($"[BotNodeGroup.GetRoleNodeForGroupFromChildren ({gameObject.name})] No child groups found.");
                return null;
            }

            for (int i = 0; i < childGroups.Count; i++)
            {
                if (childGroups[i].group == group)
                {
                    BotNode node = childGroups[i].GetRoleNodeForGroup(role, group, ignoreOccupancy, ignoreMechanics);
                    if (node != null)
                    {
                        if (log)
                            Debug.Log($"[BotNodeGroup.GetRoleNodeForGroupFromChildren ({gameObject.name})] Found node {node.name} for role {role} in child group {childGroups[i].name}");
                        return node;
                    }
                }
            }

            if (log)
                Debug.LogWarning($"[BotNodeGroup.GetRoleNodeForGroupFromChildren ({gameObject.name})] No available node found for role {role} in group {group}");
            return null;
        }

        public BotNode GetRoleNodeFromChildren(Role role, bool ignoreOccupancy = true, bool ignoreMechanics = true)
        {
            if (childGroups == null || childGroups.Count <= 0)
            {
                if (log)
                    Debug.LogWarning($"[BotNodeGroup.GetRoleNodeFromChildren ({gameObject.name})] No child groups found.");
                return null;
            }

            for (int i = 0; i < childGroups.Count; i++)
            {
                if (childGroups[i].allowedRoles.Contains(role))
                {
                    BotNode node = childGroups[i].GetRoleNode(role, ignoreOccupancy, ignoreMechanics);
                    if (node != null)
                    {
                        if (log)
                            Debug.Log($"[BotNodeGroup.GetRoleNodeFromChildren ({gameObject.name})] Found node {node.name} for role {role} in child group {childGroups[i].name}");
                        return node;
                    }
                }
            }

            if (log)
                Debug.LogWarning($"[BotNodeGroup.GetRoleNodeFromChildren ({gameObject.name})] No available node found for role {role} in group {group}");
            return null;
        }

        public BotNode GetGroupNodeFromChildren(int group, bool ignoreOccupancy = true, bool ignoreMechanics = true)
        {
            if (childGroups == null || childGroups.Count <= 0)
            {
                if (log)
                    Debug.LogWarning($"[BotNodeGroup.GetGroupNodeFromChildren ({gameObject.name})] No child groups found.");
                return null;
            }

            for (int i = 0; i < childGroups.Count; i++)
            {
                if (childGroups[i].group == group)
                {
                    BotNode node = childGroups[i].GetGroupNode(group, ignoreOccupancy, ignoreMechanics);
                    if (node != null)
                    {
                        if (log)
                            Debug.Log($"[BotNodeGroup.GetGroupNodeFromChildren ({gameObject.name})] Found node {node.name} for group {group} in child group {childGroups[i].name}");
                        return node;
                    }
                }
            }

            if (log)
                Debug.LogWarning($"[BotNodeGroup.GetGroupNodeFromChildren ({gameObject.name})] No available node found for group {group}");
            return null;
        }

        public BotNode GetNearestNodeFromChildren(Vector3 position, bool ignoreOccupancy = true, bool ignoreMechanics = true)
        {
            if (childGroups == null || childGroups.Count == 0)
            {
                if (log)
                    Debug.LogWarning($"[BotNodeGroup.GetNearestNodeFromChildren ({gameObject.name})] No child groups found.");
                return null;
            }

            BotNode nearestNode = null;
            float nearestDistance = float.MaxValue;

            foreach (BotNodeGroup group in childGroups)
            {
                BotNode node = group.GetNearestNode(position, ignoreOccupancy, ignoreMechanics);
                if (node != null)
                {
                    float distance = Vector3.Distance(position, node.transform.position);
                    if (distance < nearestDistance)
                    {
                        nearestDistance = distance;
                        nearestNode = node;
                    }
                }
            }

            if (nearestNode != null && log)
                Debug.Log($"[BotNodeGroup.GetNearestNodeFromChildren ({gameObject.name})] Found nearest node {nearestNode.name} at distance {nearestDistance}");

            return nearestNode;
        }

        public BotNode GetFurthestNodeFromChildren(Vector3 position, bool ignoreOccupancy = true, bool ignoreMechanics = true)
        {
            if (childGroups == null || childGroups.Count == 0)
            {
                if (log)
                    Debug.LogWarning($"[BotNodeGroup.GetFurthestNodeFromChildren ({gameObject.name})] No child groups found.");
                return null;
            }

            BotNode furthestNode = null;
            float furthestDistance = 0f;

            foreach (BotNodeGroup group in childGroups)
            {
                BotNode node = group.GetFurthestNode(position, ignoreOccupancy, ignoreMechanics);
                if (node != null)
                {
                    float distance = Vector3.Distance(position, node.transform.position);
                    if (distance > furthestDistance)
                    {
                        furthestDistance = distance;
                        furthestNode = node;
                    }
                }
            }

            if (furthestNode != null && log)
                Debug.Log($"[BotNodeGroup.GetFurthestNodeFromChildren ({gameObject.name})] Found furthest node {furthestNode.name} at distance {furthestDistance}");

            return furthestNode;
        }

        public BotNode GetEnabledNode()
        {
            foreach (BotNode node in nodes)
            {
                if (node.gameObject.activeSelf)
                {
                    return node;
                }
            }
            return null;
        }

        public BotNode GetEnabledNodeFromChildren()
        {
            foreach (BotNodeGroup group in childGroups)
            {
                BotNode node = group.GetEnabledNode();
                if (node != null)
                {
                    return node;
                }
            }
            return null;
        }

        public BotNode GetUnoccupiedNode()
        {
            foreach (BotNode node in nodes)
            {
                if (!node.occupied && node.gameObject.activeSelf)
                {
                    return node;
                }
            }
            return null;
        }

        public BotNode GetUnoccupiedNodeFromChildren()
        {
            foreach (BotNodeGroup group in childGroups)
            {
                BotNode node = group.GetUnoccupiedNode();
                if (node != null)
                {
                    return node;
                }
            }
            return null;
        }

        public BotNode GetEmptyNode()
        {
            foreach (BotNode node in nodes)
            {
                if (!node.hasMechanic && node.gameObject.activeSelf)
                {
                    return node;
                }
            }
            return null;
        }

        public BotNode GetEmptyNodeFromChildren()
        {
            foreach (BotNodeGroup group in childGroups)
            {
                BotNode node = group.GetEmptyNode();
                if (node != null)
                {
                    return node;
                }
            }
            return null;
        }
    }
}