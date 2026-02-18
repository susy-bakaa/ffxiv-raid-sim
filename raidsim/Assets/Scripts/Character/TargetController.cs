// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;
using NaughtyAttributes;
using dev.susybaka.raidsim.Actions;
using dev.susybaka.raidsim.Characters;
using dev.susybaka.raidsim.Core;
using dev.susybaka.raidsim.UI;
using dev.susybaka.Shared;
using dev.susybaka.Shared.Attributes;
using dev.susybaka.Shared.Audio;
using dev.susybaka.Shared.UserInterface;

namespace dev.susybaka.raidsim.Targeting
{
    public class TargetController : MonoBehaviour
    {
        public enum TargetType { nearest, sideToSide, enmity, random }

        Camera m_camera;
        PauseMenu m_pauseMenu;
        ConfigMenu m_configMenu;

        CharacterState m_characterState;
        ActionController m_actionController;
        TargetController m_targetController;

        public bool log = false;
        public TargetNode self;
        public PartyList targetList;
        public TargetNode currentTarget;
        public TargetType targetType = TargetType.nearest;
        private TargetType wasTargetType;
        public List<TargetNode> availableTargets;
        public List<TargetNode> targetTriggerNodes;
        public List<int> allowedGroups;
        public float maxTargetDistance = 100f;
        public float mouseClickThreshold = 0.2f;
        [Obsolete("Has no functionality. This field is kept for compatibility reasons. It may be removed in a future version.")]
        [SerializeField, HideInInspector] public int autoTargetRate = 55;
        public LayerMask mask;
        public bool isPlayer;
        public bool isAi;
        public bool canMouseRaycast = true;
        public bool autoTarget;
        private bool wasAutoTarget;
        public bool onlyAliveTargets = false;
        private bool wasOnlyAliveTargets;
        public bool onlyIfSomeoneHasEnmity = false;
        private bool wasOnlyIfSomeoneHasEnmity;
        public bool enableAutoAttacksOnDoubleMouseRaycast = false;
        public bool ignoreTargetingRestrictions = false;
        private bool wasIgnoreTargetingRestrictions;

        [Header("Events")]
        public bool eventsEnabled = true;
        private bool wasEventsEnabled;
        public UnityEvent<TargetNode> onTarget;

        [Header("User Interface")]
        public float fadeDuration = 0.25f;
        public bool showTargetLevel = false;
        public bool showTargetLetter = true;
        public CanvasGroup targetInfo;
        public TextMeshProUGUI targetName;
        public Slider targetHealth;
        public TextMeshProUGUI targetHealthPercentage;
        public CanvasGroup targetCastbarGroup;
        public Slider targetCastbar;
        public TextMeshProUGUI targetCastbarProgress;
        public TextMeshProUGUI targetCastbarName;
        public HudElement targetCastbarHudElement;
        public CanvasGroup targetCastbarInterruptGroup;
        public CanvasGroup targetStatusEffectsGroup;
        public CanvasGroup targetsTargetGroup;
        public TextMeshProUGUI targetsTargetName;
        public Slider targetsTargetHealth;
        public List<TargetColor> targetColors;
        public List<HudElementColor> targetColoredHudElements;
        public List<HudElementColor> targetsTargetColoredHudElements;
        public Transform targetDamagePopupParent;
        public bool changeCursor = false;
        [CursorName] public string combatCursor = "combat";

        [Header("Audio")]
        public bool playAudio = false;
        [Range(0f, 1f)] public float audioVolume = 1f;
        [SerializeField][SoundName] private string onTargetAudio = "action_target";
        [SerializeField][SoundName] private string onUntargetAudio = "action_untarget";

        private float mouseDownTime;
        private bool targetColorsUpdated;
        private bool targetsTargetColorsUpdated;
        private bool wasMouseClick = false;
        private bool cursorWasSet = false;
#if UNITY_WEBPLAYER
        private float _targetInterval = 0.1f;
        private float _nextTargetTime;
#endif

#if UNITY_EDITOR
        [Space(20)]
#pragma warning disable CS0414 // The field 'TargetController.dummy' is assigned but its value is never used
        [SerializeField] private int _dummy = 0;
#pragma warning restore CS0414 // The field 'TargetController.dummy' is assigned but its value is never used
        [Button("Tab Target")]
        public void DebugTabTarget()
        {
            CycleTarget();
        }
#endif

        public void SetPauseMenu(PauseMenu pauseMenu)
        {
            m_pauseMenu = pauseMenu;
        }
        public void SetConfigMenu(ConfigMenu configMenu)
        {
            m_configMenu = configMenu;
        }

        private void Awake()
        {
            m_camera = Camera.main;

            if (targetCastbarGroup != null)
            {
                targetCastbarGroup.alpha = 0f;
                targetCastbarGroup.blocksRaycasts = false;
                targetCastbarGroup.interactable = false;
            }
            if (targetsTargetGroup != null)
            {
                targetsTargetGroup.alpha = 0f;
                targetsTargetGroup.blocksRaycasts = false;
                targetsTargetGroup.interactable = false;
            }

            if (self == null)
            {
                self = GetComponentInChildren<TargetNode>();
            }

            wasTargetType = targetType;
            wasAutoTarget = autoTarget;
            wasOnlyAliveTargets = onlyAliveTargets;
            wasOnlyIfSomeoneHasEnmity = onlyIfSomeoneHasEnmity;
            wasIgnoreTargetingRestrictions = ignoreTargetingRestrictions;
            wasEventsEnabled = eventsEnabled;

            targetTriggerNodes = new List<TargetNode>();
        }

