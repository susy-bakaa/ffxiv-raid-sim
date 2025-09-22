// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using dev.susybaka.raidsim.Characters;
using dev.susybaka.raidsim.Core;
using dev.susybaka.raidsim.UI;
using dev.susybaka.Shared;
using static dev.susybaka.raidsim.Core.GlobalData;
using static dev.susybaka.raidsim.StatusEffects.StatusEffectData;
using static dev.susybaka.raidsim.UI.PartyList;

namespace dev.susybaka.raidsim.Bots
{
    public class NextBotTimeline : MonoBehaviour
    {
        public enum choiceType { statusEffect, fightTimelineEventRandomResult, partyListPriority, none }

        [ShowIf(nameof(TypeEnum), choiceType.statusEffect)] public List<StatusEffectInfo> effects = new List<StatusEffectInfo>();
        [ShowIf(nameof(TypeEnum), choiceType.statusEffect)] public List<CharacterState> otherBots = new List<CharacterState>();
        private List<CharacterState> originalOtherBots = new List<CharacterState>();
        [ShowIf(nameof(TypeEnum), choiceType.statusEffect)] public List<BotTimeline> otherBotsFromTimelines = new List<BotTimeline>();
        [HideIf(nameof(TypeEnum), choiceType.none)] public List<IndexMapping> indexMapping = new List<IndexMapping>();
        [HideIf(nameof(TypeEnum), choiceType.none)] public List<IndexMapping> indexMapping2 = new List<IndexMapping>();
        [HideIf(nameof(TypeEnum), choiceType.none)] public int fightTimelineEventRandomResultId = -1;
        [HideIf(nameof(TypeEnum), choiceType.none)] public int fightTimelineEventRandomResultId2 = -1;
        public List<BotTimeline> timelines = new List<BotTimeline>();
        public choiceType type = choiceType.statusEffect;
        private choiceType TypeEnum { get { return type; } }
        [ShowIf(nameof(TypeEnum), choiceType.partyListPriority)] public PartyList partyList;
        [HideIf(nameof(TypeEnum), choiceType.none)] public bool fallbackToLast = false;
        [ShowIf(nameof(TypeEnum), choiceType.statusEffect)] public bool allowSubStatuses = false;
        [ShowIf(nameof(TypeEnum), choiceType.statusEffect)] public bool checkOtherBotsInstead = false;
        [ShowIf(nameof(TypeEnum), choiceType.statusEffect)] public bool combineWithRandomEventResult = false;
        [ShowIf(nameof(TypeEnum), choiceType.statusEffect)] public bool useIndexMappingForEffects = false;
        [ShowIf(nameof(checkOtherBotsInstead))] public bool useIndexMappingForOtherBots = false;
        [ShowIf(nameof(TypeEnum), choiceType.partyListPriority)] public bool useStatusEffectForPriorityCheck = false;
        [ShowIf(nameof(TypeEnum), choiceType.partyListPriority)] public bool looseStatusCheck = false;
        [ShowIf(nameof(TypeEnum), choiceType.fightTimelineEventRandomResult)] public bool useDoubleEventCheck = false;
        [HideIf(nameof(TypeEnum), choiceType.none)] public bool useIndexMapping = false;
        [ShowIf(nameof(TypeEnum), choiceType.none)] public bool random = false;
        [ShowIf(nameof(checkOtherBotsInstead))] public bool useOtherBotIndexAsIndexMapping = false;
        [ShowIf(nameof(checkOtherBotsInstead))] public bool useOtherBotIndexAsFinalIndex = false;
        [ShowIf(nameof(checkOtherBotsInstead))] public bool fallBackToPlayerForInactiveBots = true;
        public bool strictMatch = false;
        public bool endIfDisabled = true;
        public bool forceEnd = false;
        public bool log = false;

        private CharacterState player;

        private void Awake()
        {
            if (player == null)
                player = Utilities.FindAnyByName("Player").GetComponent<CharacterState>();

            originalOtherBots = new List<CharacterState>();
            for (int i = 0; i < otherBots.Count; i++)
            {
                originalOtherBots.Add(otherBots[i]);
            }

            if (FightTimeline.Instance != null)
            {
                FightTimeline.Instance.onReset.AddListener(ResetVariables);
            }
        }

