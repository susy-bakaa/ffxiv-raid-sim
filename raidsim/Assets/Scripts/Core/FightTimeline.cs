using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static TimelineData;

public class FightTimeline : MonoBehaviour
{
    public static FightTimeline Instance;

    public List<StatusEffectData> allAvailableStatusEffects = new List<StatusEffectData>();
    public List<CharacterState> players = new List<CharacterState>();

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

        for (int i = 0; events.Length > 0; i++)
        {
            yield return new WaitForSeconds(events[i].time);
            for (int k = 0; k < events[i].actions.Count; k++)
            {
                for (int j = 0; j < timelineActions.Count; j++)
                {
                    if (timelineActions[j] == events[i].actions[k])
                    {
                        timelineActions[j].ExecuteAction();
                    }
                }
                Debug.Log(events[i].actions[k].actionName);
            }           
        }

        Debug.Log("Timeline finished");
    }

    public void WipeParty()
    {
        for (int i = 0; i < players.Count; i++)
        {
            players[i].ModifyHealth(-999999);
        }
    }
}
