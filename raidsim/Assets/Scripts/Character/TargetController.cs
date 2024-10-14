using System.Collections.Generic;
using NaughtyAttributes;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;

public class TargetController : MonoBehaviour
{
    public enum TargetType { nearest, sideToSide, enmity }

    Camera m_camera;
    PauseMenu m_pauseMenu;
    ConfigMenu m_configMenu;

    CharacterState m_characterState;
    ActionController m_actionController;
    TargetController m_targetController;

    public TargetNode self;
    public PartyList targetList;
    public TargetNode currentTarget;
    public TargetType targetType = TargetType.nearest;
    //public TargetNode selfTarget;
    public List<TargetNode> availableTargets;
    public List<TargetNode> targetTriggerNodes;
    public List<int> allowedGroups;
    //public KeyCode targetKey = KeyCode.Tab;
    //public KeyCode cancelKey = KeyCode.Escape;
    //public KeyCode selfTargetKey = KeyCode.F1;
    public float maxTargetDistance = 100f;
    public float mouseClickThreshold = 0.2f;
    public int autoTargetRate = 55;
    public LayerMask mask;
    public bool isPlayer;
    public bool isAi;
    public bool canMouseRaycast = true;
    public bool autoTarget;
    public bool onlyAliveTargets = false;
    public bool onlyIfSomeoneHasEnmity = false;
    public bool enableAutoAttacksOnDoubleMouseRaycast = false;

    [Header("Events")]
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

    private float mouseDownTime;
    private bool targetColorsUpdated;
    private bool targetsTargetColorsUpdated;
    private int rateLimit = 55;
    private bool wasMouseClick = false;

#if UNITY_EDITOR
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

    void Awake()
    {
        m_camera = Camera.main;

        if (targetCastbarGroup != null)
            targetCastbarGroup.alpha = 0f;
        if (targetsTargetGroup != null)
            targetsTargetGroup.alpha = 0f;

        if (self == null)
        {
            self = GetComponentInChildren<TargetNode>();
        }

        rateLimit = Random.Range(autoTargetRate - 9, autoTargetRate + 1);

        targetTriggerNodes = new List<TargetNode>();
    }

    void Update()
    {
        if (isPlayer && canMouseRaycast)
            HandleMouseClick();
        //HandleTabTargeting();
        if (isPlayer)
            UpdateUserInterface();
        if (autoTarget && Utilities.RateLimiter(rateLimit))
            Target();
    }

    void OnEnable()
    {
        if (currentTarget != null)
        {
            onTarget.Invoke(currentTarget);
        }
        else
        {
            onTarget.Invoke(null);
        }
    }

