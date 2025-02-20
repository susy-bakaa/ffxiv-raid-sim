using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static BotTimeline;

public class BotTimelineChoice : MonoBehaviour
{
    public List<BotTimeline> availableTimelines = new List<BotTimeline>();
    public bool ChooseDisabled = true;

    public void Choose(BotTimeline timeline)
    {
        if (availableTimelines == null || availableTimelines.Count < 1)
            return;

        if (ChooseDisabled)
        {
            for (int i = 0; i < availableTimelines.Count; i++)
            {
                if (!availableTimelines[i].bot.gameObject.activeSelf)
                {
                    availableTimelines[i].bot = timeline.bot;
                    availableTimelines[i].bot.botTimeline = availableTimelines[i];
                    availableTimelines[i].StartTimeline();

                    if (availableTimelines[i].events != null && availableTimelines[i].events.Count > 0)
                    {
                        BotEvent e = availableTimelines[i].events[0];
                        e.waitAtNode -= 0.1f;
                        availableTimelines[i].events[0] = e;
                    }
                    return;
                }
            }
        }
    }
}
