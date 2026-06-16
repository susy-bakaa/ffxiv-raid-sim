using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using dev.susybaka.raidsim.Nodes;
using static dev.susybaka.raidsim.StatusEffects.StatusEffectData;
using dev.susybaka.raidsim.Core;

namespace dev.susybaka.raidsim.Bots
{
    public class ResolveDynamicBotNodes : MonoBehaviour
    {
        private enum PickType { Random, BasedOnDebuff, BasedOnMultipleDebuffs, BasedOnCharacterEvent }

        [SerializeField] private PickType type = PickType.Random;
        [SerializeField, ShowIf(nameof(type), PickType.BasedOnDebuff)] private List<StatusEffectContext> effects = new();
        [SerializeField, ShowIf(nameof(type), PickType.BasedOnMultipleDebuffs)] private List<StatusEffectContextArray> multipleEffects = new();
        [SerializeField, ShowIf(nameof(type), PickType.BasedOnCharacterEvent), Min(0)] private int characterEventId = 0;
        [SerializeField, ShowIf(nameof(type), PickType.BasedOnCharacterEvent)] private List<int> characterEventResults = new();
        [SerializeField] private List<NodeResolveGroup> nodes = new();

        public void Resolve(BotTimeline timeline)
        {
            if (effects == null || nodes == null || effects.Count < 1 || nodes.Count < 1)
            {
                Debug.LogWarning($"[ResolveDynamicBotNodes ({gameObject.name})] Cannot resolve dynamic nodes, missing effects or node groups.", gameObject);
                return;
            }

            switch (type)
            {
                case PickType.Random:
                    int randomIndex = FightTimeline.Instance.random.Pick($"ResolveDynamicBotNodes_RandomIndex_{gameObject.name}", nodes.Count, FightTimeline.Instance.GlobalRngMode);
                    ResolveNodesForGroup(timeline, nodes[randomIndex]);
                    break;
                case PickType.BasedOnDebuff:
                    ResolveBasedOnDebuff(timeline);
                    break;
                case PickType.BasedOnMultipleDebuffs:
                    ResolveBasedOnMultipleDebuffs(timeline);
                    break;
                case PickType.BasedOnCharacterEvent:
                    ResolveBasedOnCharacterEvent(timeline);
                    break;
            }
        }

        private void ResolveBasedOnDebuff(BotTimeline timeline)
        {
            int noDebuffIndex = -1;
            bool debuffFound = false;

            for (int i = 0; i < effects.Count; i++)
            {
                if (effects[i].data == null)
                {
                    noDebuffIndex = i;
                    continue;
                }

                if (timeline.bot.state.HasEffect(effects[i].data != null ? effects[i].data.statusName : effects[i].name, effects[i].tag))
                {
                    int groupIndex = Mathf.Min(i, nodes.Count - 1);
                    ResolveNodesForGroup(timeline, nodes[groupIndex]);
                    debuffFound = true;
                    break;
                }
            }

            if (!debuffFound && noDebuffIndex > -1)
            {
                int groupIndex = Mathf.Min(noDebuffIndex, nodes.Count - 1);
                ResolveNodesForGroup(timeline, nodes[groupIndex]);
                debuffFound = true;
            }
        }

        private void ResolveBasedOnMultipleDebuffs(BotTimeline timeline)
        {
            int noDebuffIndex = -1;
            bool debuffFound = false;

            for (int i = 0; i < multipleEffects.Count; i++)
            {
                bool hasEffect = true;
                bool empty = true;

                if (multipleEffects[i].effectInfos == null || multipleEffects[i].effectInfos.Length < 1)
                {
                    noDebuffIndex = i;
                    hasEffect = false;
                    continue;
                }

                foreach (StatusEffectContext effect in multipleEffects[i].effectInfos)
                {
                    if (effect.data != null)
                    {
                        empty = false;
                    }
                    else
                    {
                        continue;
                    }

                    if (!timeline.bot.state.HasEffect(effect.data != null ? effect.data.statusName : effect.name, effect.tag))
                    {
                        hasEffect = false;
                        break;
                    }
                }

                if (empty)
                {
                    noDebuffIndex = i;
                    continue;
                }

                if (hasEffect)
                {
                    int groupIndex = Mathf.Min(i, nodes.Count - 1);
                    ResolveNodesForGroup(timeline, nodes[groupIndex]);
                    debuffFound = true;
                    break;
                }
            }

            if (!debuffFound && noDebuffIndex > -1)
            {
                int groupIndex = Mathf.Min(noDebuffIndex, nodes.Count - 1);
                ResolveNodesForGroup(timeline, nodes[groupIndex]);
                debuffFound = true;
            }
        }

        private void ResolveBasedOnCharacterEvent(BotTimeline timeline)
        {
            for (int i = 0; i < characterEventResults.Count; i++)
            {
                if (timeline.bot.state.TryGetCharacterEventResult(characterEventId, out int e))
                {
                    if (e == characterEventResults[i])
                    {
                        int groupIndex = Mathf.Min(i, nodes.Count - 1);
                        ResolveNodesForGroup(timeline, nodes[groupIndex]);
                    }
                    break;
                }
            }
        }

        private void ResolveNodesForGroup(BotTimeline timeline, NodeResolveGroup resolveGroup)
        {
            if (resolveGroup.nodes.Count < 1 || resolveGroup.eventIndicies.Count < 1)
                return;

            for (int i = 0; i < timeline.events.Count; i++)
            {
                for (int j = 0; j < resolveGroup.eventIndicies.Count; j++)
                {
                    if (i == resolveGroup.eventIndicies[j])
                    {
                        int nodeIndex = Mathf.Min(j, resolveGroup.nodes.Count - 1);
                        BotTimeline.BotEvent botEvent = timeline.events[i];
                        botEvent.node = resolveGroup.nodes[nodeIndex].transform;
                        timeline.events[i] = botEvent;
                    }
                }
            }
        }

        [System.Serializable]
        private struct NodeResolveGroup
        {
            public string name;
            public List<BotNode> nodes;
            public List<int> eventIndicies;
        }
    }
}