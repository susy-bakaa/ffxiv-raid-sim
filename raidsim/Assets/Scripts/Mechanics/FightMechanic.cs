using UnityEngine;
using dev.susybaka.raidsim.Bots;
using dev.susybaka.raidsim.Characters;
using dev.susybaka.raidsim.Core;
using dev.susybaka.raidsim.Nodes;
using static dev.susybaka.raidsim.Core.GlobalData;

namespace dev.susybaka.raidsim.Mechanics
{
    public class FightMechanic : MonoBehaviour
    {
        [Header("Basic Settings")]
        public bool mechanicEnabled = true;
        public string mechanicName = string.Empty;
        public bool onlyTriggerOnce = false;
        public bool global = false;
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

            FightTimeline.Instance.onReset.AddListener(InterruptMechanic);
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

        public virtual void TriggerMechanic(MechanicNode node)
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
            triggered = false;
        }

        protected bool CanTrigger()
        {
            return CanTrigger(new ActionInfo(null, null, null));
        }

        protected bool CanTrigger(ActionInfo actionInfo)
        {
            if (global && onlyTriggerOnce)
            {
                if (FightTimeline.Instance != null && !string.IsNullOrEmpty(mechanicName))
                {
                    if (FightTimeline.Instance.executedMechanics.Contains(mechanicName))
                    {
                        if (log)
                            Debug.Log($"[DelayedMechanic ({gameObject.name})] Global mechanic {mechanicName} already triggered");
                        return false;
                    }
                    FightTimeline.Instance.executedMechanics.Add(mechanicName);
                }
            }
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
}