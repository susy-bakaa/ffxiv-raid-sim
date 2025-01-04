using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using static GlobalData;
using static PartyList;
using static StatusEffectData;

public class NextBotTimeline : MonoBehaviour
{
    public enum choiceType { statusEffect, fightTimelineEventRandomResult, partyListPriority, none }

    [ShowIf("TypeEnum", choiceType.statusEffect)] public List<StatusEffectInfo> effects = new List<StatusEffectInfo>();
    [HideIf("TypeEnum", choiceType.none)] public List<IndexMapping> indexMapping = new List<IndexMapping>();
    [HideIf("TypeEnum", choiceType.none)] public List<IndexMapping> indexMapping2 = new List<IndexMapping>();
    [HideIf("TypeEnum", choiceType.none)] public int fightTimelineEventRandomResultId = -1;
    [HideIf("TypeEnum", choiceType.none)] public int fightTimelineEventRandomResultId2 = -1;
    public List<BotTimeline> timelines = new List<BotTimeline>();
    public choiceType type = choiceType.statusEffect;
    private choiceType TypeEnum { get { return type; } }
    [ShowIf("TypeEnum", choiceType.partyListPriority)] public PartyList partyList;
    [HideIf("TypeEnum", choiceType.none)] public bool fallbackToLast = false;
    [ShowIf("TypeEnum", choiceType.statusEffect)] public bool allowSubStatuses = false;
    [ShowIf("TypeEnum", choiceType.statusEffect)] public bool combineWithRandomEventResult = false;
    [ShowIf("TypeEnum", choiceType.statusEffect)] public bool useIndexMappingForEffects = false;
    [ShowIf("TypeEnum", choiceType.partyListPriority)] public bool useStatusEffectForPriorityCheck = false;
    [ShowIf("TypeEnum", choiceType.partyListPriority)] public bool looseStatusCheck = false;
    [ShowIf("TypeEnum", choiceType.fightTimelineEventRandomResult)] public bool useDoubleEventCheck = false;
    [HideIf("TypeEnum", choiceType.none)] public bool useIndexMapping = false;
    [ShowIf("TypeEnum", choiceType.none)] public bool random = false;
    public bool endIfDisabled = true;
    public bool forceEnd = false;
    public bool log = false;

