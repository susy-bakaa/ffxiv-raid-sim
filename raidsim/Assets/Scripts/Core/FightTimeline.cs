using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static TimelineData;

public class FightTimeline : MonoBehaviour
{
    public static FightTimeline Instance;

    public List<StatusEffectData> allAvailableStatusEffects = new List<StatusEffectData>();
    public PartyList party;

    [Header("Current")]
    public List<TimelineAction> timelineActions = new List<TimelineAction>();
    public TimelineData timeline;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(Instance.gameObject);
        }
        Instance = this;

        timelineActions.AddRange(transform.GetComponentsInChildren<TimelineAction>());
    }

    public void PlayTimeline()
    {
        StartCoroutine(SimulateTimeline());
    }

    private IEnumerator SimulateTimeline()
    {
        TimelineEvent[] events = timeline.events.ToArray();
        //float lastWaitTime = 0f;

        for (int i = 0; i < events.Length; i++)
        {
            Debug.Log(events[i].name);
            if (events[i].actions.Count > 0)
            {
                for (int k = 0; k < events[i].actions.Count; k++)
                {
                    for (int j = 0; j < timelineActions.Count; j++)
                    {
                        if (timelineActions[j].data == events[i].actions[k])
                        {
                            timelineActions[j].ExecuteAction();
                        }
                    }
                    Debug.Log(events[i].actions[k].actionName);
                }
            }
            else
            {
                Debug.Log($"No actions found for this event! ({events[i].name})");
            }
            yield return new WaitForSeconds(events[i].time);
            //if (i == events.Length - 1)
            //    lastWaitTime = events[i].time;
        }

        //yield return new WaitForSeconds(lastWaitTime);

        Debug.Log("Timeline finished");
    }

    public void WipeParty()
    {
        for (int i = 0; i < party.members.Count; i++)
        {
            party.members[i].ModifyHealth(0, true);
        }
    }
}
