// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using NaughtyAttributes;
using dev.susybaka.raidsim.Actions;
using dev.susybaka.raidsim.Bots;
using dev.susybaka.raidsim.Characters;
using dev.susybaka.raidsim.Inputs;
using dev.susybaka.raidsim.Mechanics;
using dev.susybaka.raidsim.Nodes;
using dev.susybaka.raidsim.StatusEffects;
using dev.susybaka.raidsim.Targeting;
using dev.susybaka.raidsim.UI;
using dev.susybaka.Shared;
using dev.susybaka.Shared.Attributes;
using dev.susybaka.Shared.Audio;
using static dev.susybaka.raidsim.Core.GlobalData;
using TMPro;

namespace dev.susybaka.raidsim.Core
{
    public class FightTimeline : MonoBehaviour
    {
        public static FightTimeline Instance;

        public UserInput input;
        public FightSelector fightSelector;
        public CharacterState player;
        public PartyList partyList;
        public PartyList enemyList;
        public bool partyWiped { private set; get; }
        public bool enemiesWiped { private set; get; }
        public Transform mechanicParent;
        public Transform enemiesParent;
        public Transform charactersParent;
        public Transform botNodeParent;

        public static float timeScale = 1f;
        public static float time { private set; get; }
        public static float deltaTime { private set; get; }
        public Vector3 arenaOffset = Vector3.zero;
        public Vector3 arenaBounds = Vector3.zero;
        public bool isCircle;
        private Vector3 originalArenaBounds = Vector3.zero;
        private bool originalIsCircle;

        [Header("Current")]
        public string timelineName = "Unnamed fight timeline";
        public string timelineAbbreviation = string.Empty;
        public bool playing = false;
        public bool paused = false;
        public List<string> pausedBy = new List<string>();
        public UnityEvent<bool> onPausedChanged;
        public bool disableBotTimelines = false;
        public Transform botTimelineParent;
        public List<BotTimeline> botTimelines = new List<BotTimeline>();
        public List<TimelineEvent> events = new List<TimelineEvent>();
        public List<RandomEventResult> m_randomEventResults = new List<RandomEventResult>();
        public List<string> executedMechanics = new List<string>();
        public bool jon = false;
        public bool log = false;
        public bool clearRandomEventResultsOnStart = true;
        public bool noNewSeedOnStart = false;
        public bool disableKnockbacks = false;
        public UnityEvent<bool> onNoNewSeedOnStartChanged;
        public UnityEvent onReset;
        public UnityEvent onPlay;

        [Header("User Interface")]
        public Button[] disableDuringPlayback;
        public bool useAutomarker = false;
        public UnityEvent<bool> onUseAutomarkerChanged;
        public int botNameType = 0;
        public bool colorBotNamesByRole = false;

        [Header("Audio")]
        [SerializeField][SoundName] private string reloadSound = "ui_close";
        [SerializeField][SoundName] private string pauseSound = "<None>";
        [SerializeField][SoundName] private string playSound = "<None>";
        [SerializeField][Range(0f, 1f)] private float audioVolume = 1f;

        public string ReloadSound { get { return reloadSound; } }
        public string PauseSound { get { return pauseSound; } }
        public string PlaySound { get { return playSound; } }
        public float AudioVolume { get { return audioVolume; } }

        // PRIVATE

        private Dictionary<int, List<CharacterActionData>> randomEventCharacterActions = new Dictionary<int, List<CharacterActionData>>();
        private Dictionary<int, int> randomEventResults = new Dictionary<int, int>();
        private BotTimeline[] allBotTimelines;
        private SetDynamicBotNode[] allSetDynamicBotNodes;
        private BotTimelineBranch[] allBotTimelineBranches;
        private BotNode[] allBotNodes;
        private MechanicNode[] allMechanicNodes;
        private List<CharacterState> allCharacters = new List<CharacterState>();
        private bool wasPaused;
        private Coroutine iePlayTimeline;
        private Coroutine ieResetTimeline;
        private bool hasBeenPlayed = false;
        private bool resetCalled = false;
        private float gameSpeed = 1f;
        private TextMeshProUGUI simulationSpeedLabel;

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

#if UNITY_EDITOR
        [Header("Editor")]
        [SerializeField] private float m_wipeDelay;

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
            //LoadEffectsButton();
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
            if (string.IsNullOrEmpty(reloadSound))
                reloadSound = "<None>";
            if (string.IsNullOrEmpty(pauseSound))
                pauseSound = "<None>";
            if (string.IsNullOrEmpty(playSound))
                playSound = "<None>";

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

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(Instance.gameObject);
            }
            Instance = this;

