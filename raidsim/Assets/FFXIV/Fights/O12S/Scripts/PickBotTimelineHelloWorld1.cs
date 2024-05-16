using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickBotTimelineHelloWorld1 : MonoBehaviour
{
    //public NodeManager nodeManager;
    public List<AIController> controllers = new List<AIController>();
    public List<CharacterState> states = new List<CharacterState>();
    public BotTimeline overflowDebuffTimeline;
    public BotTimeline synchShortDebuffTimeline;
    public BotTimeline synchLongDebuffTimeline;
    public BotTimeline LatentWestDebuffTimeline;
    public bool latent1;
    public BotTimeline LatentCenterDebuffTimeline;
    public bool latent2;
    public BotTimeline LatentEastDebuffTimeline;
    public bool latent3;
    public BotTimeline NoDebuffWestTimeline;
    public bool no1;
    public BotTimeline NoDebuffEastWTimeline;
    public bool no2;

    public void ChooseTimelines()
    {
        for (int i = 0; i < states.Count; i++)
        {
            //nodeManager.SetClockSpot(states[i], "HelloWorldRotSpread");
            if (states[i].HasEffect("CriticalOverflowBug"))
            {
                controllers[i].botTimeline = overflowDebuffTimeline;
            }
            else if (states[i].HasEffect("CriticalSynchronizationBugShort"))
            {
                controllers[i].botTimeline = synchShortDebuffTimeline;
            }
            else if (states[i].HasEffect("CriticalSynchronizationBugLong"))
            {
                controllers[i].botTimeline = synchLongDebuffTimeline;
            }
            else if (states[i].HasEffect("LatentDefect"))
            {
                if (!latent1)
                {
                    latent1 = true;
                    controllers[i].botTimeline = LatentWestDebuffTimeline;
                }
                else if (!latent2)
                {
                    latent2 = true;
                    controllers[i].botTimeline = LatentCenterDebuffTimeline;
                }
                else if (!latent3)
                {
                    latent3 = true;
                    controllers[i].botTimeline = LatentEastDebuffTimeline;
                }
            }
            else
            {
                if (!no1)
                {
                    no1 = true;
                    controllers[i].botTimeline = NoDebuffWestTimeline;
                }
                else if (!no2)
                {
                    no2 = true;
                    controllers[i].botTimeline = NoDebuffEastWTimeline;
                }
            }
        }
    }

    public void PlayTimelines()
    {
        overflowDebuffTimeline.StartTimeline();
        synchShortDebuffTimeline.StartTimeline();
        synchLongDebuffTimeline.StartTimeline();
        LatentWestDebuffTimeline.StartTimeline();
        LatentCenterDebuffTimeline.StartTimeline();
        LatentEastDebuffTimeline.StartTimeline();
        NoDebuffWestTimeline.StartTimeline();
        NoDebuffEastWTimeline.StartTimeline();
    }
}
