using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static StatusEffectData;

public class NextBotTimeline : MonoBehaviour
{
    public List<StatusEffectInfo> effects = new List<StatusEffectInfo>();
    public List<BotTimeline> timelines = new List<BotTimeline>();
    public bool chooseBasedOnStatusEffect = true;
    public bool endIfDisabled = true;

    public void Choose(BotTimeline old)
    {
        if (endIfDisabled && !old.bot.gameObject.activeSelf)
            return;

        if (old != null)
        {
            AIController bot = old.bot;

            if (chooseBasedOnStatusEffect)
            {
                for (int i = 0; i < effects.Count; i++)
                {
                    if (bot.state.HasEffect(effects[i].data.statusName, effects[i].tag))
                    {
                        timelines[i].bot = bot;
                        bot.botTimeline = timelines[i];
                        timelines[i].StartTimeline();
                    }
                }
            }
        }
    }
}
