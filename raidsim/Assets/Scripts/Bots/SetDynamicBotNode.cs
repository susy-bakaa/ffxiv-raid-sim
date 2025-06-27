using System.Collections;
using NaughtyAttributes;
using UnityEngine;
using static BotTimeline;

public class SetDynamicBotNode : MonoBehaviour
{
    public enum DynamicNodeType { priority, nearest, furthest, roleBased, groupBased, enabled, unoccupied, empty }

    public BotNodeGroup nodeGroup;
    public BotNodeGroup NodeGroupFallback;
    public DynamicNodeType m_type = DynamicNodeType.priority;
    [ShowIf("m_type", DynamicNodeType.priority)] public bool reversePriority = false;
    private bool wasReversePriority = false;
    [ShowIf("m_type", DynamicNodeType.priority)] public bool reversePriorityFallback = false;
    private bool wasReversePriorityFallback = false;
    [ShowIf("m_type", DynamicNodeType.priority)] public bool waitIfPriorityLowerThan = false;
    private bool wasWaitIfPriorityLowerThan = false;
    [ShowIf("m_type", DynamicNodeType.priority)] public bool reverseIfPriorityLowerThan = false;
    private bool wasReverseIfPriorityLowerThan = false;
    [ShowIf("m_type", DynamicNodeType.priority)] public int priority = -1;
    private int wasPriority = -1;
    [ShowIf("m_type", DynamicNodeType.roleBased)] public bool matchGroup = false;
    [ShowIf("showWaitTime")] public float waitTime = 0f;
    [ShowIf("showWaitTime")] public bool reduceWaitTimeFromTimeline = false;
    public int targetTimelineEventIndex = -1;
    public bool childGroups = false;
    public bool log = false;

    private Coroutine ieSetNode;
    private bool done = false;

    private bool showWaitTime => (waitIfPriorityLowerThan && m_type == DynamicNodeType.priority);

    private void Awake()
    {
        wasReversePriority = reversePriority;
        wasReversePriorityFallback = reversePriorityFallback;
        wasWaitIfPriorityLowerThan = waitIfPriorityLowerThan;
        wasReverseIfPriorityLowerThan = reverseIfPriorityLowerThan;
        wasPriority = priority;
        done = false;
    }

    public void ResetComponent()
    {
        reversePriority = wasReversePriority;
        reversePriorityFallback = wasReversePriorityFallback;
        waitIfPriorityLowerThan = wasWaitIfPriorityLowerThan;
        reverseIfPriorityLowerThan = wasReverseIfPriorityLowerThan;
        priority = wasPriority;
        StopAllCoroutines();
        ieSetNode = null;
        done = false;
    }

    public void SetNode(BotTimeline timeline)
    {
        SetNode(timeline, targetTimelineEventIndex);
    }

    public void SetNode(BotTimeline timeline, int targetEventIndex)
    {
        if (nodeGroup == null)
        {
            Debug.LogWarning($"[SetDynamicBotNode ({gameObject.name})] No node group assigned!");
            return;
        }

        switch (m_type)
        {
            case DynamicNodeType.priority:
                if (log)
                    Debug.Log($"[SetDynamicBotNode ({gameObject.name})] Setting node based on priority: {priority}");
                SetNodeTypePriority(timeline, targetEventIndex);
                break;
            case DynamicNodeType.nearest:
                if (log)
                    Debug.Log($"[SetDynamicBotNode ({gameObject.name})] Setting node based on nearest node");
                SetNodeTypeDistance(timeline, targetEventIndex, true);
                break;
            case DynamicNodeType.furthest:
                if (log)
                    Debug.Log($"[SetDynamicBotNode ({gameObject.name})] Setting node based on furthest node");
                SetNodeTypeDistance(timeline, targetEventIndex, false);
                break;
            case DynamicNodeType.roleBased:
                if (log)
                    Debug.Log($"[SetDynamicBotNode ({gameObject.name})] Setting node based on role-based assignment");
                if (matchGroup)
                    SetNodeTypeRoleGroup(timeline, targetEventIndex);
                else
                    SetNodeTypeRole(timeline, targetEventIndex);
                break;
            case DynamicNodeType.groupBased:
                if (log)
                    Debug.Log($"[SetDynamicBotNode ({gameObject.name})] Setting node based on group-based assignment");
                if (matchGroup)
                    SetNodeTypeRoleGroup(timeline, targetEventIndex);
                else
                    SetNodeTypeRole(timeline, targetEventIndex);
                break;
            case DynamicNodeType.enabled:
                if (log)
                    Debug.Log($"[SetDynamicBotNode ({gameObject.name})] Setting node based on enabled nodes in group");
                SetNodeTypeEnabled(timeline, targetEventIndex);
                break;
            case DynamicNodeType.unoccupied:
                if (log)
                    Debug.Log($"[SetDynamicBotNode ({gameObject.name})] Setting node based on unoccupied nodes in group");
                SetNodeTypeUnoccupied(timeline, targetEventIndex);
                break;
            case DynamicNodeType.empty:
                if (log)
                    Debug.Log($"[SetDynamicBotNode ({gameObject.name})] Setting node based on empty nodes in group");
                SetNodeTypeEmpty(timeline, targetEventIndex);
                break;
            default:
                Debug.LogError($"[SetDynamicBotNode ({gameObject.name})] node type not implemented yet!");
                break;
        }
    }

