using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;

public class TargetController : MonoBehaviour
{
    Camera m_camera;

    CharacterState m_characterState;
    ActionController m_actionController;
    TargetController m_targetController;

    public TargetNode currentTarget;
    public TargetNode selfTarget;
    public List<TargetNode> availableTargets;
    public List<int> allowedGroups;
    public KeyCode targetKey = KeyCode.Tab;
    public KeyCode cancelKey = KeyCode.Escape;
    public KeyCode selfTargetKey = KeyCode.F1;
    public float maxTargetDistance = 100f;
    public LayerMask mask;
    public bool isPlayer;
    [Header("User Interface")]
    public CanvasGroup targetInfo;
    public TextMeshProUGUI targetName;
    public Slider targetHealth;
    public TextMeshProUGUI targetHealthPercentage;
    public CanvasGroup targetCastbarGroup;
    public Slider targetCastbar;
    public TextMeshProUGUI targetCastbarProgress;
    public TextMeshProUGUI targetCastbarName;
    public CanvasGroup targetCastbarInterruptGroup;
    public CanvasGroup targetStatusEffectsGroup;
    public CanvasGroup targetsTargetGroup;
    public TextMeshProUGUI targetsTargetName;
    public Slider targetsTargetHealth;
    public TargetColor defaultTargetColor;
    public List<TargetColor> targetColors;

    void Awake()
    {
        m_camera = Camera.main;

        if (selfTarget == null)
        {
            selfTarget = GetComponentInChildren<TargetNode>();
        }
    }

    void Update()
    {
        HandleMouseClick();
        HandleTabTargeting();
        UpdateUserInterface();
    }

    void HandleMouseClick()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                TargetNode targetNode = hit.transform.GetComponent<TargetNode>();
                if (targetNode != null && targetNode.gameObject.CompareTag("target") && targetNode.Targetable && allowedGroups.Contains(targetNode.Group))
                {
                    SetTarget(targetNode);
                }
                else
                {
                    SetTarget(null);
                }
            }
        }
    }

    void HandleTabTargeting()
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
    }

    public void SetTarget(TargetNode target)
    {
        if (currentTarget == null && isPlayer)
            currentTarget.onDetarget.Invoke();

        currentTarget = target;

        // Additional logic for targeting (e.g., UI updates) can go here.
        if (currentTarget != null && isPlayer)
            currentTarget.onTarget.Invoke();
    }

    void CycleTarget()
    {
        availableTargets = FindAllTargetableNodes();

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

    List<TargetNode> FindAllTargetableNodes()
    {
        TargetNode[] allNodes = FindObjectsOfType<TargetNode>();
        List<TargetNode> targetableNodes = new List<TargetNode>();

        foreach (var node in allNodes)
        {
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

        // Sort targets based on distance
        targetableNodes.Sort((a, b) => Vector3.Distance(transform.position, a.transform.position)
                                        .CompareTo(Vector3.Distance(transform.position, b.transform.position)));

        return targetableNodes;
    }

    void UpdateUserInterface()
    {
        if (currentTarget != null)
        {
            if (targetInfo.alpha <= 0f)
            {
                targetInfo.LeanAlpha(1f, 0.5f);
            }

            // CHARACTER STATE
            if (m_characterState != null)
            {
                targetName.text = m_characterState.characterName;
                targetHealth.maxValue = m_characterState.currentMaxHealth;
                targetHealth.minValue = 0;
                targetHealth.value = m_characterState.health;

                if (m_characterState.healthBarTextInPercentage)
                {
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
                        }

                        targetHealthPercentage.text = result;
                    }
                }
                else
                {
                    targetHealthPercentage.text = m_characterState.health.ToString();
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
                    targetName.text = "??? Unknown Target";
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
                        targetCastbar.value = m_actionController.LastCastTime - m_actionController.CastTime;

                        if (targetCastbarGroup.alpha == 0f)
                        {
                            targetCastbarGroup.LeanAlpha(1f, 0.1f);
                        }
                        if (targetCastbarInterruptGroup != null)
                        {
                            targetCastbarInterruptGroup.alpha = 0f;
                        }
                    }
                    if (targetCastbarProgress != null)
                    {
                        targetCastbarProgress.text = m_actionController.CastTime.ToString("00.00");
                    }
                }
                else
                {
                    if (m_actionController.Interrupted)
                    {
                        if (targetCastbar != null && targetCastbarGroup.alpha == 1f)
                        {
                            targetCastbarGroup.alpha = 0.99f;
                            Utilities.FunctionTimer.Create(() => targetCastbarGroup.LeanAlpha(0f, 0.5f), 2f, $"{m_actionController}_castBar_fade_out_if_interrupted", true);
                        }
                        Utilities.FunctionTimer.Create(() => 
                        {
                            if (targetCastbar != null)
                            {
                                targetCastbar.value = 0f;
                            }
                            if (targetCastbarInterruptGroup != null)
                            {
                                targetCastbarInterruptGroup.alpha = 0f;
                            }
                        }, 2.5f, $"{m_actionController}_interrupted_status", true);
                    }
                    else
                    {
                        if (targetCastbar != null && targetCastbarGroup.alpha == 1f)
                        {
                            targetCastbarGroup.alpha = 0.99f;
                            targetCastbarGroup.LeanAlpha(0f, 0.5f);
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
                    targetCastbarGroup.LeanAlpha(0f, 0.5f);
                }
            }
            // TARGET CONTROLLER
            if (m_targetController != null)
            {
                if (m_targetController.currentTarget != null)
                {
                    if (m_targetController.currentTarget.TryGetCharacterState(out CharacterState ttState))
                    {
                        if (targetsTargetGroup.alpha <= 0f)
                        {
                            targetsTargetGroup.alpha = 0.01f;
                            targetsTargetGroup.LeanAlpha(1f, 0.5f);
                        }
                        if (targetsTargetHealth != null)
                        {
                            targetsTargetHealth.maxValue = ttState.currentMaxHealth;
                            targetsTargetHealth.minValue = 0;
                            targetsTargetHealth.value = ttState.health;
                        }
                        if (targetsTargetName != null)
                        {
                            targetsTargetName.text = ttState.characterName;
                        }
                    }
                    else if (targetsTargetGroup.alpha >= 1f)
                    {
                        targetsTargetGroup.alpha = 0.99f;
                        targetsTargetGroup.LeanAlpha(0f, 0.5f);

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
                    targetsTargetGroup.LeanAlpha(0f, 0.5f);

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
                    targetsTargetGroup.LeanAlpha(0f, 0.5f);

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
        else if (targetInfo.alpha >= 1f)
        {
            targetInfo.alpha = 0.99f;
            targetInfo.LeanAlpha(0f, 0.5f);
        }
    }

    public struct TargetColor
    {
        public List<int> groups;
        public Color main;
        public Color highlight;
        public Color Background;
    }
}