using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Events;
using static GlobalData;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


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
    public Vector3 arenaBounds = Vector3.zero;
    public bool isCircle;
    private Vector3 originalArenaBounds = Vector3.zero;
    private bool originalIsCircle;

    [Header("Current")]
    public string timelineName = "Unnamed fight timeline";
    public bool playing = false;
    public bool paused = false;
    public List<string> pausedBy = new List<string>();
    public UnityEvent<bool> onPausedChanged;
    public bool disableBotTimelines = false;
    public List<BotTimeline> botTimelines = new List<BotTimeline>();
    public List<TimelineEvent> events = new List<TimelineEvent>();
    public List<RandomEventResult> m_randomEventResults = new List<RandomEventResult>();
    public bool log = false;

    [Header("User Interface")]
    public Button[] disableDuringPlayback;
    public bool useAutomarker = false;
    public UnityEvent<bool> onUseAutomarkerChanged;

    // PRIVATE

    private Dictionary<int, List<CharacterActionData>> randomEventCharacterActions = new Dictionary<int, List<CharacterActionData>>();
    private Dictionary<int, int> randomEventResults = new Dictionary<int, int>();
    private bool wasPaused;

    [System.Serializable]
    public struct RandomEventResult
    {
        public int id; 
        public int value;

        public RandomEventResult(int id, int value)
        {
            this.id = id;
            this.value = value;
        }
    }

    public int GetRandomEventResult(int id)
    {
        if (randomEventResults.TryGetValue(id, out int value))
        {
            return value;
        }

        return -1;
    }

    public void AddRandomEventResult(int id, int result)
    {
        int newId = id;
        while (randomEventResults.ContainsKey(newId))
        {
            newId++;
        }
        //Debug.Log($"Added RandomEventResult with id of {newId}");
        randomEventResults.Add(newId, result);
        m_randomEventResults.Add(new RandomEventResult(newId, result));
    }