        private void Update()
        {
            if (!ignoreTargetingRestrictions)
            {
                if (Utilities.RateLimiter(13))
                {
                    UpdateTarget();
                }
            }

            if (isPlayer && canMouseRaycast)
                HandleMouseClick();
            if (isPlayer)
                UpdateUserInterface();
#if !UNITY_WEBPLAYER
            if (autoTarget)
                Target();
#else
            if (!autoTarget)
                return;
            if (Time.unscaledTime < _nextTargetTime)
                return;

            _nextTargetTime = Time.unscaledTime + _targetInterval;
            Target();
#endif
        }

        private void OnEnable()
        {
            if (currentTarget != null)
            {
                if (eventsEnabled)
                    onTarget.Invoke(currentTarget);
            }
        }

        public void ResetController()
        {
            targetType = wasTargetType;
            autoTarget = wasAutoTarget;
            onlyAliveTargets = wasOnlyAliveTargets;
            onlyIfSomeoneHasEnmity = wasOnlyIfSomeoneHasEnmity;
            ignoreTargetingRestrictions = wasIgnoreTargetingRestrictions;
            eventsEnabled = wasEventsEnabled;
            currentTarget = null;
            availableTargets.Clear();
            targetTriggerNodes.Clear();
            SetTarget();
            UpdateUserInterface();
            UpdateTarget();
            if (self != null)
            {
                self.onDetarget.Invoke();
                self.UpdateUserInterface(0f, 0f);
                self.ResetHitboxRadius();
            }
            if (targetCastbarGroup != null)
            {
                targetCastbarGroup.alpha = 0f;
                targetCastbarGroup.blocksRaycasts = false;
                targetCastbarGroup.interactable = false;
            }
            if (targetCastbarName != null)
                targetCastbarName.text = "Unknown Cast";
            if (targetsTargetGroup != null)
            {
                targetsTargetGroup.alpha = 0f;
                targetsTargetGroup.blocksRaycasts = false;
                targetsTargetGroup.interactable = false;
            }
        }

        private void HandleMouseClick()
        {
            if (m_pauseMenu != null && m_pauseMenu.isOpen)
                return;
            if (m_configMenu != null && m_configMenu.isOpen)
                return;
            if (m_configMenu != null && m_configMenu.ApplyPopup.isOpen)
                return;

            Ray ray;
            RaycastHit hit;

            if (Input.GetMouseButtonDown(0))
            {
                mouseDownTime = Time.unscaledTime;
            }

            if (Input.GetMouseButtonUp(0))
            {
                float clickDuration = Time.unscaledTime - mouseDownTime;

                //Debug.Log($"clickDuration {clickDuration} clickDuration <= mouseClickThreshold {clickDuration <= mouseClickThreshold}");

                if (clickDuration <= mouseClickThreshold)
                {
                    ray = m_camera.ScreenPointToRay(Input.mousePosition);

                    //Debug.Log($"Input.mousePosition {Input.mousePosition}");

                    Debug.DrawLine(ray.origin, ray.direction * maxTargetDistance, Color.red);

                    if (Physics.Raycast(ray, out hit, maxTargetDistance))
                    {
                        if (hit.transform.childCount > 0 && hit.transform.GetChild(hit.transform.childCount - 1).TryGetComponent(out TargetNode targetNode) && targetNode.gameObject.CompareTag("target") && targetNode.Targetable && allowedGroups.Contains(targetNode.Group))
                        {
                            wasMouseClick = true;
                            SetTarget(targetNode);
                            if (changeCursor)
                            {
                                CursorHandler.Instance.SetCursorByID(0);
                                cursorWasSet = false;
                            }
                        }
                        else
                        {
                            wasMouseClick = true;
                            SetTarget(null);
                            if (changeCursor)
                            {
                                CursorHandler.Instance.SetCursorByID(0);
                                cursorWasSet = false;
                            }
                        }
                    }
                    else
                    {
                        SetTarget(null);
                        if (changeCursor)
                        {
                            CursorHandler.Instance.SetCursorByID(0);
                            cursorWasSet = false;
                        }
                    }
                }
                else
                {
                    if (changeCursor)
                    {
                        CursorHandler.Instance.SetCursorByID(0);
                        cursorWasSet = false;
                    }
                }
            }
            else if (changeCursor && Utilities.RateLimiter(30) && CursorHandler.Instance != null)
            {
                if (currentTarget != null)
                {
                    if (allowedGroups.Contains(currentTarget.Group) && currentTarget.Targetable)
                    {
                        if (cursorWasSet)
                        {
                            CursorHandler.Instance.SetCursorByID(0);
                            cursorWasSet = false;
                        }
                        return;
                    }
                }

                ray = m_camera.ScreenPointToRay(Input.mousePosition);

                //Debug.Log($"Input.mousePosition {Input.mousePosition}");

                Debug.DrawLine(ray.origin, ray.direction * maxTargetDistance, Color.yellow);

                if (Physics.Raycast(ray, out hit, maxTargetDistance))
                {
                    if (hit.transform.childCount > 0 && hit.transform.GetChild(hit.transform.childCount - 1).TryGetComponent(out TargetNode targetNode) && targetNode.gameObject.CompareTag("target") && targetNode.Targetable && allowedGroups.Contains(targetNode.Group))
                    {
                        CursorHandler.Instance.SetCursorByName(combatCursor);
                        cursorWasSet = true;
                    }
                    else
                    {
                        CursorHandler.Instance.SetCursorByID(0);
                        cursorWasSet = false;
                    }
                }
                else
                {
                    CursorHandler.Instance.SetCursorByID(0);
                    cursorWasSet = false;
                }
            }
        }

