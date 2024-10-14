using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Events;
using static StatusEffectData;

public class BotTimeline : MonoBehaviour
{
    PartyList party;
    ActionController controller;
    TargetController targeting;
    public AIController bot;

    [Header("Timeline")]
    public Transform currentTarget;
    public List<BotEvent> events;

    [Header("Events")]
    public UnityEvent<BotTimeline> onBegin;
    public UnityEvent<BotTimeline> onFinish;

    private int index;

    void Awake()
    {
        index = UnityEngine.Random.Range(1000, 10000);
        onFinish.AddListener(CleanUp);
    }

    public void StartTimeline()
    {
        if (controller == null)
        {
            if (bot.TryGetComponent(out ActionController botAC))
            {
                controller = botAC;
            }
            else
            {
                Debug.LogWarning($"No action controller found for {bot}!");
                return;
            }
        }
        if (targeting == null)
        {
            if (bot.TryGetComponent(out TargetController botTC))
            {
                targeting = botTC;
            }
            else
            {
                Debug.LogWarning($"No target controller found for {bot}!");
                return;
            }
        }
        if (party == null)
        {
            if (bot.TryGetComponent(out CharacterState botCS))
            {
                party = botCS.partyList;
            }
            else
            {
                Debug.LogWarning($"No party list found for {bot}!");
                return;
            }
        }

        if (events != null && events.Count > 0)
        {
            StartCoroutine(PlayTimeline());
            onBegin.Invoke(this);
        }
        else if (events != null)
        {
            onBegin.Invoke(this);
            Utilities.FunctionTimer.Create(this, () => TriggerOnFinish(), 0.1f, $"{index}_botTimeline_no_events_onFinish_delay", true, true);
        }
    }

    private void TriggerOnFinish()
    {
        onFinish.Invoke(this);
    }

    public IEnumerator PlayTimeline()
    {
        for (int i = 0; i < events.Count; i++)
        {
            if (!string.IsNullOrEmpty(events[i].targetStatusEffectHolder.name) && events[i].targetStatusEffectHolder.data != null && party != null)
            {
                foreach (PartyList.PartyMember member in party.members)
                {
                    if (member.characterState.HasEffect(events[i].targetStatusEffectHolder.data.statusName, events[i].targetStatusEffectHolder.tag))
                    {
                        targeting.SetTarget(member.targetController.self);
                        break;
                    }
                    else
                    {
                        targeting.SetTarget(events[i].target);
                    }
                }
            }
            else
            {
                targeting.SetTarget(events[i].target);
            }
            if (events[i].node != null)
            {
                currentTarget = events[i].node.transform;
            }
            else
            {
                currentTarget = null;
            }
            if (events[i].action != null)
            {
                yield return new WaitForSeconds(events[i].waitForAction);
                controller.PerformAction(events[i].action.actionName);
            }
            if (events[i].rotation != Vector3.zero && events[i].waitForRotation > 0)
            {
                yield return new WaitForSeconds(events[i].waitForRotation);
                // set target rotation or something
            }
            yield return new WaitForSeconds(events[i].waitAtNode);
        }
        onFinish.Invoke(this);
    }

    public void CleanUp(BotTimeline botTimeline = null)
    {
        /*Utilities.FunctionTimer.Create(this, () =>
        {
            bot = null;
            controller = null;
            currentTarget = null;
        }, 1f, $"{gameObject.name}_clean_up_delay", false, true);*/
    }

    [System.Serializable]
    public struct BotEvent
    {
        public string name;
        [HideIf("clockSpot")] public Transform node;
        public float waitAtNode;
        public CharacterActionData action;
        public float waitForAction;
        public Vector3 rotation;
        public float waitForRotation;
        public TargetNode target;
        public StatusEffectInfo targetStatusEffectHolder;

        public BotEvent(string name, Transform node, float waitAtNode, CharacterActionData action, float waitForAction, Vector3 rotation, float waitForRotation, TargetNode cycleTarget, StatusEffectInfo targetStatusEffectHolder)
        {
            this.name = name;
            this.node = node;
            this.waitAtNode = waitAtNode;
            this.action = action;
            this.waitForAction = waitForAction;
            this.rotation = rotation;
            this.waitForRotation = waitForRotation;
            this.target = cycleTarget;
            this.targetStatusEffectHolder = targetStatusEffectHolder;
        }
    }
}
