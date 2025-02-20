using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BotTimelineBranch : MonoBehaviour
{
    public BotTimeline normal;
    public BotTimeline alternative;
    public BotNodeGroup nodeGroup;
    public bool basedOnSectorNodeAvailability = false;
    public bool randomMicroDelay = false;
    
    private SetDynamicBotNode dynamicNodeNormal;
    private SetDynamicBotNode dynamicNodeAlternative;
    private Coroutine ieChooseBranch;

    public void ChooseBranch(BotTimeline timeline)
    {
        if (normal == null || alternative == null)
            return;

        normal.TryGetComponent(out dynamicNodeNormal);
        alternative.TryGetComponent(out dynamicNodeAlternative);

        if (randomMicroDelay)
        {
            if (ieChooseBranch == null)
            {
                ieChooseBranch = StartCoroutine(IE_ChooseBranch(timeline, new WaitForSeconds(Random.Range(0.01f, 0.1f))));
            }
        }
        else
        {
            ChooseBranchInternal(timeline);
        }
    }

    private IEnumerator IE_ChooseBranch(BotTimeline timeline, WaitForSeconds wait)
    {
        yield return wait;
        ChooseBranchInternal(timeline);
        ieChooseBranch = null;
    }

    private void ChooseBranchInternal(BotTimeline timeline)
    {
        if (nodeGroup != null && basedOnSectorNodeAvailability)
        {
            if (nodeGroup.DoesSectorHaveNodesAvailable(timeline.bot.state.sector))
            {
                normal.bot = timeline.bot;
                normal.bot.botTimeline = normal;
                if (dynamicNodeNormal != null)
                {
                    dynamicNodeNormal.SetNode(normal);
                }
                normal.StartTimeline();
            }
            else
            {
                alternative.bot = timeline.bot;
                alternative.bot.botTimeline = alternative;
                if (dynamicNodeAlternative != null)
                {
                    dynamicNodeAlternative.SetNode(alternative);
                }
                alternative.StartTimeline();
            }
        }
    }
}