        public void UpdateTarget()
        {
            if (currentTarget != null)
            {
                if (currentTarget.TryGetCharacterState(out CharacterState state))
                {
                    if (state.untargetable.value || state.disabled)
                    {
                        SetTarget();
                    }
                    else if (self.TryGetCharacterState(out CharacterState characterState))
                    {
                        if (characterState.disabled)
                        {
                            SetTarget();
                        }
                    }
                }
            }
        }

        public void SetTarget()
        {
            SetTarget(null);
        }

        public void SetTargetToTargetsTarget()
        {
            if (currentTarget != null)
            {
                if (currentTarget.TryGetTargetController(out TargetController result))
                {
                    if (result.currentTarget != null)
                    {
                        SetTarget(result.currentTarget);
                    }
                }
            }
        }

        public void SetTarget(TargetNode target)
        {
            if (!ignoreTargetingRestrictions)
            {
                if (target != null && target.TryGetCharacterState(out CharacterState state))
                {
                    if (state.untargetable.value)
                    {
                        //Debug.Log($"Set target cancelled {state.characterName} is untargetable");
                        return;
                    }
                }
            }

            if (currentTarget != null && currentTarget != target && isPlayer)
                currentTarget.onDetarget.Invoke();

            if (currentTarget != null)
            {
                if (m_characterState != null)
                {
                    if (m_characterState.targetStatusEffectIconGroup != null)
                    {
                        if (m_characterState.targetStatusEffectIconGroup.alpha >= 1f)
                        {
                            m_characterState.targetStatusEffectIconGroup.blocksRaycasts = false;
                            m_characterState.targetStatusEffectIconGroup.interactable = false;
                            m_characterState.targetStatusEffectIconGroup.alpha = 0.99f;
                            if (!FightTimeline.Instance.paused)
                                m_characterState.targetStatusEffectIconGroup.LeanAlpha(0f, fadeDuration);
                            else
                                m_characterState.targetStatusEffectIconGroup.alpha = 0f;
                        }
                    }
                    if (m_characterState.showTargetDamagePopups && targetDamagePopupParent != null)
                        m_characterState.targetDamagePopupParent = null;
                }
                if (!FightTimeline.Instance.paused)
                    currentTarget.UpdateUserInterface(0f, fadeDuration);
                else
                    currentTarget.UpdateUserInterface(0f, 0f);
            }

            if (currentTarget == target && enableAutoAttacksOnDoubleMouseRaycast && wasMouseClick && self != null)
            {
                self.GetActionController().autoAttackEnabled = true;
            }

            bool isTargetChanged = currentTarget != target;

            currentTarget = target;
            if (eventsEnabled)
                onTarget.Invoke(currentTarget);

            if (log)
            {
                if (target != null)
                {
                    Debug.Log($"[{gameObject.name}] SetTarget to {target} {target.transform.parent.name}");
                }
                else
                {
                    Debug.Log($"[{gameObject.name}] SetTarget to null");
                }
            }

            targetColorsUpdated = false;
            targetsTargetColorsUpdated = false;
            m_characterState = null;
            m_actionController = null;
            m_targetController = null;
            wasMouseClick = false;

            // Additional logic for targeting (e.g., UI updates) can go here.
            if (currentTarget != null && isPlayer && eventsEnabled)
                currentTarget.onTarget.Invoke();

            if (playAudio && AudioManager.Instance != null && isTargetChanged)
            {
                if (currentTarget != null)
                {
                    AudioManager.Instance.Play(onTargetAudio, audioVolume);
                }
                else
                {
                    AudioManager.Instance.Play(onUntargetAudio, audioVolume);
                }
            }
        }

#if !UNITY_WEBPLAYER
        public void CycleTarget()
        {
            if (targetType == TargetType.enmity && isAi && targetList != null)
            {
                _ = CycleTargetAsync(true, true);
            }
            else if (targetType == TargetType.nearest)
            {
                _ = CycleTargetAsync(true, false);
            }
            else if (targetType == TargetType.random && isAi && targetList != null)
            {
                _ = CycleTargetAsync(false, false);
            }
        }
#else
        public void CycleTarget()
        {
            if (targetType == TargetType.enmity && isAi && targetList != null)
            {
                availableTargets = FindAllTargetableNodes(true, true);
            }
            else if (targetType == TargetType.nearest)
            {
                availableTargets = FindAllTargetableNodes(true, false);
            }
            else if (targetType == TargetType.random && isAi && targetList != null)
            {
                availableTargets = FindAllTargetableNodes(false, false);
            }

            if (availableTargets.Count == 0)
                return;

            if (currentTarget == null)
            {
                SetTarget(availableTargets[0]);
            }
            else
            {
                int currentIndex = availableTargets.IndexOf(currentTarget);
                int nextIndex = (currentIndex + 1) % availableTargets.Count;
                SetTarget(availableTargets[nextIndex]);
            }
        }
#endif

#if !UNITY_WEBPLAYER
        private async Task CycleTargetAsync(bool sortByDistance, bool sortByEnmity)
        {
            availableTargets = await FindAllTargetableNodesAsync(sortByDistance, sortByEnmity);
            if (availableTargets.Count == 0)
                return;

            if (currentTarget == null)
            {
                SetTarget(availableTargets[0]);
            }
            else
            {
                int currentIndex = availableTargets.IndexOf(currentTarget);
                int nextIndex = (currentIndex + 1) % availableTargets.Count;
                SetTarget(availableTargets[nextIndex]);
            }
        }
#endif

