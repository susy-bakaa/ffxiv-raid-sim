using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Events;

public class BotTimeline : MonoBehaviour
{
    ActionController controller;
    public AIController bot;

    [Header("Timeline")]
    public Transform currentTarget;
    public List<BotEvent> events;

    [Header("Events")]
    public UnityEvent<BotTimeline> onBegin;
    public UnityEvent<BotTimeline> onFinish;

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

        if (events != null)
        {
            StartCoroutine(PlayTimeline());
            onBegin.Invoke(this);
        }
    }

    public IEnumerator PlayTimeline()
    {
        for (int i = 0; i < events.Count; i++)
        {
            currentTarget = events[i].node.transform;
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

        public BotEvent(string name, Transform node, float waitAtNode, CharacterActionData action, float waitForAction, Vector3 rotation, float waitForRotation)
        {
            this.name = name;
            this.node = node;
            this.waitAtNode = waitAtNode;
            this.action = action;
            this.waitForAction = waitForAction;
            this.rotation = rotation;
            this.waitForRotation = waitForRotation;
        }
    }
}
