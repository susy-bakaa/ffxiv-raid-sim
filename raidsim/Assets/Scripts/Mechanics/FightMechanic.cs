using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ActionController;
using static GlobalStructs;

public class FightMechanic : MonoBehaviour
{
    public bool mechanicEnabled = true;
    public string mechanicName = string.Empty;

    public void TriggerMechanic(CharacterCollection characterCollection)
    {
        for (int i = 0; i < characterCollection.values.Count; i++)
        {
            TriggerMechanic(characterCollection.values[i]);
        }
    }

    public void TriggerMechanicAnimationEvent()
    {
        TriggerMechanic(new ActionInfo(null, null, null));
    }

    public void TriggerMechanic()
    {
        TriggerMechanic(new ActionInfo(null, null, null));
    }

    public void TriggerMechanic(BotTimeline botTimeline)
    {
        TriggerMechanic(new ActionInfo(null, botTimeline.bot.state, null));
    }

    public void TriggerMechanic(CharacterState state)
    {
        TriggerMechanic(new ActionInfo(null, state, null));
    }

    public virtual void TriggerMechanic(ActionInfo actionInfo)
    {
        if (!mechanicEnabled)
            return;
    }

    public void InterruptMechanic(CharacterCollection characterCollection)
    {
        for (int i = 0; i < characterCollection.values.Count; i++)
        {
            InterruptMechanic(characterCollection.values[i]);
        }
    }

    public void InterruptMechanic()
    {
        InterruptMechanic(new ActionInfo(null, null, null));
    }

    public void InterruptMechanic(CharacterState state)
    {
        InterruptMechanic(new ActionInfo(null, state, null));
    }

    public virtual void InterruptMechanic(ActionInfo actionInfo)
    {
        if (!mechanicEnabled)
            return;
    }
}
