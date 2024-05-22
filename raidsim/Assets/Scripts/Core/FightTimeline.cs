using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class FightTimeline : MonoBehaviour
{
    public static FightTimeline Instance;

    public List<StatusEffectData> allAvailableStatusEffects = new List<StatusEffectData>();
    public PartyList party;

    [Header("Current")]
    public string timelineName = "Unnamed fight timeline";
    public bool playing = false;
    public bool paused = false;
    public UnityEvent<bool> onPausedChanged;
    public List<BotTimeline> botTimelines = new List<BotTimeline>();
    public List<TimelineEvent> events = new List<TimelineEvent>();

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(Instance.gameObject);
        }
        Instance = this;

        if (party == null)
            party = FindObjectOfType<PartyList>();
    }

    void Update()
    {
        if (paused)
        {
            Time.timeScale = 0f;
        }
        else
        {
            Time.timeScale = 1f;
        }
    }

    public void StartTimeline()
    {
        playing = true;
        StartCoroutine(PlayTimeline());
        for (int i = 0; i < botTimelines.Count; i++)
        {
            if (botTimelines[i].bot != null)
            {
                botTimelines[i].StartTimeline();
            }
        }
    }

    private IEnumerator PlayTimeline()
    {
        for (int i = 0; i < events.Count; i++)
        {
            Debug.Log(events[i].name);
            if (events[i].characterEvents.Count > 0)
            {
                TimelineCharacterEvent[] cEvents = events[i].characterEvents.ToArray();
                for (int e = 0; e < cEvents.Length; e++)
                {
                    // Check for CharacterEvent actions to be performed on this events character
                    if (cEvents[e].actions != null)
                    {
                        if (cEvents[e].performCharacterActions != null && cEvents[e].performCharacterActions.Length > 0)
                        {
                            for(int a = 0; a < cEvents[e].performCharacterActions.Length; a++)
                            {
                                cEvents[e].actions.PerformAction(cEvents[e].performCharacterActions[a]);
                            }
                        }
                    }
                    // Check for CharacterEvent buffs and debuffs to be given to this events character
                    if (cEvents[e].character != null)
                    {
                        if (cEvents[e].giveBuffs != null && cEvents[e].giveBuffs.Length > 0)
                        {
                            for (int b = 0; b < cEvents[e].giveBuffs.Length; b++)
                            {
                                cEvents[e].character.AddEffect(cEvents[e].giveBuffs[b]);
                            }
                        }
                        if (cEvents[e].giveDebuffs != null && cEvents[e].giveDebuffs.Length > 0)
                        {
                            for (int d = 0; d < cEvents[e].giveDebuffs.Length; d++)
                            {
                                cEvents[e].character.AddEffect(cEvents[e].giveDebuffs[d]);
                            }
                        }
                    }
                    // Check for CharacterEvent movement to be performed for this events character
                    if (cEvents[e].controller != null)
                    {

                    }
                    // Check for CharacterEvent fight mechanics performed for this event
                    if (cEvents[e].triggerMechanics != null && cEvents[e].triggerMechanics.Length > 0)
                    {
                        for (int m = 0; m < cEvents[e].triggerMechanics.Length; m++)
                        {
                            cEvents[e].triggerMechanics[m].TriggerMechanic(new ActionController.ActionInfo());
                        }
                    }
                }
            }
            else
            {
                Debug.Log($"No TimelineCharacterEvents found for this timeline event! ({events[i].name})");
            }
            yield return new WaitForSeconds(events[i].time);
        }

        playing = false;
        Debug.Log($"Fight timeline {timelineName} finished!");
    }

    public void WipeParty()
    {
        for (int i = 0; i < party.members.Count; i++)
        {
            party.members[i].ModifyHealth(0, true);
        }
    }

    public void TogglePause()
    {
        TogglePause(!paused);
    }

    public void TogglePause(bool state)
    {
        onPausedChanged.Invoke(state);
        paused = state;
    }

    [System.Serializable]
    public struct TimelineEvent
    {
        public string name;
        public float time;
        public List<TimelineCharacterEvent> characterEvents;

        public TimelineEvent(string name, float time, List<TimelineCharacterEvent> characterEvents)
        {
            this.name = name;
            this.time = time;
            this.characterEvents = characterEvents;
        }
    }

    [System.Serializable]
    public struct TimelineCharacterEvent
    {
        public string name;
        public CharacterState character;
        public ActionController actions;
        public AIController controller;
        public StatusEffectData[] giveBuffs;
        public StatusEffectData[] giveDebuffs;
        public CharacterAction[] performCharacterActions;
        public FightMechanic[] triggerMechanics;

        public TimelineCharacterEvent(string name, CharacterState character, ActionController actions, AIController controller, StatusEffectData[] giveBuffs, StatusEffectData[] giveDebuffs, CharacterAction[] performCharacterActions, FightMechanic[] triggerMechanics)
        {
            this.name = name;
            this.character = character;
            this.actions = actions;
            this.controller = controller;
            this.giveBuffs = giveBuffs;
            this.giveDebuffs = giveDebuffs;
            this.performCharacterActions = performCharacterActions;
            this.triggerMechanics = triggerMechanics;
        }
    }
}