        public void SetRandomTargetFromList()
        {
            SetRandomTargetFromList($"{gameObject.name}_TargetController_SetRandomTargetFromList");
        }

        public void SetRandomTargetFromList(string key)
        {
            if (targetList == null)
                return;

            List<CharacterState> targets = targetList.GetActiveMembers();
            List<TargetNode> nodes = new List<TargetNode>();
            foreach (CharacterState target in targets)
            {
                if (target.TryGetComponent(out TargetController targetController))
                {
                    if (targetController.self != null)
                        nodes.Add(targetController.self);
                }
            }

            SetTarget(nodes.PickRandomItemPCG(FightTimeline.Instance.random, key));
        }

#if !UNITY_WEBPLAYER
        public void Target()
        {
            if (targetType == TargetType.enmity && isAi && targetList != null)
            {
                if (self == null)
                    Debug.LogError($"TargetController variable 'self' cannot be null when using TargetType.enmity!");

                if (onlyIfSomeoneHasEnmity && self != null && targetList.HasAnyEnmity(self.GetCharacterState()))
                {
                    _ = RefreshTargetsAsync(true, true);
                }
                else if (onlyIfSomeoneHasEnmity)
                {
                    availableTargets.Clear();
                }
                else
                {
                    _ = RefreshTargetsAsync(true, true);
                }
            }
            else if (targetType == TargetType.nearest)
            {
                _ = RefreshTargetsAsync(true, false);
            }
            else if (targetType == TargetType.random && isAi && targetList != null)
            {
                _ = RefreshTargetsAsync(false, false);
            }
        }
#else
        public void Target()
        {
            if (targetType == TargetType.enmity && isAi && targetList != null)
            {
                if (self == null)
                    Debug.LogError($"TargetController variable 'self' cannot be null when using TargetType.enmity!");

                if (onlyIfSomeoneHasEnmity && self != null && targetList.HasAnyEnmity(self.GetCharacterState()))
                {
                    availableTargets = FindAllTargetableNodes(true, true);
                }
                else if (onlyIfSomeoneHasEnmity)
                {
                    availableTargets.Clear();
                }
                else
                {
                    availableTargets = FindAllTargetableNodes(true, true);
                }
            }
            else if (targetType == TargetType.nearest)
            {
                availableTargets = FindAllTargetableNodes(true, false);
            }
            else if (targetType == TargetType.random && isAi && targetList != null)
            {
                availableTargets = FindAllTargetableNodes(false, false);
            }

            if (availableTargets.Count == 0)
            {
                SetTarget(null);
                return;
            }

            SetTarget(availableTargets[0]);
        }
#endif

#if !UNITY_WEBPLAYER
        private async Task RefreshTargetsAsync(bool sortByDistance, bool sortByEnmity)
        {
            availableTargets = await FindAllTargetableNodesAsync(sortByDistance, sortByEnmity);
            if (availableTargets.Count > 0)
                SetTarget(availableTargets[0]);
        }
#endif

#if !UNITY_WEBPLAYER
        private async Task<List<TargetNode>> FindAllTargetableNodesAsync(bool sortByDistance, bool sortByEnmity)
        {
            // Cache references on the main thread
            var selfPos = transform.position;
            var triggerNodesSet = new HashSet<TargetNode>(targetTriggerNodes ?? new List<TargetNode>());
            var allNodes = FindObjectsOfType<TargetNode>();
            var selfChar = self != null && self.TryGetCharacterState(out var state) ? state : null;
            var enmityList = sortByEnmity && targetList != null && selfChar != null
                ? targetList.GetEnmityList(selfChar)
                : null;

            List<TargetData> collectedData = new();
            foreach (var node in allNodes)
            {
                if (!node.gameObject.CompareTag("target"))
                    continue;
                if ((mask & (1 << node.gameObject.layer)) == 0)
                    continue;
                if (!node.Targetable)
                    continue;
                if (!allowedGroups.Contains(node.Group))
                    continue;
                if (onlyAliveTargets && node.TryGetCharacterState(out var cState) && cState.dead)
                    continue;
                if (targetType == TargetType.nearest && triggerNodesSet.Count > 0 && !triggerNodesSet.Contains(node))
                    continue;

                var pos = node.transform.position;
                var dist = Vector3.Distance(selfPos, pos);
                if (dist > maxTargetDistance)
                    continue;

                Vector3 vp = m_camera.WorldToViewportPoint(pos);
                if (vp.z < 0 || vp.x < 0 || vp.x > 1 || vp.y < 0 || vp.y > 1)
                    continue;

                float enmity = 0;
                if (sortByEnmity && enmityList != null && node.TryGetCharacterState(out var targetChar))
                {
                    int index = enmityList.IndexOf(targetChar);
                    enmity = index >= 0 ? enmityList.Count - index : 0;
                }

                collectedData.Add(new TargetData
                {
                    node = node,
                    position = pos,
                    distance = dist,
                    enmityScore = enmity
                });
            }

            return await Task.Run(() =>
            {
                if (sortByEnmity)
                {
                    return collectedData
                        .OrderByDescending(t => t.enmityScore)
                        .ThenBy(t => t.distance)
                        .Select(t => t.node)
                        .ToList();
                }

                if (sortByDistance)
                {
                    return collectedData
                        .OrderBy(t => t.distance)
                        .Select(t => t.node)
                        .ToList();
                }

                // Fallback: shuffle
                return collectedData
                    .OrderBy(_ => UnityEngine.Random.value)
                    .Select(t => t.node)
                    .ToList();
            });
        }
#else
        List<TargetNode> FindAllTargetableNodes(bool sortByDistance, bool sortByEnmity)
        {
            TargetNode[] allNodes = FindObjectsOfType<TargetNode>();
            List<TargetNode> targetableNodes = new List<TargetNode>();

            foreach (var node in allNodes)
            {
                // Retrieve CharacterState and check if the target is alive or dead based on the onlyAliveTargets setting
                if (node.TryGetCharacterState(out CharacterState nodeCharacterState))
                {
                    if (onlyAliveTargets && nodeCharacterState.dead)
                    {
                        continue; // Skip this node as it is dead and we only want alive targets
                    }
                }
                // If targeting is set to nearest and we have nodes in the trigger attached to this controllers own target node,
                // check if the current target node is inside that trigger or skip it
                if (targetType == TargetType.nearest && targetTriggerNodes != null && targetTriggerNodes.Count > 0)
                {
                    if (!targetTriggerNodes.Contains(node))
                    {
                        continue;
                    }
                }

                if (node.gameObject.CompareTag("target") && (mask & (1 << node.gameObject.layer)) != 0 && node.Targetable && allowedGroups.Contains(node.Group))
                {
                    float distanceToNode = Vector3.Distance(transform.position, node.transform.position);
                    if (distanceToNode <= maxTargetDistance)
                    {
                        Vector3 viewportPoint = m_camera.WorldToViewportPoint(node.transform.position);
                        if (viewportPoint.z > 0 && viewportPoint.x > 0 && viewportPoint.x < 1 && viewportPoint.y > 0 && viewportPoint.y < 1)
                        {
                            targetableNodes.Add(node);
                        }
                    }
                }
            }

            // We need to sort it automatically like this here already or the targeting is delayed and slow as fuck
            // This is still a mess honestly
            // Sort targets based on distance
            targetableNodes.Sort((a, b) => Vector3.Distance(transform.position, a.transform.position)
                                            .CompareTo(Vector3.Distance(transform.position, b.transform.position)));

            if (sortByEnmity && targetList != null && self != null && self.TryGetCharacterState(out CharacterState selfCharacterState))
            {
                List<CharacterState> targets = targetList.GetEnmityList(selfCharacterState);

                if (targets != null && targets.Count > 0)
                {
                    targetableNodes.Sort((a, b) =>
                    {
                        bool aHasState = a.TryGetCharacterState(out CharacterState stateA);
                        bool bHasState = b.TryGetCharacterState(out CharacterState stateB);

                        if (aHasState && bHasState)
                        {
                            int indexA = targets.IndexOf(stateA);
                            int indexB = targets.IndexOf(stateB);

                            // Ensure valid indices, if state is not found, it should be lowest priority
                            if (indexA == -1)
                                indexA = int.MaxValue;
                            if (indexB == -1)
                                indexB = int.MaxValue;

                            return indexA.CompareTo(indexB);
                        }
                        else if (aHasState)
                        {
                            // a has state, b does not, so a should come before b
                            return -1;
                        }
                        else if (bHasState)
                        {
                            // b has state, a does not, so b should come before a
                            return 1;
                        }
                        else
                        {
                            // Neither a nor b has state, consider them equal
                            return 0;
                        }
                    });
                }
            }
            else if (sortByDistance)
            {
                // Filter available targets based on if we only want ones from a specified target list.
                if (isAi && targetList != null)
                {
                    for (int i = 0; i < targetableNodes.Count; i++)
                    {
                        if (targetableNodes[i].TryGetCharacterState(out CharacterState result))
                        {
                            if (!targetList.HasCharacterState(result))
                            {
                                targetableNodes.RemoveAt(i);
                                i--;
                            }
                        }

                        //Debug.Log($"node {targetableNodes[i].gameObject.name}");
                    }
                }

                // Sort targets based on distance
                targetableNodes.Sort((a, b) => Vector3.Distance(transform.position, a.transform.position)
                                                .CompareTo(Vector3.Distance(transform.position, b.transform.position)));
            }
            else
            {
                // Filter available targets based on if we only want ones from a specified target list.
                if (isAi && targetList != null)
                {
                    for (int i = 0; i < targetableNodes.Count; i++)
                    {
                        if (targetableNodes[i].TryGetCharacterState(out CharacterState result))
                        {
                            if (!targetList.HasCharacterState(result))
                            {
                                targetableNodes.RemoveAt(i);
                                i--;
                            }
                        }

                        //Debug.Log($"node {targetableNodes[i].gameObject.name}");
                    }
                }

                // Sort targets randomly
                targetableNodes.Shuffle();
            }

            return targetableNodes;
        }
#endif