            if (mechanicParent == null)
            {
                mechanicParent = Utilities.FindAnyByName("Mechanics").transform;
            }

            if (enemiesParent == null)
            {
                enemiesParent = Utilities.FindAnyByName("Enemies").transform;
            }

            if (charactersParent == null)
            {
                charactersParent = Utilities.FindAnyByName("Characters").transform;
            }

            if (charactersParent != null)
            {
                allCharacters.Clear();
                allCharacters.AddRange(charactersParent.GetComponentsInChildren<CharacterState>());
                allCharacters.AddRange(enemiesParent.GetComponentsInChildren<CharacterState>());
            }

            if (botNodeParent == null)
            {
                botNodeParent = Utilities.FindAnyByName("AINodes").transform;
            }

            if (botNodeParent != null)
            {
                allBotNodes = botNodeParent.GetComponentsInChildren<BotNode>();
                allMechanicNodes = botNodeParent.GetComponentsInChildren<MechanicNode>();
            }

            if (!disableBotTimelines)
            {
                if (botTimelineParent == null)
                {
                    botTimelineParent = Utilities.FindAnyByName("BotTimelines").transform;
                }

                if (botTimelineParent != null)
                {
                    allBotTimelines = botTimelineParent.GetComponentsInChildren<BotTimeline>();
                    allSetDynamicBotNodes = botTimelineParent.GetComponentsInChildren<SetDynamicBotNode>();
                    allBotTimelineBranches = botTimelineParent.GetComponentsInChildren<BotTimelineBranch>();
                }
            }

            input = GetComponentInChildren<UserInput>();
            fightSelector = FindFirstObjectByType<FightSelector>();

            for (int i = 0; i < events.Count; i++)
            {
                TimelineEvent e = events[i];
                e.SaveRandomEventPools();
                events[i] = e;
            }

            originalArenaBounds = arenaBounds;
            originalIsCircle = isCircle;
            hasBeenPlayed = false;

            onNoNewSeedOnStartChanged.Invoke(noNewSeedOnStart);