#if UNITY_EDITOR
    [Header("Editor")]
    [SerializeField] private float m_wipeDelay;
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
        if (m_wipeDelay > 0f)
        {
            Utilities.FunctionTimer.Create(this, () => WipeParty(partyList), m_wipeDelay, "fightTimeline_wipe_delay", false, true);
        }
        else
        {
            WipeParty(partyList);
        }
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
                if (pl.members.ContainsCharacterState(player))
                {
                    partyList = pl;
                    break;
                }
            }
        }
    }

    private void OnValidate()
    {
        for (int i = 0; i < events.Count; i++)
        {
            for (int k = 0; k < events[i].randomEventPools.Count; k++)
            {
                RandomEventPool temp = events[i].randomEventPools[k];

                temp.id = k;
                if (events[i].randomEventPools[k].id < 0)
                {
                    temp.id = 0;
                }
                temp.name = $"{temp.id}_{events[i].randomEventPools[k].CharacterActionPool.Count}";
                
                events[i].randomEventPools[k] = temp;
            }
            for (int j = 0; j < events[i].characterEvents.Count; j++)
            {
                TimelineCharacterEvent temp = events[i].characterEvents[j];

                if (events[i].characterEvents[j].randomEventPoolId < -1 || events[i].randomEventPools.Count <= 0)
                {
                    temp.randomEventPoolId = -1;
                }
                else if (events[i].characterEvents[j].randomEventPoolId > events[i].randomEventPools.Count - 1)
                {
                    temp.randomEventPoolId = events[i].randomEventPools.Count - 1;
                }

                events[i].characterEvents[j] = temp;
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

        originalArenaBounds = arenaBounds;
        originalIsCircle = isCircle;
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

    void OnDestroy()
    {
        pausedBy.Clear();
        paused = false;
        Time.timeScale = 1f;
        //Utilities.FunctionTimer.CleanUp(SceneManager.GetActiveScene(), new Scene());
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

        randomEventResults.Clear();
        m_randomEventResults.Clear();

        for (int i = 0; i < disableDuringPlayback.Length; i++)
        {
            disableDuringPlayback[i].interactable = false;
        }

        playing = true;
        StartCoroutine(PlayTimeline());
        
        if (disableBotTimelines)
            return;

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
                    Debug.Log(cEvents[e].name);
                    // Check for CharacterEvent fight mechanics performed for this event
                    if (cEvents[e].triggerMechanics != null && cEvents[e].triggerMechanics.Length > 0)
                    {
                        for (int m = 0; m < cEvents[e].triggerMechanics.Length; m++)
                        {
                            if (cEvents[e].character != null)
                                cEvents[e].triggerMechanics[m].TriggerMechanic(new ActionController.ActionInfo(null, cEvents[e].character, null));
                            else
                                cEvents[e].triggerMechanics[m].TriggerMechanic(new ActionController.ActionInfo(null, null, null));
                        }
                    }
                    // Check for CharacterEvent targets to be set for this events character
                    if (cEvents[e].targets != null)
                    {
                        if (!cEvents[e].pickRandomTarget)
                            cEvents[e].targets.SetTarget(cEvents[e].target);
                        else
                            cEvents[e].targets.SetRandomTargetFromList();
                        if (cEvents[e].faceTarget && cEvents[e].bossController != null && cEvents[e].targets.currentTarget != null)
                        {
                            Vector3 direction = cEvents[e].targets.currentTarget.transform.position - cEvents[e].targets.transform.position;
                            Quaternion rotation = Quaternion.LookRotation(direction);
                            cEvents[e].targets.transform.rotation = rotation;
                            cEvents[e].bossController.SetRotationTarget(cEvents[e].targets.currentTarget);

                            if (cEvents[e].randomChecks != null && cEvents[e].randomChecks.Length > 0)
                            {
                                for (int r = 0; r < cEvents[e].randomChecks.Length; r++)
                                {
                                    if (!cEvents[e].randomChecks[r].matchFromTarget)
                                        continue;

                                    bool eventMatched = false;

                                    if (cEvents[e].randomChecks[r].matchedEffect != null)
                                    {
                                        if (cEvents[e].targets.currentTarget.TryGetCharacterState(out CharacterState characterState))
                                        {
                                            if (characterState.HasAnyVersionOfEffect(cEvents[e].randomChecks[r].matchedEffect.statusName))
                                            {
                                                eventMatched = true;
                                                AddRandomEventResult(cEvents[e].id + cEvents[e].randomChecks[r].id, cEvents[e].randomChecks[r].positiveResult);
                                            }
                                        }
                                    }

                                    if (!eventMatched)
                                    {
                                        AddRandomEventResult(cEvents[e].id + cEvents[e].randomChecks[r].id, cEvents[e].randomChecks[r].negativeResult);
                                    }
                                }
                            }
                        }
                    }
                    // Check for CharacterEvent actions to be performed on this events character
                    if (cEvents[e].actions != null)
                    {
                        if (cEvents[e].performCharacterActions != null && cEvents[e].performCharacterActions.Length > 0)
                        {
                            if (cEvents[e].randomEventPoolId > -1)
                            {
                                RandomEventPool pool = new RandomEventPool(-1, null);

                                for (int r = 0; r < events[i].randomEventPools.Count; r++)
                                {
                                    if (events[i].randomEventPools[r].id == cEvents[e].randomEventPoolId)
                                    {
                                        pool = events[i].randomEventPools[r];
                                    }
                                }

                                if (pool.id > -1)
                                {
                                    List<CharacterActionData> availableActions = pool.CharacterActionPool;

                                    if (randomEventCharacterActions.ContainsKey(pool.id))
                                    {
                                        availableActions = randomEventCharacterActions[pool.id];
                                    }
                                    else
                                    {
                                        randomEventCharacterActions.Add(pool.id, pool.CharacterActionPool);
                                    }

                                    CharacterActionData temp = availableActions.GetRandomItem();

                                    for (int a = 0; a < cEvents[e].performCharacterActions.Length; a++)
                                    {
                                        if (cEvents[e].performCharacterActions[a].data == temp)
                                        {
                                            cEvents[e].actions.PerformAction(cEvents[e].performCharacterActions[a]);
                                            randomEventResults.Add(cEvents[e].id, a);
                                            m_randomEventResults.Add(new RandomEventResult(cEvents[e].id, a));
                                            availableActions.Remove(temp);
                                        }
                                    }

                                    if (availableActions.Count <= 0)
                                    {
                                        randomEventCharacterActions.Remove(pool.id);
                                    }
                                }
                            }
                            else
                            {
                                if (!cEvents[e].pickRandomCharacterAction)
                                {
                                    for (int a = 0; a < cEvents[e].performCharacterActions.Length; a++)
                                    {
                                        cEvents[e].actions.PerformAction(cEvents[e].performCharacterActions[a]);
                                    }
                                }
                                else
                                {
                                    int r = UnityEngine.Random.Range(0, cEvents[e].performCharacterActions.Length);
                                    cEvents[e].actions.PerformAction(cEvents[e].performCharacterActions[r]);
                                    randomEventResults.Add(cEvents[e].id, r);
                                    m_randomEventResults.Add(new RandomEventResult(cEvents[e].id, r));
                                }
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
                                cEvents[e].character.AddEffect(cEvents[e].giveBuffs[b], cEvents[e].character);
                            }
                        }
                        if (cEvents[e].giveDebuffs != null && cEvents[e].giveDebuffs.Length > 0)
                        {
                            for (int d = 0; d < cEvents[e].giveDebuffs.Length; d++)
                            {
                                cEvents[e].character.AddEffect(cEvents[e].giveDebuffs[d], cEvents[e].character);
                            }
                        }
                    }
                    // Check for CharacterEvent movement to be performed for this events character
                    if (cEvents[e].controller != null)
                    {
                        if (cEvents[e].moveToTarget && cEvents[e].teleport && cEvents[e].target != null)
                        {
                            cEvents[e].controller.transform.position = cEvents[e].target.transform.position;
                            cEvents[e].controller.transform.rotation = cEvents[e].target.transform.rotation;
                        } 
                        else if (cEvents[e].moveToTarget && cEvents[e].botTimeline != null)
                        {
                            cEvents[e].controller.botTimeline = cEvents[e].botTimeline;
                            cEvents[e].botTimeline.StartTimeline();
                        }
                    }
                    // Check for CharacterEvent movement to be performed for this events boss
                    if (cEvents[e].bossController != null)
                    {
                        if (cEvents[e].moveToTarget && !cEvents[e].teleport)
                        {
                            cEvents[e].bossController.SetTarget(cEvents[e].target);
                        }
                        else if (cEvents[e].moveToTarget && cEvents[e].teleport && cEvents[e].target != null)
                        {
                            cEvents[e].bossController.transform.position = cEvents[e].target.transform.position;
                            cEvents[e].bossController.transform.rotation = cEvents[e].target.transform.rotation;
                        }
                    }
                    cEvents[e].onEvent.Invoke();
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

        for (int i = 0; i < disableDuringPlayback.Length; i++)
        {
            disableDuringPlayback[i].interactable = true;
        }
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
            party.members[i].characterState.ModifyHealth(new Damage(100, true, true, Damage.DamageType.unique, Damage.ElementalAspect.unaspected, Damage.PhysicalAspect.none, Damage.DamageApplicationType.percentageFromMax, cause), true);
            yield return new WaitForSeconds(0.066f);
        }
    }

    public void ToggleAutomarker()
    {
        useAutomarker = !useAutomarker;
        onUseAutomarkerChanged.Invoke(useAutomarker);
    }

    public void TogglePause(string label)
    {
        if (pausedBy.Contains(label))
        {
            pausedBy.Remove(label);
        }
        else
        {
            pausedBy.Add(label);
        }

        UpdatePause();
    }

    public void TogglePause(bool state, string label)
    {
        if (state && !pausedBy.Contains(label))
        {
            pausedBy.Add(label);
        }
        else if (!state && pausedBy.Contains(label))
        {
            pausedBy.Remove(label);
        }

        UpdatePause();
    }

    public void ResetPauseState()
    {
        pausedBy.Clear();
        UpdatePause();
    }

    private void UpdatePause()
    {
        if (pausedBy.Count > 0)
        {
            paused = true;
        }
        else
        {
            paused = false;
        }

        if (paused != wasPaused)
        {
            wasPaused = paused;
            onPausedChanged.Invoke(paused);
        }
    }

    public void ChangeArena(Vector3 bounds, bool? isCircle = null)
    {
        if (bounds != Vector3.zero)
        {
            arenaBounds = bounds;
        }
        if (isCircle != null)
        {
            this.isCircle = (bool)isCircle;
        }
    }

    public void ResetArena() 
    { 
        arenaBounds = originalArenaBounds; 
        isCircle = originalIsCircle; 
    }

    [System.Serializable]
    public struct TimelineEvent
    {
        public string name;
        public float time;
        public List<RandomEventPool> randomEventPools;
        public List<TimelineCharacterEvent> characterEvents;

        public TimelineEvent(string name, float time, List<RandomEventPool> randomSets, List<TimelineCharacterEvent> characterEvents)
        {
            this.name = name;
            this.time = time;
            this.randomEventPools = randomSets;
            this.characterEvents = characterEvents;
        }
    }

    [System.Serializable]
    public struct RandomEventPool
    {
        public string name;
        public int id;
        public List<CharacterActionData> CharacterActionPool;

        public RandomEventPool(int id, List<CharacterActionData> characterActionPool)
        {
            this.id = id;
            this.CharacterActionPool = characterActionPool;
            if (characterActionPool != null)
                this.name = $"{id}_{characterActionPool.Count}";
            else
                this.name = id.ToString();
        }
    }

    [System.Serializable]
    public struct TimelineCharacterEvent
    {
        public string name;
        public int id;
        public CharacterState character;
        public ActionController actions;
        public TargetController targets;
        public AIController controller;
        public BossController bossController;
        public StatusEffectData[] giveBuffs;
        public StatusEffectData[] giveDebuffs;
        public CharacterAction[] performCharacterActions;
        public TargetNode target;
        public BotTimeline botTimeline;
        public bool moveToTarget;
        public bool faceTarget;
        public bool teleport;
        public bool pickRandomCharacterAction;
        public bool pickRandomTarget;
        public int randomEventPoolId;
        public FightMechanic[] triggerMechanics;
        public UnityEvent onEvent;
        public RandomCheck[] randomChecks;

        /*public TimelineCharacterEvent(string name, int id, CharacterState character, ActionController actions, TargetController targets, AIController controller, BossController bossController, StatusEffectData[] giveBuffs, StatusEffectData[] giveDebuffs, CharacterAction[] performCharacterActions, TargetNode target, BotTimeline botTimeline, bool moveToTarget, bool faceTarget, bool teleport, bool pickRandomCharacterAction, bool pickRandomTarget, FightMechanic[] triggerMechanics, UnityEvent onEvent, int eventRandomSetPoolId = -1)
        {
            this.name = name;
            this.id = id;
            this.character = character;
            this.actions = actions;
            this.targets = targets;
            this.controller = controller;
            this.bossController = bossController;
            this.giveBuffs = giveBuffs;
            this.giveDebuffs = giveDebuffs;
            this.performCharacterActions = performCharacterActions;
            this.target = target;
            this.botTimeline = botTimeline;
            this.moveToTarget = moveToTarget;
            this.faceTarget = faceTarget;
            this.teleport = teleport;
            this.pickRandomCharacterAction = pickRandomCharacterAction;
            this.pickRandomTarget = pickRandomTarget;
            this.randomEventPoolId = eventRandomSetPoolId;
            this.triggerMechanics = triggerMechanics;
            this.onEvent = onEvent;
        }*/
    }

    [System.Serializable]
    public struct RandomCheck
    {
        public string name;
        public StatusEffectData matchedEffect;
        [MinValue(0)] public int id;
        [MinValue(0)] public int positiveResult;
        [MinValue(0)] public int negativeResult;
        public bool matchFromTarget;
    }
}