        private void ResetVariables()
        {
            if (originalOtherBots == null || originalOtherBots.Count < 1)
                return;

            otherBots = new List<CharacterState>();
            for (int i = 0; i < originalOtherBots.Count; i++)
            {
                otherBots.Add(originalOtherBots[i]);
            }
        }

        public void Choose(BotTimeline old)
        {
            if (forceEnd)
                return;

            if (endIfDisabled && !old.bot.gameObject.activeSelf)
                return;

            if (fallBackToPlayerForInactiveBots && checkOtherBotsInstead && otherBots != null && otherBots.Count > 0)
            {
                for (int i = 0; i < otherBots.Count; i++)
                {
                    if (!otherBots[i].gameObject.activeSelf && !otherBots[i].characterName.ToLower().Contains("hidden"))
                    {
                        if (log || old.bot.log)
                            Debug.Log($"[{gameObject.name}] Bot {otherBots[i].gameObject.name} is inactive. Falling back to the player {player.gameObject.name} ({player.characterName}) instead.");

                        otherBots[i] = player;
                        break;
                    }
                }
            }

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
                            if (!checkOtherBotsInstead)
                            {
                                if (CheckBotForStatusEffect(bot, bot.state, effects[i], i))
                                {
                                    return;
                                }
                            }
                            else if (checkOtherBotsInstead)
                            {
                                if (otherBots != null && otherBots.Count > 0)
                                {
                                    if (log || bot.log)
                                        Debug.Log($"[{gameObject.name}] Does any of the {otherBots.Count} tracked bots have {effects[i].data?.statusName} with tag {effects[i].tag}?");

                                    for (int o = 0; o < otherBots.Count; o++)
                                    {
                                        if (log || bot.log)
                                            Debug.Log($"[{gameObject.name}] Checking bot {otherBots[o].gameObject.name}...\n");

                                        if (CheckBotForStatusEffect(bot, otherBots[o], effects[i], i))
                                        {
                                            return;
                                        }
                                    }
                                }
                                else if (otherBotsFromTimelines != null && otherBotsFromTimelines.Count > 0)
                                {
                                    if (log || bot.log)
                                        Debug.Log($"[{gameObject.name}] Does any of the {otherBotsFromTimelines.Count} tracked timelines have bots that have {effects[i].data?.statusName} with tag {effects[i].tag}?");

                                    for (int o = 0; o < otherBotsFromTimelines.Count; o++)
                                    {
                                        if (log || bot.log)
                                            Debug.Log($"[{gameObject.name}] Checking timeline {otherBotsFromTimelines[o].gameObject.name}...\n");

                                        if (otherBotsFromTimelines[o].bot == null || otherBotsFromTimelines[o].bot.state == null)
                                        {
                                            if (log || bot.log)
                                                Debug.Log($"[{gameObject.name}] Timeline {otherBotsFromTimelines[o].gameObject.name} does not have a valid bot assigned to it!\n");
                                            continue;
                                        }

                                        if (log || bot.log)
                                            Debug.Log($"[{gameObject.name}] Checking bot {otherBotsFromTimelines[o].bot.state.gameObject.name}...\n");

                                        if (CheckBotForStatusEffect(bot, otherBotsFromTimelines[o].bot.state, effects[i], i))
                                        {
                                            return;
                                        }
                                    }
                                }
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
                    case choiceType.fightTimelineEventRandomResult:
                        int r = FightTimeline.Instance.GetRandomEventResult(fightTimelineEventRandomResultId);
                        int r2 = FightTimeline.Instance.GetRandomEventResult(fightTimelineEventRandomResultId2);

                        if (useIndexMapping)
                        {
                            // first
                            if (indexMapping != null && indexMapping.Count > 0)
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
                            // second
                            if (useDoubleEventCheck && indexMapping2 != null && indexMapping2.Count > 0)
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

        private bool CheckBotForStatusEffect(AIController bot, CharacterState state, StatusEffectInfo effect, int i)
        {
            if (state.HasEffect(effect.data?.statusName, effect.tag))
            {
                int result = i;

                if (useIndexMapping && useIndexMappingForEffects && indexMapping != null && indexMapping.Count > 0)
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
                    if (useIndexMapping && !useIndexMappingForEffects && indexMapping != null && indexMapping.Count > 0)
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
                if (checkOtherBotsInstead && useIndexMappingForOtherBots && otherBots != null && otherBots.Count > 0 && otherBots.Contains(state))
                {
                    if (!useOtherBotIndexAsIndexMapping && indexMapping != null && indexMapping.Count > 0)
                    {
                        for (int j = 0; j < indexMapping.Count; j++)
                        {
                            if (indexMapping[j].previousIndex == otherBots.IndexOf(state))
                            {
                                result += indexMapping[j].nextIndex;
                                break;
                            }
                        }
                    }
                    else
                    {
                        result += (otherBots.IndexOf(state) + 1);
                    }
                }
                if (checkOtherBotsInstead && useOtherBotIndexAsFinalIndex && otherBots != null && otherBots.Count > 0)
                {
                    result = otherBots.IndexOf(state);
                }

                timelines[result].bot = bot;
                bot.botTimeline = timelines[result];
                timelines[result].StartTimeline();
                if (log || bot.log)
                    Debug.Log($"[{gameObject.name}] --> Yes, Next bot timeline has been chosen from main status effect {effects[result].name} as {timelines[result].gameObject.name}!");
                return true;
            }
            else if (!state.HasEffect(effects[i].data?.statusName, effects[i].tag) && effects[i].data == null)
            {
                int result = i;

                if (useIndexMapping && useIndexMappingForEffects && indexMapping != null && indexMapping.Count > 0)
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
                    if (useIndexMapping && !useIndexMappingForEffects && indexMapping != null && indexMapping.Count > 0)
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
                if (checkOtherBotsInstead && useIndexMappingForOtherBots && otherBots != null && otherBots.Count > 0 && otherBots.Contains(state))
                {
                    if (!useOtherBotIndexAsIndexMapping && indexMapping != null && indexMapping.Count > 0)
                    {
                        for (int j = 0; j < indexMapping.Count; j++)
                        {
                            if (indexMapping[j].previousIndex == otherBots.IndexOf(state))
                            {
                                result += indexMapping[j].nextIndex;
                                break;
                            }
                        }
                    }
                    else
                    {
                        result += (otherBots.IndexOf(state) + 1);
                    }
                }
                if (checkOtherBotsInstead && useOtherBotIndexAsFinalIndex && otherBots != null && otherBots.Count > 0)
                {
                    result = otherBots.IndexOf(state);
                }
                if (strictMatch)
                {
                    if (log || bot.log)
                        Debug.Log($"[{gameObject.name}] ---> No, since strict matching is enabled and Bot {state.gameObject.name} lacks any status effect '{effects[result].name}' with tag {effects[i].tag}.");
                    return false;
                }

                timelines[result].bot = bot;
                bot.botTimeline = timelines[result];
                timelines[result].StartTimeline();
                if (log || bot.log)
                    Debug.Log($"[{gameObject.name}] --> Yes, Next bot timeline has been chosen from a lack of any status effect '{effects[result].name}' as {timelines[result].gameObject.name}!");
                return true;
            }

            if (allowSubStatuses)
            {
                if (effects[i].data.refreshStatusEffects != null && effects[i].data.refreshStatusEffects.Count > 0)
                {
                    for (int k = 0; k < effects[i].data.refreshStatusEffects.Count; k++)
                    {
                        if (state.HasEffect(effects[i].data.refreshStatusEffects[k].statusName))
                        {
                            timelines[i].bot = bot;
                            bot.botTimeline = timelines[i];
                            timelines[i].StartTimeline();
                            if (log || bot.log)
                                Debug.Log($"[{gameObject.name}] --> Yes, Next bot timeline has been chosen from sub status effect {effects[i].data.refreshStatusEffects[k].statusName} as {timelines[i].gameObject.name}!");
                            return true;
                        }
                    }
                }
            }
            if (log || bot.log)
                Debug.Log($"[{gameObject.name}] ---> No, Bot {state.gameObject.name} does not have {effects[i].data.statusName} with tag {effects[i].tag}.");
            return false;
        }
    }
}