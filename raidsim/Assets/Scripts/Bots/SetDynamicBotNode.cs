using System.Collections;
using System.Xml.Serialization;
using NaughtyAttributes;
using UnityEngine;
using static BotTimeline;

public class SetDynamicBotNode : MonoBehaviour
{
    public BotNodeGroup nodeGroup;
    public BotNodeGroup NodeGroupFallback;
    public bool reversePriority = false;
    private bool wasReversePriority = false;
    public bool reversePriorityFallback = false;
    private bool wasReversePriorityFallback = false;
    public bool waitIfPriorityLowerThan = false;
    private bool wasWaitIfPriorityLowerThan = false;
    public bool reverseIfPriorityLowerThan = false;
    private bool wasReverseIfPriorityLowerThan = false;
    public int priority = -1;
    private int wasPriority = -1;
    [ShowIf("waitIfPriorityLowerThan")] public float waitTime = 0f;
    [ShowIf("waitIfPriorityLowerThan")] public bool reduceWaitTimeFromTimeline = false;
    public int targetTimelineEventIndex = -1;
    public bool log = false;

    private Coroutine ieSetNode;

    private void Awake()
    {
        wasReversePriority = reversePriority;
        wasReversePriorityFallback = reversePriorityFallback;
        wasWaitIfPriorityLowerThan = waitIfPriorityLowerThan;
        wasReverseIfPriorityLowerThan = reverseIfPriorityLowerThan;
        wasPriority = priority;
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
                        ieSetNode = StartCoroutine(IE_AssignNode(timeline, targetEventIndex, new WaitForSeconds(waitTime)));
                        if (log)
                            Debug.Log($"[SetDynamicBotNode ({gameObject.name})] Setting node in {waitTime} seconds");
                    }
                }
                else
                {
                    AssignNode(timeline, targetEventIndex);
                }
            }
            else
            {
                AssignNode(timeline, targetEventIndex);
            }
        }
        else
        {
            AssignNode(timeline, targetEventIndex);
        }
    }

    private void AssignNode(BotTimeline timeline, int targetEventIndex)
    {
        BotNode node = null;

        node = TryGetNodeFromGroup(nodeGroup, timeline, reversePriority);

        if (node == null && NodeGroupFallback != null)
        {
            node = TryGetNodeFromGroup(NodeGroupFallback, timeline, reversePriorityFallback);
        }

        if (node != null && targetEventIndex > -1 && timeline.events != null && timeline.events.Count <= targetEventIndex)
        {
            BotEvent targetEvent = timeline.events[targetEventIndex];
            targetEvent.node = node.transform;
            timeline.events[targetEventIndex] = targetEvent;
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
            }
        }

        if (log)
            Debug.Log($"[SetDynamicBotNode ({gameObject.name})] Setting node to {node?.name}");
    }

    private IEnumerator IE_AssignNode(BotTimeline timeline, int targetEventIndex, WaitForSeconds wait)
    {
        timeline.paused = true;
        if (reduceWaitTimeFromTimeline)
        {
            BotEvent e = timeline.events[timeline.events.Count - 1];
            e.waitAtNode -= waitTime;
            timeline.events[timeline.events.Count - 1] = e;
        }
        yield return wait;
        AssignNode(timeline, targetEventIndex);
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
}
