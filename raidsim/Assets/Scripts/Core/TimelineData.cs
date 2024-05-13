using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Timeline", menuName = "FFXIV/New Timeline")]
public class TimelineData : ScriptableObject
{
    public string timelineName = "Unnamed Timeline";
    public float totalDuration = 0f;
    public List<TimelineEvent> events = new List<TimelineEvent>();

    void OnValidate()
    {
        if (events.Count < 1)
        {
            totalDuration = 0f;
        }
        else
        {
            totalDuration = 0f;
            for (int i = 0; i < events.Count; i++)
            {
                TimelineEvent e = events[i];
                if (events[i].actions.Count > 0 && events[i].actions[0] != null)
                    e.name = $"{events[i].actions[0].actionName} for {events[i].time}s";
                events[i] = e;
                totalDuration += events[i].time;
            }
        }
    }

    [System.Serializable]
    public struct TimelineEvent
    {
        public string name;
        public float time;
        public List<TimelineActionData> actions;

        public TimelineEvent(string name, float time, List<TimelineActionData> actions)
        {
            this.name = name;
            this.time = time;
            this.actions = actions;
        }
    }
}
