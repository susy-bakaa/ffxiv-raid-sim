using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Events;
using static GlobalStructs;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class FightTimeline : MonoBehaviour
{
    public static FightTimeline Instance;

    public List<StatusEffectData> allAvailableStatusEffects = new List<StatusEffectData>();
    public UserInput input;
    public CharacterState player;
    public PartyList partyList;
    public PartyList enemyList;
    public bool partyWiped { private set; get; }
    public bool enemiesWiped { private set; get; }
    public Transform mechanicParent;

    public static float timeScale = 1f;
    public static float time { private set; get; }
    public static float deltaTime { private set; get; }

    [Header("Current")]
    public string timelineName = "Unnamed fight timeline";
    public bool playing = false;
    public bool paused = false;
    public UnityEvent<bool> onPausedChanged;
    public List<BotTimeline> botTimelines = new List<BotTimeline>();
    public List<TimelineEvent> events = new List<TimelineEvent>();

#if UNITY_EDITOR
    [Button("Load All Status Effects")]
    public void LoadEffectsButton()
    {
        allAvailableStatusEffects.Clear();

        // Find all asset GUIDs of type StatusEffectData
        string[] guids = AssetDatabase.FindAssets("t:StatusEffectData");

        foreach (string guid in guids)
        {
            // Load the asset by its GUID
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            StatusEffectData statusEffect = AssetDatabase.LoadAssetAtPath<StatusEffectData>(assetPath);

            if (statusEffect != null)
            {
                allAvailableStatusEffects.Add(statusEffect);
            }
        }

        // Optionally, log the count of loaded assets
        Debug.Log($"Loaded {allAvailableStatusEffects.Count} status effects.");
    }

    [Button("Wipe Party")]
    public void WipePartyButton()
    {
        WipeParty(partyList);
    }

    private void Reset()
    {
        LoadEffectsButton();
        mechanicParent = GameObject.Find("Mechanics").transform;
        input = GetComponentInChildren<UserInput>();
        player = GameObject.Find("Player").GetComponent<CharacterState>();
        if (player != null)
        {
            PartyList[] partyLists = FindObjectsOfType<PartyList>();
            foreach (PartyList pl in partyLists)
            {
                if (pl.members.Contains(player))
                {
                    partyList = pl;
                    break;
                }
            }
        }
    }
#endif

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(Instance.gameObject);
        }
        Instance = this;

        if (mechanicParent == null)
        {
            mechanicParent = GameObject.Find("Mechanics").transform;
        }

        input = GetComponentInChildren<UserInput>();
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

        if (timeScale < 0f)
            timeScale = 0f;

        deltaTime = Time.deltaTime * timeScale;
        time = Time.time * timeScale;
    }

    public void StartTimeline()
    {
        int seed = ((int)System.DateTime.Now.Ticks + Mathf.RoundToInt(Time.time) + System.DateTime.Now.DayOfYear + (int)System.TimeZoneInfo.Local.BaseUtcOffset.Ticks + Mathf.RoundToInt(SystemInfo.batteryLevel) + SystemInfo.graphicsDeviceID + SystemInfo.graphicsDeviceVendorID + SystemInfo.graphicsMemorySize + SystemInfo.processorCount + SystemInfo.processorFrequency + SystemInfo.systemMemorySize) / 6;

        seed = Mathf.RoundToInt(seed);

        if (seed > 1000000)
        {
            seed /= 2;
        }
        else if (seed < -1000000)
        {
            seed += Mathf.Abs(seed) / 3;
        }
        if (seed > 1000000)
        {
            seed /= 3;
        }
        else if (seed < -1000000)
        {
            seed += Mathf.Abs(seed) / 2;
        }

        Random.InitState(seed);
        Debug.Log($"New random seed: {seed}");

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
                            cEvents[e].triggerMechanics[m].TriggerMechanic(new ActionController.ActionInfo(null, cEvents[e].character, null));
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

    public void WipeParty(PartyList party, string cause = "")
    {
        if (party == partyList)
        {
            if (!partyWiped)
            {
                partyWiped = true;
                StartCoroutine(ExecutePartyWipe(party, cause));
            }
        }
        if (party == enemyList)
        {
            if (!enemiesWiped)
            {
                enemiesWiped = true;
                StartCoroutine(ExecutePartyWipe(party, cause));
            }
        }
        else
        {
            StopCoroutine(ExecutePartyWipe(party, cause));
            StartCoroutine(ExecutePartyWipe(party, cause));
        }
    }

    // Execute the party wipe with a small delay between each member to simulate how FFXIV applies party wide damage
    private IEnumerator ExecutePartyWipe(PartyList party, string cause)
    {
        for (int i = 0; i < party.members.Count; i++)
        {
            party.members[i].ModifyHealth(new Damage(100, true, true, Damage.DamageType.unique, Damage.ElementalAspect.unaspected, Damage.PhysicalAspect.none, Damage.DamageApplicationType.percentageFromMax, cause), true);
            yield return new WaitForSeconds(0.066f);
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
