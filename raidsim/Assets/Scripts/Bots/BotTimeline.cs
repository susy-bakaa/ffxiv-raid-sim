using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;
using static GlobalData;
using static StatusEffectData;

public class BotTimeline : MonoBehaviour
{
    PartyList party;
    ActionController controller;
    TargetController targeting;
    public AIController bot;

    [Header("Timeline")]
    public Transform currentTarget;
    public Sector sector;
    public bool updateSector = false;
    public bool skipExtraDelay = false;
    public bool paused = false;
    public List<BotEvent> events;

    [Header("Events")]
    public UnityEvent<BotTimeline> onBegin;
    public UnityEvent<BotTimeline> onFinish;

    private int index;
    public bool TeleportAfterClose { get; private set; }
    private float reduceWaitTime = 0f;

    void Awake()
    {
        index = UnityEngine.Random.Range(1000, 10000);
        reduceWaitTime = 0f;
        if (FightTimeline.Instance != null && FightTimeline.Instance.log)
            onBegin.AddListener((BotTimeline timeline) => { Debug.Log($"[BotTimeline ({timeline.gameObject})] {timeline.bot.name} started timeline {gameObject.name}"); });
    }

    public void StartTimeline()
    {
        if (!bot.gameObject.activeSelf)
            return;

        if (bot != null && updateSector)
        {
            bot.state.sector = sector;
        }
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
                if (updateSector && party != null)
                    party.UpdatePartyList();
            }
            else
            {
                Debug.LogWarning($"No party list found for {bot}!");
                return;
            }
        }

        if (events != null && events.Count > 0)
        {
            onBegin.Invoke(this);
            StartCoroutine(IE_PlayTimeline());
        }
        else if (events != null)
        {
            onBegin.Invoke(this);
            if (!skipExtraDelay)
                Utilities.FunctionTimer.Create(this, () => TriggerOnFinish(), 0.1f, $"{index}_botTimeline_no_events_onFinish_delay", true, true);
            else
                TriggerOnFinish();
        }
    }

    private void TriggerOnFinish()
    {
        onFinish.Invoke(this);
    }

    public IEnumerator IE_PlayTimeline()
    {
        for (int i = 0; i < events.Count; i++)
        {
            if (paused)
            {
                yield return new WaitUntil(() => !paused);
            }
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
            if (events[i].teleportAfterCloseEnough)
            {
                TeleportAfterClose = true;
            }
            else
            {
                TeleportAfterClose = false;
            }
            if (events[i].action != null)
            {
                if (events[i].waitForAction > 0f)
                {
                    yield return new WaitForSeconds(events[i].waitForAction);
                }
                if (!events[i].unrestrictedAction)
                    controller.PerformAction(events[i].action.actionName);
                else
                    controller.PerformActionHidden(events[i].action.actionName);
                if (FightTimeline.Instance.log)
                    Debug.Log($"[BotTimeline] {controller.gameObject.name} perform action {events[i].action.actionName}");
            }
            if (events[i].faceTowards != null)
            {
                if (events[i].waitForRotation > 0)
                {
                    yield return new WaitForSeconds(events[i].waitForRotation);
                }
                bot.transform.LookAt(events[i].faceTowards);
                bot.transform.eulerAngles = new Vector3(0, bot.transform.eulerAngles.y, 0);
            }
            else if (events[i].faceAway != null)
            {
                if (events[i].waitForRotation > 0)
                {
                    yield return new WaitForSeconds(events[i].waitForRotation);
                }
                bot.transform.LookAt(bot.transform.position - (events[i].faceAway.position - bot.transform.position));
                //bot.transform.LookAt(events[i].faceAway);
                bot.transform.eulerAngles = new Vector3(0, bot.transform.eulerAngles.y, 0);
            }
            else if (events[i].rotation != Vector3.zero && events[i].waitForRotation > 0)
            {
                if (events[i].waitForRotation > 0f)
                {
                    yield return new WaitForSeconds(events[i].waitForRotation);
                }
                // set target rotation or something
            }
            float finalWait = events[i].waitAtNode;
            if (reduceWaitTime > 0f)
            {
                finalWait -= reduceWaitTime;
            }
            if (events[i].randomWaitVariance > 0f)
            {
                finalWait = events[i].waitAtNode + Random.Range(-events[i].randomWaitVariance, events[i].randomWaitVariance);
            }
            if (finalWait > 0f)
            {
                yield return new WaitForSeconds(finalWait);
            }
            else
            {
                yield return null;
            }
        }
        onFinish.Invoke(this);
    }

    public void ResetTimeline()
    {
        StopAllCoroutines();
        bot = null;
        controller = null;
        targeting = null;
        party = null;
        currentTarget = null;
        reduceWaitTime = 0f;

        if (events != null && events.Count > 0)
        {
            for (int i = 0; i < events.Count; i++)
            {
                if (events[i].dynamic)
                {
                    BotEvent e = events[i];
                    e.node = null;
                    events[i] = e;
                }
            }
        }
    }

    public void SetReducedWaitTime(float time)
    {
        reduceWaitTime = time;
    }

    [System.Serializable]
    public struct BotEvent
    {
        public string name;
        [HideIf("clockSpot")] public Transform node;
        public bool dynamic;
        public float waitAtNode;
        public float randomWaitVariance;
        public bool teleportAfterCloseEnough;
        public CharacterActionData action;
        public bool unrestrictedAction;
        public float waitForAction;
        public Vector3 rotation;
        public Transform faceTowards;
        public Transform faceAway;
        public float waitForRotation;
        public TargetNode target;
        public StatusEffectInfo targetStatusEffectHolder;

        public BotEvent(string name, Transform node, bool dynamic, float waitAtNode, float randomWaitVariance, bool teleportAfterCloseEnough, CharacterActionData action, bool unrestrictedAction, float waitForAction, Vector3 rotation, Transform faceTowards, Transform faceAway, float waitForRotation, TargetNode cycleTarget, StatusEffectInfo targetStatusEffectHolder)
        {
            this.name = name;
            this.node = node;
            this.dynamic = dynamic;
            this.waitAtNode = waitAtNode;
            this.randomWaitVariance = randomWaitVariance;
            this.teleportAfterCloseEnough = teleportAfterCloseEnough;
            this.action = action;
            this.unrestrictedAction = unrestrictedAction;
            this.waitForAction = waitForAction;
            this.rotation = rotation;
            this.faceTowards = faceTowards;
            this.faceAway = faceAway;
            this.waitForRotation = waitForRotation;
            this.target = cycleTarget;
            this.targetStatusEffectHolder = targetStatusEffectHolder;
        }
    }
}