    public void Choose(BotTimeline old)
    {
        if (forceEnd)
            return;

        if (endIfDisabled && !old.bot.gameObject.activeSelf)
            return;

        AIController bot = null;

        if (old != null)
        {
            bot = old.bot;
        }

        if (log || (bot != null && bot.log))
            Debug.Log($"[{gameObject.name}] Choosing next bot timeline...");

        if (bot != null)
        {
            switch (type)
            {
                case choiceType.statusEffect:
                    for (int i = 0; i < effects.Count; i++)
                    {
                        if (log || bot.log)
                            Debug.Log($"[{gameObject.name}] Does bot {bot.state.gameObject.name} have {effects[i].data?.statusName} with tag {effects[i].tag}?");
                        if (bot.state.HasEffect(effects[i].data?.statusName, effects[i].tag))
                        {
                            int result = i;

                            if (useIndexMapping && useIndexMappingForEffects)
                            {
                                for (int j = 0; j < indexMapping.Count; j++)
                                {
                                    if (indexMapping[j].previousIndex == i)
                                    {
                                        result = indexMapping[j].nextIndex;
                                        break;
                                    }
                                }
                            }
                            if (combineWithRandomEventResult && fightTimelineEventRandomResultId > -1)
                            {
                                if (useIndexMapping && !useIndexMappingForEffects)
                                {
                                    for (int j = 0; j < indexMapping.Count; j++)
                                    {
                                        if (indexMapping[j].previousIndex == FightTimeline.Instance.GetRandomEventResult(fightTimelineEventRandomResultId))
                                        {
                                            result += indexMapping[j].nextIndex;
                                            break;
                                        }
                                    }
                                }
                                else
                                {
                                    result += FightTimeline.Instance.GetRandomEventResult(fightTimelineEventRandomResultId);
                                }
                            }

                            timelines[result].bot = bot;
                            bot.botTimeline = timelines[result];
                            timelines[result].StartTimeline();
                            if (log || bot.log)
                                Debug.Log($"[{gameObject.name}] --> Yes, Next bot timeline has been chosen from main status effect {effects[result].name} as {timelines[result].gameObject.name}!");
                            return;
                        } 
                        else if (!bot.state.HasEffect(effects[i].data?.statusName, effects[i].tag) && effects[i].data == null)
                        {
                            int result = i;

                            if (useIndexMapping && useIndexMappingForEffects)
                            {
                                for (int j = 0; j < indexMapping.Count; j++)
                                {
                                    if (indexMapping[j].previousIndex == i)
                                    {
                                        result = indexMapping[j].nextIndex;
                                        break;
                                    }
                                }
                            }
                            if (combineWithRandomEventResult && fightTimelineEventRandomResultId > -1)
                            {
                                if (useIndexMapping && !useIndexMappingForEffects)
                                {
                                    for (int j = 0; j < indexMapping.Count; j++)
                                    {
                                        if (indexMapping[j].previousIndex == FightTimeline.Instance.GetRandomEventResult(fightTimelineEventRandomResultId))
                                        {
                                            result += indexMapping[j].nextIndex;
                                            break;
                                        }
                                    }
                                }
                                else
                                {
                                    result += FightTimeline.Instance.GetRandomEventResult(fightTimelineEventRandomResultId);
                                }
                            }

                            timelines[result].bot = bot;
                            bot.botTimeline = timelines[result];
                            timelines[result].StartTimeline();
                            if (log || bot.log)
                                Debug.Log($"[{gameObject.name}] --> Yes, Next bot timeline has been chosen from a lack of any status effect '{effects[result].name}' as {timelines[result].gameObject.name}!");
                            return;
                        }

                        if (allowSubStatuses)
                        {
                            if (effects[i].data.refreshStatusEffects != null && effects[i].data.refreshStatusEffects.Count > 0)
                            {
                                for (int k = 0; k < effects[i].data.refreshStatusEffects.Count; k++)
                                {
                                    if (bot.state.HasEffect(effects[i].data.refreshStatusEffects[k].statusName))
                                    {
                                        timelines[i].bot = bot;
                                        bot.botTimeline = timelines[i];
                                        timelines[i].StartTimeline();
                                        if (log || bot.log)
                                            Debug.Log($"[{gameObject.name}] --> Yes, Next bot timeline has been chosen from sub status effect {effects[i].data.refreshStatusEffects[k].statusName} as {timelines[i].gameObject.name}!");
                                        return;
                                    }
                                }
                            }
                        }
                        if (log || bot.log)
                            Debug.Log($"[{gameObject.name}] ---> No, Bot {bot.state.gameObject.name} does not have {effects[i].data.statusName} with tag {effects[i].tag}.");
                    }
                    if (fallbackToLast)
                    {
                        int lastIndex = timelines.Count - 1;
                        timelines[lastIndex].bot = bot;
                        bot.botTimeline = timelines[lastIndex];
                        timelines[lastIndex].StartTimeline();
                        if (log || bot.log)
                            Debug.Log($"[{gameObject.name}] --> Next bot timeline has been chosen as fallback timeline (last timeline available in list) as {timelines[lastIndex].gameObject.name}!");
                        return;
                    }
                    break;
                case choiceType.fightTimelineEventRandomResult:
                    int r = FightTimeline.Instance.GetRandomEventResult(fightTimelineEventRandomResultId);
                    int r2 = FightTimeline.Instance.GetRandomEventResult(fightTimelineEventRandomResultId2);

                    if (useIndexMapping)
                    {
                        for (int i = 0; i < indexMapping.Count; i++)
                        {
                            if (indexMapping[i].previousIndex == r)
                            {
                                r = indexMapping[i].nextIndex;
                                if (log || bot.log)
                                    Debug.Log($"[{gameObject.name}] --- Index Mapping {indexMapping[i].name} of index {i} will result in next index of {r}");
                                break;
                            }
                        }
                        // second
                        if (useDoubleEventCheck)
                        {
                            for (int i = 0; i < indexMapping2.Count; i++)
                            {
                                if (indexMapping2[i].previousIndex == r2)
                                {
                                    r2 = indexMapping2[i].nextIndex;
                                    if (log || bot.log)
                                        Debug.Log($"[{gameObject.name}] --- Second Index Mapping {indexMapping2[i].name} of index {i} will result in next index of {r2}");
                                    break;
                                }
                            }
                            r += r2;
                        }
                    }

                    if (r < timelines.Count && r > -1)
                    {
                        timelines[r].bot = bot;
                        bot.botTimeline = timelines[r];
                        timelines[r].StartTimeline();
                        if (log || bot.log)
                            Debug.Log($"[{gameObject.name}] --> Yes, Next bot timeline has been chosen from fight timeline random event result id of {fightTimelineEventRandomResultId} of result {r} as {timelines[r].gameObject.name}!");
                        return;
                    }

                    if (fallbackToLast)
                    {
                        int lastIndex = timelines.Count - 1;
                        timelines[lastIndex].bot = bot;
                        bot.botTimeline = timelines[lastIndex];
                        timelines[lastIndex].StartTimeline();
                        if (log || bot.log)
                            Debug.Log($"[{gameObject.name}] --> Next bot timeline has been chosen as fallback timeline (last timeline available in list) as {timelines[lastIndex].gameObject.name}!");
                        return;
                    }
                    break;
                case choiceType.partyListPriority:
                    int p = -1;

                    if (partyList != null)
                    {
                        if (useStatusEffectForPriorityCheck && effects.Count > 0)
                        {
                            if (looseStatusCheck)
                            {
                                List<PartyMember> members = partyList.GetPrioritySortedList(effects[0].data);

                                if (members.Count > 0)
                                {
                                    for (int i = 0; i < members.Count; i++)
                                    {
                                        if (members[i].aiController != null)
                                        {
                                            if (members[i].aiController == old.bot)
                                            {
                                                p = i;
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                // Implement later
                            }
                        }
                    }
                    if (p > -1)
                    {
                        if (p < timelines.Count)
                        {
                            timelines[p].bot = bot;
                            bot.botTimeline = timelines[p];
                            timelines[p].StartTimeline();
                            if (log || bot.log)
                                Debug.Log($"[{gameObject.name}] --> Yes, Next bot timeline has been chosen from party list priority {p} as {timelines[p].gameObject.name}!");
                            return;
                        }
                    }
                    if (fallbackToLast)
                    {
                        int lastIndex = timelines.Count - 1;
                        timelines[lastIndex].bot = bot;
                        bot.botTimeline = timelines[lastIndex];
                        timelines[lastIndex].StartTimeline();
                        if (log || bot.log)
                            Debug.Log($"[{gameObject.name}] --> Next bot timeline has been chosen as fallback timeline (last timeline available in list) as {timelines[lastIndex].gameObject.name}!");
                        return;
                    }
                    break;
                case choiceType.none:
                    int n = 0;

                    if (random)
                    {
                        n = Random.Range(0, timelines.Count);
                    }

                    timelines[n].bot = bot;
                    bot.botTimeline = timelines[n];
                    timelines[n].StartTimeline();
                    if (log || bot.log)
                        Debug.Log($"[{gameObject.name}] --> Next bot timeline has been chosen as {timelines[n].gameObject.name}!");
                    return;
            }
            if (log || bot.log)
                Debug.Log($"[{gameObject.name}] --> No, Next bot timeline has not been chosen! Ending execution of bot timeline for {bot.state.gameObject.name}.");
        }
    }
}