        public void SetAutoTargeting(bool state)
        {
            autoTarget = state;
        }

        public void SetOnlyAliveTargeting(bool state)
        {
            onlyAliveTargets = state;
        }

        public void SetEventStatus(bool state)
        {
            eventsEnabled = state;
        }

        private void UpdateUserInterface()
        {
            if (currentTarget != null && targetInfo != null)
            {
                if (!targetColorsUpdated)
                {
                    TargetColor targetColor = targetColors[0];

                    for (int i = 0; i < targetColors.Count; i++)
                    {
                        if (currentTarget.IsNodeInGroups(targetColors[i].groups.ToArray()))
                        {
                            targetColor = targetColors[i];
                            break;
                        }
                    }

                    for (int i = 0; i < targetColoredHudElements.Count; i++)
                    {
                        targetColoredHudElements[i].SetColor(targetColor.colors);
                    }

                    if (!FightTimeline.Instance.paused)
                        currentTarget.UpdateUserInterface(1f, fadeDuration);
                    else
                        currentTarget.UpdateUserInterface(1f, 0f);

                    targetColorsUpdated = true;
                }

                if (targetInfo != null && targetInfo.alpha <= 0f)
                {
                    targetInfo.blocksRaycasts = true;
                    targetInfo.interactable = true;
                    targetInfo.alpha = 0.01f;
                    if (!FightTimeline.Instance.paused)
                        targetInfo.LeanAlpha(1f, fadeDuration);
                    else
                        targetInfo.alpha = 1f;
                }

                // CHARACTER STATE
                if (m_characterState != null)
                {
                    if (targetName != null)
                    {
                        string letter = "";

                        if (showTargetLetter && m_characterState.characterLetter >= 0 && m_characterState.characterLetter <= 25)
                        {
                            letter = $"<sprite=\"{m_characterState.letterSpriteAsset}\" name=\"{m_characterState.characterLetter}\" tint=\"FF7E95\">";
                        }

                        if (showTargetLevel)
                        {
                            targetName.text = $"Lv{m_characterState.characterLevel} {letter}{m_characterState.GetCharacterName()}";
                        }
                        else
                        {
                            targetName.text = $"{letter}{m_characterState.GetCharacterName()}";
                        }
                    }
                    targetHealth.maxValue = m_characterState.currentMaxHealth;
                    targetHealth.minValue = 0;
                    targetHealth.value = m_characterState.health;

                    /*if (m_characterState.healthBarTextInPercentage)
                    {*/
                    float healthPercentage = ((float)m_characterState.health / (float)m_characterState.currentMaxHealth) * 100f;
                    // Set the health bar text with proper formatting
                    if (Mathf.Approximately(healthPercentage, 100f))  // Use Mathf.Approximately for floating point comparison
                    {
                        targetHealthPercentage.text = "100%";
                    }
                    else
                    {
                        string result = healthPercentage.ToString("F1") + "%";

                        if (m_characterState.health > 0)
                        {
                            if (result == "0%" || result == "0.0%" || result == "0.00%" || result == "00.0%" || result == "00.00%" || result == "0,0%" || result == "0,00%" || result == "00,0%" || result == "00,00%")
                            {
                                result = "0.1%";
                            }
                            if (result == "100.0%" || result == "100,0%")
                            {
                                result = "99.9%";
                            }
                        }

                        result = result.Replace(',', '.').Replace(':', '.').Replace(';', '.');

                        targetHealthPercentage.text = result;
                    }

                    if (m_characterState.targetStatusEffectIconGroup != null)
                    {
                        if (m_characterState.targetStatusEffectIconGroup.alpha <= 0f)
                        {
                            m_characterState.targetStatusEffectIconGroup.blocksRaycasts = true;
                            m_characterState.targetStatusEffectIconGroup.interactable = true;
                            m_characterState.targetStatusEffectIconGroup.alpha = 0.01f;
                            if (!FightTimeline.Instance.paused)
                                m_characterState.targetStatusEffectIconGroup.LeanAlpha(1f, fadeDuration);
                            else
                                m_characterState.targetStatusEffectIconGroup.alpha = 1f;

                        }
                    }

                    if (m_characterState.showTargetDamagePopups && targetDamagePopupParent != null)
                    {
                        m_characterState.targetDamagePopupParent = targetDamagePopupParent;
                    }
                }
                else
                {
                    if (currentTarget.TryGetCharacterState(out CharacterState state))
                    {
                        m_characterState = state;
                    }
                    else
                    {
                        targetName.text = "???";
                        targetHealth.maxValue = 1;
                        targetHealth.minValue = 0;
                        targetHealth.value = 1;
                        targetHealthPercentage.text = "???%";
                    }
                }
                // ACTION CONTROLLER
                if (m_actionController != null)
                {
                    if (m_actionController.CastTime > 0f && !m_actionController.Interrupted)
                    {
                        if (targetCastbar != null)
                        {
                            targetCastbar.minValue = 0f;
                            if (m_actionController.LastAction != null)
                                targetCastbar.maxValue = m_actionController.LastAction.data.cast;
                            else
                                targetCastbar.maxValue = 4.7f;
                            targetCastbar.value = m_actionController.LastCastTime - m_actionController.CastTime;

                            if (targetCastbarGroup.alpha == 0f)
                            {
                                targetCastbarGroup.blocksRaycasts = true;
                                targetCastbarGroup.interactable = true;
                                targetCastbarGroup.alpha = 0.01f;
                                if (!FightTimeline.Instance.paused)
                                    targetCastbarGroup.LeanAlpha(1f, 0.1f);
                                else
                                    targetCastbarGroup.alpha = 1f;
                            }
                            if (targetCastbarInterruptGroup != null)
                            {
                                targetCastbarInterruptGroup.alpha = 0f;
                            }
                            if (targetCastbarHudElement != null)
                            {
                                targetCastbarHudElement.ChangeColors(false);
                            }
                        }
                        if (targetCastbarProgress != null)
                        {
                            targetCastbarProgress.text = m_actionController.CastTime.ToString("00.00").Replace(',', '.').Replace(':', '.').Replace(';', '.');
                        }
                        if (targetCastbarName != null)
                        {
                            targetCastbarName.text = m_actionController.LastAction.data.GetActionName();
                        }
                    }
                    else
                    {
                        if (m_actionController.Interrupted)
                        {
                            if (targetCastbarHudElement != null)
                            {
                                targetCastbarHudElement.ChangeColors(true);
                            }
                            if (targetCastbar != null && targetCastbarGroup.alpha == 1f)
                            {
                                targetCastbarGroup.alpha = 0.99f;
                                Utilities.FunctionTimer.Create(this, () => { targetCastbarGroup.blocksRaycasts = false; targetCastbarGroup.interactable = false; if (!FightTimeline.Instance.paused) { targetCastbarGroup.LeanAlpha(0f, fadeDuration); } else { targetCastbarGroup.alpha = 0f; } }, 2f, $"{m_actionController}_castBar_fade_out_if_interrupted", true);
                            }
                            Utilities.FunctionTimer.Create(this, () =>
                            {
                                if (targetCastbar != null)
                                {
                                    targetCastbar.value = 0f;
                                }
                                if (targetCastbarInterruptGroup != null)
                                {
                                    targetCastbarInterruptGroup.alpha = 0f;
                                }
                                if (targetCastbarProgress != null)
                                {
                                    targetCastbarProgress.text = "00.00";
                                }
                                if (targetCastbarName != null)
                                {
                                    targetCastbarName.text = "Unknown Cast";
                                }
                            }, 2.5f, $"{m_actionController}_interrupted_status", true);
                        }
                        else
                        {
                            if (targetCastbar != null && targetCastbarGroup.alpha == 1f)
                            {
                                targetCastbarGroup.blocksRaycasts = false;
                                targetCastbarGroup.interactable = false;
                                targetCastbarGroup.alpha = 0.99f;
                                if (!FightTimeline.Instance.paused)
                                    targetCastbarGroup.LeanAlpha(0f, fadeDuration);
                                else
                                    targetCastbarGroup.alpha = 0f;
                            }
                        }
                    }
                }
                else
                {
                    if (currentTarget.TryGetActionController(out ActionController aController))
                    {
                        m_actionController = aController;
                    }
                    else if (targetCastbarGroup.alpha >= 1f)
                    {
                        targetCastbarGroup.blocksRaycasts = false;
                        targetCastbarGroup.interactable = false;
                        if (!FightTimeline.Instance.paused)
                            targetCastbarGroup.LeanAlpha(0f, fadeDuration);
                        else
                            targetCastbarGroup.alpha = 0f;
                    }
                }
                // TARGET CONTROLLER
                if (m_targetController != null)
                {
                    if (m_targetController.currentTarget != null)
                    {
                        if (!targetsTargetColorsUpdated)
                        {
                            TargetColor targetsTargetColor = targetColors[0];

                            for (int i = 0; i < targetColors.Count; i++)
                            {
                                if (m_targetController.currentTarget.IsNodeInGroups(targetColors[i].groups.ToArray()))
                                {
                                    targetsTargetColor = targetColors[i];
                                    break;
                                }
                            }

                            for (int i = 0; i < targetsTargetColoredHudElements.Count; i++)
                            {
                                targetsTargetColoredHudElements[i].SetColor(targetsTargetColor.colors);
                            }

                            targetsTargetColorsUpdated = true;
                        }

                        if (m_targetController.currentTarget.TryGetCharacterState(out CharacterState ttState))
                        {
                            if (targetsTargetGroup.alpha <= 0f)
                            {
                                targetsTargetGroup.blocksRaycasts = true;
                                targetsTargetGroup.interactable = true;
                                targetsTargetGroup.alpha = 0.01f;
                                if (!FightTimeline.Instance.paused)
                                    targetsTargetGroup.LeanAlpha(1f, fadeDuration);
                                else
                                    targetsTargetGroup.alpha = 1f;
                            }
                            if (targetsTargetHealth != null)
                            {
                                targetsTargetHealth.maxValue = ttState.currentMaxHealth;
                                targetsTargetHealth.minValue = 0;
                                targetsTargetHealth.value = ttState.health;
                            }
                            if (targetsTargetName != null)
                            {
                                string letter = "";

                                if (showTargetLetter && ttState.characterLetter >= 0 && ttState.characterLetter <= 25)
                                {
                                    letter = $"<sprite=\"{ttState.letterSpriteAsset}\" name=\"{ttState.characterLetter}\" tint=\"FF7E95\">";
                                }

                                if (showTargetLevel)
                                {
                                    targetsTargetName.text = $"Lv{ttState.characterLevel} {ttState.GetCharacterName()}{letter}";
                                }
                                else
                                {
                                    targetsTargetName.text = $"{ttState.GetCharacterName()}{letter}";
                                }
                            }
                        }
                        else if (targetsTargetGroup.alpha >= 1f)
                        {
                            targetsTargetGroup.blocksRaycasts = false;
                            targetsTargetGroup.interactable = false;
                            targetsTargetGroup.alpha = 0.99f;
                            if (!FightTimeline.Instance.paused)
                                targetsTargetGroup.LeanAlpha(0f, fadeDuration);
                            else
                                targetsTargetGroup.alpha = 0f;

                            if (targetsTargetHealth != null)
                            {
                                targetsTargetHealth.maxValue = 1;
                                targetsTargetHealth.minValue = 0;
                                targetsTargetHealth.value = 1;
                            }
                            if (targetsTargetName != null)
                            {
                                targetsTargetName.text = "Unknown";
                            }
                        }
                    }
                    else if (targetsTargetGroup.alpha >= 1f)
                    {
                        targetsTargetGroup.blocksRaycasts = false;
                        targetsTargetGroup.interactable = false;
                        targetsTargetGroup.alpha = 0.99f;
                        if (!FightTimeline.Instance.paused)
                            targetsTargetGroup.LeanAlpha(0f, fadeDuration);
                        else
                            targetsTargetGroup.alpha = 0f;

                        if (targetsTargetHealth != null)
                        {
                            targetsTargetHealth.maxValue = 1;
                            targetsTargetHealth.minValue = 0;
                            targetsTargetHealth.value = 1;
                        }
                        if (targetsTargetName != null)
                        {
                            targetsTargetName.text = "Unknown";
                        }
                    }
                }
                else
                {
                    if (currentTarget.TryGetTargetController(out TargetController tController))
                    {
                        m_targetController = tController;
                    }
                    else if (targetsTargetGroup.alpha >= 1f)
                    {
                        targetsTargetGroup.blocksRaycasts = false;
                        targetsTargetGroup.interactable = false;
                        targetsTargetGroup.alpha = 0.99f;
                        if (!FightTimeline.Instance.paused)
                            targetsTargetGroup.LeanAlpha(0f, fadeDuration);
                        else
                            targetsTargetGroup.alpha = 0f;

                        if (targetsTargetHealth != null)
                        {
                            targetsTargetHealth.maxValue = 1;
                            targetsTargetHealth.minValue = 0;
                            targetsTargetHealth.value = 1;
                        }
                        if (targetsTargetName != null)
                        {
                            targetsTargetName.text = "Unknown";
                        }
                    }
                }
            }
            else if (targetInfo != null && targetInfo.alpha >= 1f)
            {
                targetInfo.blocksRaycasts = false;
                targetInfo.interactable = false;
                targetInfo.alpha = 0.99f;
                if (!FightTimeline.Instance.paused)
                    targetInfo.LeanAlpha(0f, fadeDuration);
                else
                    targetInfo.alpha = 0f;
            }
        }

        [System.Serializable]
        public struct TargetColor
        {
            public List<int> groups;
            public List<Color> colors;
        }

        private struct TargetData
        {
            public TargetNode node;
            public Vector3 position;
            public float distance;
            public float enmityScore;
        }
    }
}