    private void SetNodeTypeRoleGroup(BotTimeline timeline, int targetEventIndex)
    {
        if (!childGroups)
        {
            AssignNode(timeline, targetEventIndex, nodeGroup.GetRoleNodeForGroup(timeline.bot.state.role, timeline.bot.state.group));
        }
        else
        {
            AssignNode(timeline, targetEventIndex, nodeGroup.GetRoleNodeForGroupFromChildren(timeline.bot.state.role, timeline.bot.state.group));
        }
    }

    private void SetNodeTypeRole(BotTimeline timeline, int targetEventIndex)
    {
        if (!childGroups)
        {
            AssignNode(timeline, targetEventIndex, nodeGroup.GetRoleNode(timeline.bot.state.role));
        }
        else
        {
            AssignNode(timeline, targetEventIndex, nodeGroup.GetRoleNodeFromChildren(timeline.bot.state.role));
        }
    }

    private void SetNodeTypeDistance(BotTimeline timeline, int targetEventIndex, bool nearest)
    {
        if (nearest)
        {
            if (!childGroups)
            {
                AssignNode(timeline, targetEventIndex, nodeGroup.GetNearestNode(timeline.bot.transform.position));
            }
            else
            {
                AssignNode(timeline, targetEventIndex, nodeGroup.GetNearestNodeFromChildren(timeline.bot.transform.position));
            }
        }
        else
        {
            if (!childGroups)
            {
                AssignNode(timeline, targetEventIndex, nodeGroup.GetFurthestNode(timeline.bot.transform.position));
            }
            else
            {
                AssignNode(timeline, targetEventIndex, nodeGroup.GetFurthestNodeFromChildren(timeline.bot.transform.position));
            }
        }
    }

    private void SetNodeTypePriority(BotTimeline timeline, int targetEventIndex)
    {
        if (priority >= 0)
        {
            if (nodeGroup.GetHighestPriorityForSector(timeline.bot.state.sector) < priority)
            {
                if (reverseIfPriorityLowerThan)
                {
                    reversePriority = !reversePriority;
                }

                if (waitIfPriorityLowerThan)
                {
                    if (ieSetNode == null)
                    {
                        ieSetNode = StartCoroutine(IE_AssignNode(timeline, targetEventIndex, (BotNode)null, new WaitForSeconds(waitTime)));
                        if (log)
                            Debug.Log($"[SetDynamicBotNode ({gameObject.name})] Setting node in {waitTime} seconds");
                    }
                }
                else
                {
                    AssignNodeTypePriority(timeline, targetEventIndex);
                }
            }
            else
            {
                AssignNodeTypePriority(timeline, targetEventIndex);
            }
        }
        else
        {
            AssignNodeTypePriority(timeline, targetEventIndex);
        }
    }

    private void AssignNodeTypePriority(BotTimeline timeline, int targetEventIndex)
    {
        BotNode node = null;

        node = TryGetNodeFromGroup(nodeGroup, timeline, reversePriority);

        if (node == null && NodeGroupFallback != null)
        {
            node = TryGetNodeFromGroup(NodeGroupFallback, timeline, reversePriorityFallback);
        }

        if (log)
            Debug.Log($"[SetDynamicBotNode ({gameObject.name})] Priority assignment finished with the following node: {node?.name}");

        AssignNode(timeline, targetEventIndex, node);
    }