            // Cannot use FindAnyByName here because for some reason it keeps finding the prefab and not the scene instance, even though it can only see scene objects. This is likely a Unity editor quirk.
            simulationSpeedLabel = GameObject.Find("SimulationSpeedLabel").GetComponentInChildren<TextMeshProUGUI>(true);
        }

        private void Update()
        {
            if (paused)
            {
                Time.timeScale = 0f;
            }
            else
            {
                Time.timeScale = gameSpeed;

                if (input != null)
                {
                    float s = input.GetAxisDown("Speed");

                    if (s > 0f)
                    {
                        if (gameSpeed >= 4f)
                        {
                            gameSpeed += 1f;
                        }
                        else if (gameSpeed < 4f && gameSpeed >= 2f)
                        {
                            gameSpeed += 0.5f;
                        }
                        else if (gameSpeed < 2f)
                        {
                            gameSpeed += 0.25f;
                        }
                    }
                    else if (s < 0f)
                    {
                        if (gameSpeed > 4f)
                        {
                            gameSpeed -= 1f;
                        }
                        else if (gameSpeed <= 4f && gameSpeed > 2f)
                        {
                            gameSpeed -= 0.5f;
                        }
                        else if (gameSpeed <= 2f)
                        {
                            gameSpeed -= 0.25f;
                        }
                    }

                    if (gameSpeed < 0f)
                        gameSpeed = 0f;
                    if (gameSpeed > 10f)
                        gameSpeed = 10f;

                    simulationSpeedLabel.text = gameSpeed.ToString("F2").Replace(',','.');
                }
            }

            if (timeScale < 0f)
                timeScale = 0f;

            deltaTime = Time.deltaTime * timeScale;
            time = Time.time * timeScale;
        }

        private void OnDestroy()
        {
            pausedBy.Clear();
            paused = false;
            Time.timeScale = 1f;
        }

        public void StartTimeline()
        {
            if (hasBeenPlayed)
                return;

            hasBeenPlayed = true;

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

            if (!noNewSeedOnStart)
            {
                Random.InitState(seed);
                Debug.Log($"New random seed: {seed}");
            }

            if (clearRandomEventResultsOnStart)
            {
                randomEventResults.Clear();
                m_randomEventResults.Clear();
            }

            for (int i = 0; i < disableDuringPlayback.Length; i++)
            {
                if (disableDuringPlayback[i] == null)
                    continue;

                disableDuringPlayback[i].interactable = false;
            }

            onPlay.Invoke();

            playing = true;
            if (iePlayTimeline == null)
                iePlayTimeline = StartCoroutine(IE_PlayTimeline());

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.Play(playSound, audioVolume);
            }

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

        private IEnumerator IE_PlayTimeline()
        {
            if (hasBeenPlayed)
            {
                yield return null;
                StopCoroutine(IE_PlayTimeline());
            }

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
                                    cEvents[e].triggerMechanics[m].TriggerMechanic(new ActionInfo(null, cEvents[e].character, null));
                                else
                                    cEvents[e].triggerMechanics[m].TriggerMechanic(new ActionInfo(null, null, null));
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
                                            if (!cEvents[e].performHiddenAction)
                                                cEvents[e].actions.PerformAction(cEvents[e].performCharacterActions[a]);
                                            else
                                                cEvents[e].actions.PerformActionHidden(cEvents[e].performCharacterActions[a]);
                                        }
                                    }
                                    else
                                    {
                                        int r = UnityEngine.Random.Range(0, cEvents[e].performCharacterActions.Length);
                                        bool exist = false;

                                        if (TryGetRandomEventResult(cEvents[e].id, out int rr))
                                        {
                                            r = rr;
                                            exist = true;
                                            Debug.Log($"Exists! r {r}");
                                        }

                                        if (!cEvents[e].performHiddenAction)
                                            cEvents[e].actions.PerformAction(cEvents[e].performCharacterActions[r]);
                                        else
                                            cEvents[e].actions.PerformActionHidden(cEvents[e].performCharacterActions[r]);

                                        if (!exist)
                                        {
                                            randomEventResults.Add(cEvents[e].id, r);
                                            m_randomEventResults.Add(new RandomEventResult(cEvents[e].id, r));
                                        }
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
                if (disableDuringPlayback[i] == null)
                    continue;

                disableDuringPlayback[i].interactable = true;
            }
            iePlayTimeline = null;
        }

        public void ResetTimeline(float delay)
        {
            if (resetCalled)
                return;

            if (iePlayTimeline != null)
            {
                StopAllCoroutines();
                iePlayTimeline = null;
            }

            if (delay > 0f)
            {
                if (ieResetTimeline == null)
                    ieResetTimeline = StartCoroutine(IE_ResetTimeline(new WaitForSeconds(delay)));
            }
            else
            {
                ResetTimelineInternal();
            }
        }

        private IEnumerator IE_ResetTimeline(WaitForSeconds wait)
        {
            yield return wait;
            ResetTimelineInternal();
            ieResetTimeline = null;
        }

        private void ResetTimelineInternal()
        {
            hasBeenPlayed = false;
            playing = false;
            partyWiped = false;
            enemiesWiped = false;
            executedMechanics.Clear();
            for (int i = 0; i < events.Count; i++)
            {
                TimelineEvent e = events[i];
                e.ResetRandomEventPools();
                events[i] = e;
            }
            if (allBotTimelines != null && allBotTimelines.Length > 0 && !disableBotTimelines)
            {
                for (int i = 0; i < allBotTimelines.Length; i++)
                {
                    if (i < 0 || i >= allBotTimelines.Length)
                        continue;

                    allBotTimelines[i].ResetTimeline();
                }
            }
            if (allSetDynamicBotNodes != null && allSetDynamicBotNodes.Length > 0)
            {
                for (int i = 0; i < allSetDynamicBotNodes.Length; i++)
                {
                    if (i < 0 || i >= allSetDynamicBotNodes.Length)
                        continue;

                    allSetDynamicBotNodes[i].ResetComponent();
                }
            }
            if (allBotTimelineBranches != null && allBotTimelineBranches.Length > 0)
            {
                for (int i = 0; i < allBotTimelineBranches.Length; i++)
                {
                    if (i < 0 || i >= allBotTimelineBranches.Length)
                        continue;

                    allBotTimelineBranches[i].ResetComponent();
                }
            }
            if (allCharacters != null && allCharacters.Count > 0)
            {
                for (int i = 0; i < allCharacters.Count; i++)
                {
                    if (i < 0 || i >= allCharacters.Count)
                        continue;

                    allCharacters[i].gameObject.SetActive(true);
                    allCharacters[i].ResetState();
                }
            }
            if (allBotNodes != null && allBotNodes.Length > 0)
            {
                for (int i = 0; i < allBotNodes.Length; i++)
                {
                    if (i < 0 || i >= allBotNodes.Length)
                        continue;

                    allBotNodes[i].occupied = false;
                    allBotNodes[i].hasMechanic = false;
                }
            }
            if (allMechanicNodes != null && allMechanicNodes.Length > 0)
            {
                for (int i = 0; i < allMechanicNodes.Length; i++)
                {
                    if (i < 0 || i >= allMechanicNodes.Length)
                        continue;

                    allMechanicNodes[i].isTaken = false;
                }
            }
            pausedBy.Clear();
            paused = false;
            gameSpeed = 1f;
            Time.timeScale = gameSpeed;
            if (mechanicParent != null)
            {
                foreach (Transform child in mechanicParent)
                {
                    if (!child.CompareTag("presetMech"))
                        Destroy(child.gameObject);
                }
            }
            for (int i = 0; i < disableDuringPlayback.Length; i++)
            {
                if (disableDuringPlayback[i] == null)
                    continue;

                disableDuringPlayback[i].interactable = true;
            }
            if (partyList != null)
                partyList.UpdatePartyList();
            if (enemyList != null)
                enemyList.UpdatePartyList();
            onReset.Invoke();
            resetCalled = false;
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.Play(reloadSound, audioVolume);
            }
        }

        public void WipeParty(PartyList party, string cause = "")
        {
            if (party == partyList)
            {
                if (!partyWiped)
                {
                    partyWiped = true;
                    StartCoroutine(IE_PartyWipe(party, cause));
                }
            }
            if (party == enemyList)
            {
                if (!enemiesWiped)
                {
                    enemiesWiped = true;
                    StartCoroutine(IE_PartyWipe(party, cause));
                }
            }
            else
            {
                StopCoroutine(IE_PartyWipe(party, cause));
                StartCoroutine(IE_PartyWipe(party, cause));
            }
        }

        // Execute the party wipe with a small delay between each member to simulate how FFXIV applies party wide damage
        private IEnumerator IE_PartyWipe(PartyList party, string cause)
        {
            for (int i = 0; i < party.members.Count; i++)
            {
                party.members[i].characterState.ModifyHealth(new Damage(100, true, true, Damage.DamageType.unique, Damage.ElementalAspect.unaspected, Damage.PhysicalAspect.none, Damage.DamageApplicationType.percentageFromMax, cause), true);
                yield return new WaitForSeconds(0.066f);
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

        public bool TryGetRandomEventResult(int id, out int value)
        {
            if (randomEventResults.TryGetValue(id, out int v))
            {
                value = v;
                return true;
            }
            value = -1;
            return false;
        }

        public void SetRandomEventResult(int id, int result)
        {
            if (randomEventResults.ContainsKey(id))
            {
                randomEventResults[id] = result;
                for (int i = 0; i < m_randomEventResults.Count; i++)
                {
                    if (m_randomEventResults[i].id == id)
                    {
                        RandomEventResult rer = m_randomEventResults[i];
                        rer.value = result;
                        m_randomEventResults[i] = rer;
                        break;
                    }
                }
            }
            else
            {
                if (randomEventResults.Count < 1)
                {
                    randomEventResults = new Dictionary<int, int>();
                }
                randomEventResults.Add(id, result);
                m_randomEventResults.Add(new RandomEventResult(id, result));
            }
        }

        public void AddRandomEventResult(int id, int result)
        {
            int newId = id;
            while (randomEventResults.ContainsKey(newId))
            {
                newId++;
            }
            randomEventResults.Add(newId, result);
            m_randomEventResults.Add(new RandomEventResult(newId, result));
        }

        public void ClearRandomEventResult(int id)
        {
            if (randomEventResults.ContainsKey(id))
            {
                randomEventResults.Remove(id);
                for (int i = 0; i < m_randomEventResults.Count; i++)
                {
                    if (m_randomEventResults[i].id == id)
                    {
                        m_randomEventResults.RemoveAt(i);
                        break;
                    }
                }
            }
        }

        public void ToggleJonVoiceLines(bool value)
        {
            jon = value;
        }

        public void ToggleNoNewRandomSeed()
        {
            ToggleNoNewRandomSeed(!noNewSeedOnStart);
        }

        public void ToggleNoNewRandomSeed(bool value)
        {
            noNewSeedOnStart = value;
            onNoNewSeedOnStartChanged.Invoke(value);
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

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.Play(pauseSound, audioVolume);
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
            private List<RandomEventPool> originalRandomEventPools;
            public List<TimelineCharacterEvent> characterEvents;

            public TimelineEvent(string name, float time, List<RandomEventPool> randomSets, List<TimelineCharacterEvent> characterEvents)
            {
                this.name = name;
                this.time = time;
                this.randomEventPools = randomSets;
                this.originalRandomEventPools = randomSets;
                this.characterEvents = characterEvents;
            }

            public void SaveRandomEventPools()
            {
                if (randomEventPools == null || randomEventPools.Count < 1)
                    return;

                originalRandomEventPools = new List<RandomEventPool>();

                if (Instance != null && Instance.log)
                    Debug.Log($"Saving random event pools for {name} with {randomEventPools.Count} entries");

                for (int i = 0; i < randomEventPools.Count; i++)
                {
                    originalRandomEventPools.Add(new RandomEventPool(randomEventPools[i]));
                }
            }

            public void ResetRandomEventPools()
            {
                if (originalRandomEventPools == null || originalRandomEventPools.Count < 1)
                {
                    if (Instance != null && Instance.log)
                        Debug.Log($"No original random event pools found for {name}");
                    return;
                }

                randomEventPools = new List<RandomEventPool>();

                if (Instance != null && Instance.log)
                    Debug.Log($"Resetting random event pools for {name} with {originalRandomEventPools.Count} entries");

                for (int i = 0; i < originalRandomEventPools.Count; i++)
                {
                    randomEventPools.Add(new RandomEventPool(originalRandomEventPools[i]));
                }
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

            public RandomEventPool(RandomEventPool copyFrom)
            {
                this.id = copyFrom.id;
                this.CharacterActionPool = new List<CharacterActionData>();
                for (int i = 0; i < copyFrom.CharacterActionPool.Count; i++)
                {
                    this.CharacterActionPool.Add(copyFrom.CharacterActionPool[i]);
                }
                this.name = copyFrom.name;
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
            public bool performHiddenAction;
            public int randomEventPoolId;
            public FightMechanic[] triggerMechanics;
            public UnityEvent onEvent;
            public RandomCheck[] randomChecks;
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

}