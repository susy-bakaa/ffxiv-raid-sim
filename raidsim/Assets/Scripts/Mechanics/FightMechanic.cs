using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ActionController;
using static GlobalData;

public class FightMechanic : MonoBehaviour
{
    [Header("Basic Settings")]
    public bool mechanicEnabled = true;
    public string mechanicName = string.Empty;
    public bool onlyTriggerOnce = false;
    public bool log;

    private bool triggered = false;

    private void Awake()
    {
        if (FightTimeline.Instance == null)
            return;

        if (FightTimeline.Instance.log)
        {
            log = true;
        }
    }

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
        {
            string nameToLog = string.IsNullOrEmpty(mechanicName) ? "Unnamed mechanic" : mechanicName;
            Debug.Log($"[FightMechanic] '{nameToLog}' ({gameObject.name}) is not enabled! Interruption aborted.");
            return;
        }
        if (log)
        {
            string nameToLog = string.IsNullOrEmpty(mechanicName) ? "Unnamed mechanic" : mechanicName;
            Debug.Log($"[FightMechanic] '{nameToLog}' ({gameObject.name}) was interrupted with ActionInfo (action: '{actionInfo.action?.gameObject}', source: '{actionInfo.source?.characterName}', target: '{actionInfo.target?.characterName}', targetIsPlayer: '{actionInfo.targetIsPlayer}')");
        }
    }

    protected bool CanTrigger(ActionInfo actionInfo)
    {
        if (!mechanicEnabled && log)
        {
            string nameToLog = string.IsNullOrEmpty(mechanicName) ? "Unnamed mechanic" : mechanicName;
            Debug.Log($"[FightMechanic] '{nameToLog}' ({gameObject.name}) is not enabled! Execution aborted.");
            return false;
        }
        if (triggered && onlyTriggerOnce)
        {
            if (log)
            {
                string nameToLog = string.IsNullOrEmpty(mechanicName) ? "Unnamed mechanic" : mechanicName;
                Debug.Log($"[FightMechanic] '{nameToLog}' ({gameObject.name}) has already been triggered once! Execution aborted.");
            }
            return false;
        }
        triggered = true;
        if (log)
        {
            string nameToLog = string.IsNullOrEmpty(mechanicName) ? "Unnamed mechanic" : mechanicName;
            Debug.Log($"[FightMechanic] '{nameToLog}' ({gameObject.name}) triggered with ActionInfo (action: '{actionInfo.action?.gameObject}', source: '{actionInfo.source?.characterName}', target: '{actionInfo.target?.characterName}', targetIsPlayer: '{actionInfo.targetIsPlayer}')");
        }
        return true;
    }
}