    void HandleMouseClick()
    {
        if (m_pauseMenu != null && m_pauseMenu.isOpen)
            return;
        if (m_configMenu != null && m_configMenu.isOpen)
            return;
        if (m_configMenu != null && m_configMenu.isApplyPopupOpen)
            return;

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
                Ray ray = m_camera.ScreenPointToRay(Input.mousePosition);

                //Debug.Log($"Input.mousePosition {Input.mousePosition}");

                Debug.DrawLine(ray.origin, ray.direction * maxTargetDistance, Color.red);

                if (Physics.Raycast(ray, out RaycastHit hit, maxTargetDistance))
                {
                    if (hit.transform.childCount > 0 && hit.transform.GetChild(hit.transform.childCount - 1).TryGetComponent(out TargetNode targetNode) && targetNode.gameObject.CompareTag("target") && targetNode.Targetable && allowedGroups.Contains(targetNode.Group))
                    {
                        wasMouseClick = true;
                        SetTarget(targetNode);
                    }
                    else
                    {
                        wasMouseClick = true;
                        SetTarget(null);
                    }
                }
            }
        }
    }

    /*void HandleTabTargeting()
    {
        if (Input.GetKeyDown(cancelKey))
        {
            SetTarget(null);
        }
        else if (Input.GetKeyDown(targetKey))
        {
            CycleTarget();
        } 
        else if (Input.GetKeyDown(selfTargetKey))
        {
            SetTarget(selfTarget);
        }
    }*/

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
        if (target != null && target.TryGetCharacterState(out CharacterState state))
        {
            if (state.untargetable)
            {
                //Debug.Log($"Set target cancelled {state.characterName} is untargetable");
                return;
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
                        m_characterState.targetStatusEffectIconGroup.alpha = 0.99f;
                        if (!FightTimeline.Instance.paused)
                            m_characterState.targetStatusEffectIconGroup.LeanAlpha(0f, fadeDuration);
                        else
                            m_characterState.targetStatusEffectIconGroup.alpha = 0f;
                    }
                }
            }
            currentTarget.UpdateUserInterface(0f, fadeDuration);
        }

        if (currentTarget == target && enableAutoAttacksOnDoubleMouseRaycast && wasMouseClick && self != null)
        {
            self.GetActionController().autoAttackEnabled = true;
        }

        currentTarget = target;
        onTarget.Invoke(currentTarget);

        /*if (target != null)
        {
            Debug.Log($"SetTarget to {target} {target.transform.parent.name}");
        }
        else
        {
            Debug.Log($"SetTarget to null");
        }*/

        targetColorsUpdated = false;
        targetsTargetColorsUpdated = false;
        m_characterState = null;
        m_actionController = null;
        m_targetController = null;
        wasMouseClick = false;

        // Additional logic for targeting (e.g., UI updates) can go here.
        if (currentTarget != null && isPlayer)
            currentTarget.onTarget.Invoke();
    }

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

        if (availableTargets.Count == 0)
            return;

        SetTarget(availableTargets[0]);
    }

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

                    Debug.Log($"node {targetableNodes[i].gameObject.name}");
                }
            }

            // Sort targets based on distance
            targetableNodes.Sort((a, b) => Vector3.Distance(transform.position, a.transform.position)
                                            .CompareTo(Vector3.Distance(transform.position, b.transform.position)));
        }

        return targetableNodes;
    }

    public void SetAutoTargeting(bool state)
    {
        autoTarget = state;
    }

    void UpdateUserInterface()
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

                currentTarget.UpdateUserInterface(1f, fadeDuration);

                targetColorsUpdated = true;
            }

            if (targetInfo != null && targetInfo.alpha <= 0f)
            {
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
                        targetName.text = $"Lv{m_characterState.characterLevel} {letter}{m_characterState.characterName}";
                    }
                    else
                    {
                        targetName.text = $"{letter}{m_characterState.characterName}";
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
                        m_characterState.targetStatusEffectIconGroup.alpha = 0.01f;
                        if (!FightTimeline.Instance.paused)
                            m_characterState.targetStatusEffectIconGroup.LeanAlpha(1f, fadeDuration);
                        else
                            m_characterState.targetStatusEffectIconGroup.alpha = 1f;

                    }
                }
                /*}
                else
                {
                    targetHealthPercentage.text = m_characterState.health.ToString();
                }*/
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
                        targetCastbarName.text = Utilities.InsertSpaceBeforeCapitals(m_actionController.LastAction.data.actionName);
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
                            Utilities.FunctionTimer.Create(this, () => { if (!FightTimeline.Instance.paused) { targetCastbarGroup.LeanAlpha(0f, fadeDuration); } else { targetCastbarGroup.alpha = 0f; } }, 2f, $"{m_actionController}_castBar_fade_out_if_interrupted", true);
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
                                targetsTargetName.text = $"Lv{ttState.characterLevel} {ttState.characterName}{letter}";
                            }
                            else
                            {
                                targetsTargetName.text = $"{ttState.characterName}{letter}";
                            }
                        }
                    }
                    else if (targetsTargetGroup.alpha >= 1f)
                    {
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
}