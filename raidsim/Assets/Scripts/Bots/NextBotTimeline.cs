using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static StatusEffectData;
using static TriggerRandomMechanic;

public class NextBotTimeline : MonoBehaviour
{
    public enum choiceType { statusEffect, fightTimelineEventRandomResult }

    public List<StatusEffectInfo> effects = new List<StatusEffectInfo>();
    public List<IndexMapping> indexMapping = new List<IndexMapping>();
    public int fightTimelineEventRandomResultId = -1;
    public List<BotTimeline> timelines = new List<BotTimeline>();
    public choiceType type = choiceType.statusEffect;
    public bool allowSubStatuses = false;
    public bool useIndexMapping = false;
    public bool endIfDisabled = true;
    public bool log = false;

    public void Choose(BotTimeline old)
    {
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
                            Debug.Log($"[{gameObject.name}] Does bot {bot.state.gameObject.name} have {effects[i].data.statusName} with tag {effects[i].tag}?");
                        if (bot.state.HasEffect(effects[i].data.statusName, effects[i].tag))
                        {
                            timelines[i].bot = bot;
                            bot.botTimeline = timelines[i];
                            timelines[i].StartTimeline();
                            if (log || bot.log)
                                Debug.Log($"[{gameObject.name}] --> Yes, Next bot timeline has been chosen from main status effect {effects[i].data.statusName} as {timelines[i].gameObject.name}!");
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
                    break;
                case choiceType.fightTimelineEventRandomResult:
                    int r = FightTimeline.Instance.GetRandomEventResult(fightTimelineEventRandomResultId);

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
                    break;
            }
            if (log || bot.log)
                Debug.Log($"[{gameObject.name}] --> No, Next bot timeline has not been chosen! Ending execution of bot timeline for {bot.state.gameObject.name}.");
        }
    }
}