    private void SetNodeTypeEnabled(BotTimeline timeline, int targetEventIndex)
    {
        BotNode node = null;
        if (!childGroups)
        {
            node = nodeGroup.GetEnabledNode();
        }
        else
        {
            node = nodeGroup.GetEnabledNodeFromChildren();
        }
        if (node == null && NodeGroupFallback != null)
        {
            if (!childGroups)
            {
                node = NodeGroupFallback.GetEnabledNode();
            }
            else
            {
                node = NodeGroupFallback.GetEnabledNodeFromChildren();
            }
        }
        AssignNode(timeline, targetEventIndex, node);
    }

    private void SetNodeTypeUnoccupied(BotTimeline timeline, int targetEventIndex)
    {
        BotNode node = null;
        if (!childGroups)
        {
            node = nodeGroup.GetUnoccupiedNode();
        }
        else
        {
            node = nodeGroup.GetUnoccupiedNodeFromChildren();
        }
        if (node == null && NodeGroupFallback != null)
        {
            if (!childGroups)
            {
                node = NodeGroupFallback.GetUnoccupiedNode();
            }
            else
            {
                node = NodeGroupFallback.GetUnoccupiedNodeFromChildren();
            }
        }
        AssignNode(timeline, targetEventIndex, node);
    }

    private void SetNodeTypeEmpty(BotTimeline timeline, int targetEventIndex)
    {
        BotNode node = null;
        if (!childGroups)
        {
            node = nodeGroup.GetEmptyNode();
        }
        else
        {
            node = nodeGroup.GetEmptyNodeFromChildren();
        }
        if (node == null && NodeGroupFallback != null)
        {
            if (!childGroups)
            {
                node = NodeGroupFallback.GetEmptyNode();
            }
            else
            {
                node = NodeGroupFallback.GetEmptyNodeFromChildren();
            }
        }
        AssignNode(timeline, targetEventIndex, node);
    }

    private void AssignNode(BotTimeline timeline, int targetEventIndex, BotNode node)
    {
        BotEvent? tempEvent = null;

        if (node != null && targetEventIndex > -1 && timeline.events != null && timeline.events.Count > targetEventIndex)
        {
            BotEvent targetEvent = timeline.events[targetEventIndex];
            targetEvent.node = node.transform;
            timeline.events[targetEventIndex] = targetEvent;
            tempEvent = timeline.events[targetEventIndex];
        }
        else if (node != null && targetEventIndex < 0 && timeline.events != null && timeline.events.Count > 0)
        {
            for (int i = 0; i < timeline.events.Count; i++)
            {
                BotEvent targetEvent = timeline.events[i];

                if (targetEvent.dynamic)
                {
                    targetEvent.node = node.transform;
                }

                timeline.events[i] = targetEvent;
                tempEvent = timeline.events[i];
            }
        }
        done = true;

        if (log)
            Debug.Log($"[SetDynamicBotNode ({gameObject.name})] Setting events '{tempEvent?.name}' (index {tempEvent?.index}) node to {node?.name}");
    }

    private IEnumerator IE_AssignNode(BotTimeline timeline, int targetEventIndex, BotNode node, WaitForSeconds wait)
    {
        timeline.paused = true;
        if (reduceWaitTimeFromTimeline)
        {
            BotEvent e = timeline.events[timeline.events.Count - 1];
            e.waitAtNode -= waitTime;
            timeline.events[timeline.events.Count - 1] = e;
        }
        yield return wait;
        if (m_type == DynamicNodeType.priority)
            AssignNodeTypePriority(timeline, targetEventIndex);
        else if (node != null)
            AssignNode(timeline, targetEventIndex, node);
        timeline.paused = false;
        ieSetNode = null;
    }

    private BotNode TryGetNodeFromGroup(BotNodeGroup group, BotTimeline timeline, bool reverse)
    {
        if (log)
            Debug.Log($"[SetDynamicBotNode ({gameObject.name})] Trying to get node from group {group?.gameObject.name} for timeline {timeline?.gameObject.name}");
        BotNode node = null;
        bool setLogging = false;
        if (log && !group.log)
        {
            setLogging = true;
            group.log = true;
        }
        if (!reverse)
            node = group.GetHighestPriorityNodeForSector(timeline.bot.state.sector);
        else
            node = group.GetLowestPriorityNodeForSector(timeline.bot.state.sector);
        if (setLogging)
            group.log = false;

        if (log && node == null)
            Debug.LogWarning($"[SetDynamicBotNode ({gameObject.name})] Was not able to find a node from group {group?.gameObject.name} for timeline {timeline?.gameObject.name}!");

        return node;
    }

    public bool HasFinished()
    {
        return done;
    }
}
