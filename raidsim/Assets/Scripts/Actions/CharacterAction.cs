using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static ActionController;
using static CharacterState;
using static GlobalData;

public class CharacterAction : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    CanvasGroup group;
    CharacterState character;
    Button button;
    public enum RecastType { standard, longGcd, stackedOgcd }

    [Header("Info")]
    public CharacterActionData data;
    private float timer = 0f;
    private float aTimer = 0f;
    public bool isAvailable { private set; get; }
    public bool isAnimationLocked { private set; get; }
    public bool isAutoAction = false;
    private bool wasIsAutoAction = false;
    public bool isDisabled;
    private bool wasIsDisabled;
    public bool unavailable = false;
    private bool wasUnavailable = false;
    public bool invisible = false;
    private bool wasInvisible = false;
    public bool hasTarget = false;
    public float damageMultiplier = 1f;
    public float distanceToTarget;
    public int chargesLeft = 0;
    public CharacterActionData lastAction;
    public KeyBind currentKeybind;
    public List<Role> availableForRoles;
    public List<CharacterAction> sharedRecasts;
    public List<StatusEffectData> comboOutlineEffects;
    public List<StatusEffectData> hideWhileEffects;
    public bool showInsteadWithEffects = false;
    public bool invisibilityAlsoDisables = false;

    [Header("Events")]
    public UnityEvent<ActionInfo> onExecute;
    public UnityEvent<ActionInfo> onCast;
    public UnityEvent<ActionInfo> onInterrupt;

    [Header("Visuals")]
    public RecastType recastType = RecastType.standard;
    public CanvasGroup borderStandard;
    public CanvasGroup borderDark;
    public Animator recastFillAnimator;
    public CanvasGroup recastFillGroup;
    public CanvasGroup selectionBorder;
    public CanvasGroup clickHighlight;
    public CanvasGroup comboOutlineGroup;
    public TextMeshProUGUI recastTimeText;
    public TextMeshProUGUI resourceCostText;
    public TextMeshProUGUI keybindText;
    public HudElementColor[] hudElements;
    public List<Color> defaultColors;
    public List<Color> unavailableColors;

    private string[] animations = new string[]
    {
        "ui_hotbar_recast_type1_none",
        "ui_hotbar_recast_type1_fill",
        "ui_hotbar_recast_type2_fill",
        "ui_hotbar_recast_type3_fill"
    };
    private int[] animationHashes = new int[0];
    private bool pointer;
    private bool colorsFlag;
    private bool animFlag;
    private RecastType normalRecastType;
    private bool chargeRestored = false;
    private bool permanentlyUnavailable = false;
    private float lastRecast = 0f;
    private int id = 0;
    public CharacterState GetCharacter()
    {
        return character;
    }

    void Awake()
    {
        button = GetComponent<Button>();
        group = GetComponent<CanvasGroup>();

        chargesLeft = data.charges;
        permanentlyUnavailable = unavailable;

        if (borderStandard != null)
        {
            borderStandard.alpha = 1f;
        }
        if (borderDark != null)
        {
            borderDark.alpha = 0f;
        }
        if (selectionBorder != null)
        {
            selectionBorder.alpha = 0f;
        }
        if (clickHighlight != null)
        {
            clickHighlight.alpha = 0f;
        }
        if (recastFillGroup != null)
        {
            recastFillGroup.alpha = 0f;
        }
        if (recastFillAnimator != null)
        {
            recastFillAnimator.speed = 0f;
            recastFillAnimator.Play("ui_hotbar_recast_type1_none");
        }
        if (resourceCostText != null)
        {
            if (data.manaCost > 0)
            {
                resourceCostText.text = data.manaCost.ToString();
            }
            else
            {
                resourceCostText.text = string.Empty;
            }
        }
        if (comboOutlineGroup != null)
        {
            comboOutlineGroup.alpha = 0f;
        }

        for (int i = 0; i < hudElements.Length; i++)
        {
            hudElements[i].SetColor(defaultColors);
        }

        normalRecastType = recastType;

        animationHashes = new int[animations.Length];
        for (int i = 0; i < animations.Length; i++)
        {
            animationHashes[i] = Animator.StringToHash(animations[i]);
        }

        id = UnityEngine.Random.Range(0, 10000);
    }

    void Start()
    {
        if (unavailable)
            Utilities.FunctionTimer.Create(this, () => gameObject.SetActive(false), 0.2f, $"{data.actionName}_{gameObject.name}_{id}_start_disable_delay");
    }

    void Update()
    {
        if (character != null && availableForRoles != null && availableForRoles.Count > 0)
        {
            foreach (Role role in availableForRoles)
            {
                if (role == character.role)
                {
                    if (!permanentlyUnavailable)
                    {
                        unavailable = false;
                    }
                    break;
                }
                else
                {
                    unavailable = true;
                }
            }
        }

        if (unavailable)
        {
            if (!colorsFlag)
            {
                for (int i = 0; i < hudElements.Length; i++)
                {
                    hudElements[i].SetColor(unavailableColors);
                }
                colorsFlag = true;
            }

            if (resourceCostText != null)
            {
                resourceCostText.text = "X";
            }

            ResetAnimationLock();
            ResetCooldown();
            chargesLeft = data.charges;
        }

        if (hideWhileEffects != null && hideWhileEffects.Count > 0 && character != null)
        {
            if (!showInsteadWithEffects)
                invisible = false;
            else
                invisible = true;
            for (int i = 0; i < hideWhileEffects.Count; i++)
            {
                if (character.HasAnyVersionOfEffect(hideWhileEffects[i].statusName))
                {
                    if (!showInsteadWithEffects)
                        invisible = true;
                    else
                        invisible = false;
                }
            }
        }

        if (group != null)
        {
            if (invisible)
            {
                if (invisibilityAlsoDisables)
                {
                    isDisabled = true;
                }
                group.alpha = 0f;
                group.interactable = false;
                group.blocksRaycasts = false;
            }
            else
            {
                if (invisibilityAlsoDisables)
                {
                    isDisabled = false;
                }
                group.alpha = 1f;
                group.interactable = true;
                group.blocksRaycasts = true;
            }
        }

        if (data.range > 0f && data.isTargeted && (distanceToTarget > data.range) && hasTarget && !unavailable)
        {
            isAvailable = false;
            if (!colorsFlag)
            {
                for (int i = 0; i < hudElements.Length; i++)
                {
                    hudElements[i].SetColor(unavailableColors);
                }
                colorsFlag = true;
            }

            if (resourceCostText != null)
            {
                if (data.manaCost <= 0 && data.charges < 2)
                {
                    resourceCostText.text = "X";
                }
                else if (data.manaCost > 0)
                {
                    resourceCostText.text = data.manaCost.ToString();
                }
                else if (data.charges > 1)
                {
                    resourceCostText.text = chargesLeft.ToString();
                }
            }
        }
        else if (!unavailable)
        {
            if (data.range > 0f && data.isTargeted && (distanceToTarget <= data.range) && hasTarget && data.charges > 1 && chargesLeft > 0)
            {
                isAvailable = true;
            }

            if (colorsFlag)
            {
                for (int i = 0; i < hudElements.Length; i++)
                {
                    hudElements[i].SetColor(defaultColors);
                }
                colorsFlag = false;
            }

            if (resourceCostText != null)
            {
                if (data.manaCost <= 0 && data.charges < 2)
                {
                    resourceCostText.text = string.Empty;
                }
                else if (data.manaCost > 0)
                {
                    resourceCostText.text = data.manaCost.ToString();
                }
                else if (data.charges > 1)
                {
                    resourceCostText.text = chargesLeft.ToString();
                }
            }
        }

        if (aTimer > 0f)
        {
            aTimer -= Time.unscaledDeltaTime;
            isAnimationLocked = true;
        }
        else
        {
            aTimer = 0f;
            isAnimationLocked = false;
        }
        if (timer > 0f)
        {
            timer -= FightTimeline.deltaTime;
            chargeRestored = false;
            if (chargesLeft < 1)
            {
                if (chargesLeft < 0)
                    chargesLeft = 0;

                isAvailable = false;
                recastType = normalRecastType;
            }
            else if (data.charges > 1)
            {
                recastType = RecastType.stackedOgcd;
            }
            if (recastFillGroup != null)
            {
                recastFillGroup.alpha = 1f;
            }
            if (recastTimeText != null && lastRecast > 2.5f)
            {
                recastTimeText.text = timer.ToString("F0");
            }
        }
        else
        {
            timer = 0f;
            if (!chargeRestored || chargesLeft < 1)
            {
                chargesLeft++;
                chargeRestored = true;
            }
            if (chargesLeft < data.charges && data.charges >= 1)
            {
                ActivateCooldown();
            }
            else if (chargesLeft >= data.charges)
            {
                chargesLeft = data.charges;
            }
            if (chargesLeft > 0)
            {
                isAvailable = true;
            }
            if (recastFillGroup != null)
            {
                recastFillGroup.alpha = 0f;
            }
            if (recastTimeText != null)
            {
                recastTimeText.text = "";
            }
            if (borderStandard != null)
            {
                borderStandard.alpha = 1f;
            }
            if (recastType == RecastType.longGcd && borderDark != null)
            {
                borderDark.alpha = 0f;
            }
        }

        if ((lastAction != null && data.comboAction != null) || (comboOutlineEffects != null && comboOutlineEffects.Count > 0))
        {
            bool showOutline = false;

            if (comboOutlineEffects != null && comboOutlineEffects.Count > 0 && character != null)
            {
                for (int i = 0; i < comboOutlineEffects.Count; i++)
                {
                    if (character.HasAnyVersionOfEffect(comboOutlineEffects[i].statusName))
                    {
                        showOutline = true;
                        break;
                    }
                }
            }

            if (lastAction != null && data.comboAction != null)
            {
                if (lastAction == data.comboAction && comboOutlineGroup != null)
                {
                    showOutline = true;
                }
            }

            if (showOutline)
            {
                comboOutlineGroup.alpha = 1f;
            }
            else if (comboOutlineGroup != null)
            {
                comboOutlineGroup.alpha = 0f;
            }
        }
        else if (comboOutlineGroup != null)
        {
            comboOutlineGroup.alpha = 0f;
        }

        if (borderStandard != null && timer > 0f)
        {
            if (recastType == RecastType.stackedOgcd)
            {
                borderStandard.alpha = 1f;
            }
            else
            {
                borderStandard.alpha = 0f;
            }

            if (recastType == RecastType.longGcd && borderDark != null)
            {
                borderDark.alpha = 1f;
            }
        }
        if (recastFillAnimator != null && timer > 0f)
        {
            animFlag = false;
            //Debug.Log($"new '{animations[(int)recastType + 1]}' old 'ui_hotbar_recast_type{(int)recastType + 1}_fill'");
            recastFillAnimator.Play(animationHashes[(int)recastType + 1], 0, Utilities.Map(lastRecast - timer, 0f, lastRecast, 0f, 1f));
        }
        else if (recastFillAnimator != null && !animFlag)
        {
            animFlag = true;
            recastFillAnimator.Play(animationHashes[0]);
        }

        if (button != null)
        {
            if (!isDisabled && !unavailable)
            {
                button.interactable = isAvailable;
            }
            else
            {
                button.interactable = false;
            }
        }

        if (keybindText != null)
        {
            if (currentKeybind != null)
            {
                keybindText.text = currentKeybind.ToShortString();
            }
            else
            {
                keybindText.text = string.Empty;
            }
        }
    }

    public void Initialize(ActionController controller)
    {
        if (button == null)
            button = GetComponent<Button>();

        if (button != null)
        {
            //Debug.Log($"action {gameObject.name} of {controller.gameObject.name} button was linked!");
            button.onClick.AddListener(() => { controller.PerformAction(this); });
        }

        if (controller != null)
        {
            character = controller.GetComponent<CharacterState>();
        }
    }

    public void ExecuteAction(ActionInfo actionInfo)
    {
        if (unavailable)
            return;

        if (data.buff != null)
        {
            if (data.buff.toggle || data.dispelBuffInstead)
            {
                if (actionInfo.source.HasEffect(data.buff.statusName))
                {
                    actionInfo.source.RemoveEffect(data.buff, false, actionInfo.source);
                }
                else if (!data.dispelBuffInstead)
                {
                    actionInfo.source.AddEffect(data.buff, actionInfo.source, actionInfo.sourceIsPlayer);
                }
            }
            else
            {
                actionInfo.source.AddEffect(data.buff, actionInfo.source, actionInfo.sourceIsPlayer);
            }
        }
        if (data.debuff != null)
        {
            if (actionInfo.target != null)
            {
                if (data.debuff.toggle || data.dispelDebuffInstead)
                {
                    if (actionInfo.source.HasEffect(data.debuff.statusName))
                    {
                        actionInfo.source.RemoveEffect(data.debuff, false, actionInfo.source);
                    }
                    else if (!data.dispelDebuffInstead)
                    {
                        actionInfo.source.AddEffect(data.debuff, actionInfo.source, actionInfo.sourceIsPlayer);
                    }
                }
                else
                {
                    actionInfo.source.AddEffect(data.debuff, actionInfo.source, actionInfo.sourceIsPlayer);
                }
            }
        }

        if (actionInfo.action.data.isTargeted && actionInfo.target != null)
        {
            if (actionInfo.target.targetController != null && actionInfo.target.targetController.self != null)
            {
                if (actionInfo.action.data.targetGroups.Contains(actionInfo.target.targetController.self.Group))
                {
                    int calculatedDamage = Mathf.RoundToInt((actionInfo.action.data.damage.value * damageMultiplier) * actionInfo.source.currentDamageOutputMultiplier);

                    if (actionInfo.action.data.causesDirectDamage)
                    {
                        actionInfo.target.ModifyHealth(new Damage(actionInfo.action.data.damage, calculatedDamage, actionInfo.action.data.damage.name));
                    }

                    //Debug.Log($"Action {actionInfo.action.data.actionName} executed and hit {actionInfo.target.characterName}");

                    if (actionInfo.action.data.isHeal && actionInfo.source != null && actionInfo.action.data.causesDirectDamage)
                    {
                        actionInfo.source.ModifyHealth(new Damage(Mathf.Abs(calculatedDamage), false, actionInfo.source, actionInfo.action.data.damage.name));
                    }

                    if (actionInfo.action.data.damage.negative && actionInfo.action.data.damageEnmityMultiplier != 0f)
                    {
                        if (actionInfo.source != null)
                        {
                            if (actionInfo.action.data.topEnmity && actionInfo.source.partyList != null)
                            {
                                // Set to current max enmity
                                CharacterState highestEnmityMember = actionInfo.source.partyList.GetHighestEnmityMember(actionInfo.target);
                                long highestEnmity = 0;
                                highestEnmityMember.enmity.TryGetValue(actionInfo.target, out highestEnmity);
                                actionInfo.source.ResetEnmity(actionInfo.target);
                                actionInfo.source.SetEnmity(highestEnmity, actionInfo.target);
                            }

                            actionInfo.source.AddEnmity(Math.Abs(actionInfo.action.data.enmity), actionInfo.target);
                            actionInfo.source.AddEnmity(Math.Abs(Mathf.RoundToInt(calculatedDamage * actionInfo.action.data.damageEnmityMultiplier * actionInfo.source.enmityGenerationModifier)), actionInfo.target);
                        }
                    }
                }
            }
        } 
        else if (actionInfo.action.data.isTargeted && (actionInfo.target == null || !hasTarget))
        {
            return;
        }

        onExecute.Invoke(actionInfo);
    }

    public void ActivateActionUse()
    {
        if (unavailable || invisible || isDisabled)
            return;

        chargesLeft--;
        ActivateCooldown();
        ActivateAnimationLock();
        OnPointerClick(null);
    }

    public void ActivateCooldown(bool shared = false)
    {
        if (unavailable)
            return;

        if (chargesLeft < 1)
        {
            isAvailable = false;
        }
        if (timer <= 0f)
        {
            lastRecast = data.recast;
            timer = data.recast;
        }

        if (!shared)
        {
            if (sharedRecasts != null && sharedRecasts.Count > 0)
            {
                foreach (CharacterAction sharedRecast in sharedRecasts)
                {
                    sharedRecast.chargesLeft--;
                    sharedRecast.ActivateCooldown(true);
                }
            }
        }
    }

    public void ActivateCooldown(float recast)
    {
        if (unavailable)
            return;

        chargesLeft--;

        if (chargesLeft < 1)
        {
            isAvailable = false;
        }
        if (timer <= 0f)
        {
            lastRecast = recast;
            timer = recast;
        }
    }

    public void ActivateAnimationLock(bool shared = false)
    {
        if (unavailable)
            return;

        isAnimationLocked = true;
        aTimer = data.animationLock;

        if (!shared)
        {
            if (sharedRecasts != null && sharedRecasts.Count > 0)
            {
                foreach (CharacterAction sharedRecast in sharedRecasts)
                {
                    sharedRecast.ActivateAnimationLock(true);
                }
            }
        }
    }

    public void ActivateAnimationLock(float duration)
    {
        if (unavailable)
            return;

        isAnimationLocked = true;  
        aTimer = duration;
    }

    public void ResetCooldown(bool shared = false)
    {
        chargesLeft++;
        if (chargesLeft > data.charges)
        {
            chargesLeft = data.charges;
        }
        isAvailable = true;
        timer = 0f;

        if (!shared)
        {
            if (sharedRecasts != null && sharedRecasts.Count > 0)
            {
                foreach (CharacterAction sharedRecast in sharedRecasts)
                {
                    sharedRecast.ResetCooldown(true);
                }
            }
        }
    }

    public void ResetAnimationLock(bool shared = false)
    {
        isAnimationLocked = false;
        aTimer = 0f;

        if (!shared)
        {
            if (sharedRecasts != null && sharedRecasts.Count > 0)
            {
                foreach (CharacterAction sharedRecast in sharedRecasts)
                {
                    sharedRecast.ResetAnimationLock(true);
                }
            }
        }
    }

    public void ResetAction()
    {
        ResetCooldown();
        ResetAnimationLock();
        chargesLeft = data.charges;
        isAutoAction = wasIsAutoAction;
        isDisabled = wasIsDisabled;
        unavailable = wasUnavailable;
        invisible = wasInvisible;
        if (permanentlyUnavailable)
        {
            unavailable = true;
        }
        if (unavailable)
            gameObject.SetActive(false);
    }

    public void ToggleState(bool state)
    {
        unavailable = !state;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (unavailable)
            return;

        pointer = true;
        if (selectionBorder != null)
            selectionBorder.LeanAlpha(1f, 0.25f);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (unavailable)
            return;

        pointer = false;
        if (selectionBorder != null)
            selectionBorder.LeanAlpha(0f, 0.25f);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (unavailable)
            return;

        if (clickHighlight == null) //(eventData == null && pointer) || 
        {
            return;
        }

        clickHighlight.transform.localScale = new Vector3(0f, 0f, 1f);
        clickHighlight.transform.LeanScale(new Vector3(1.5f, 1.5f, 1f), 0.5f);//.setOnComplete(() => { clickHighlight.LeanAlpha(0f, 0.15f); });
        Utilities.FunctionTimer.Create(this, () => clickHighlight.LeanAlpha(0f, 0.15f), 0.3f, $"{transform.parent.gameObject.name}_{gameObject.name}_click_animation_fade_delay", true, false);
        clickHighlight.LeanAlpha(1f, 0.15f);
        if (!pointer)
            selectionBorder.LeanAlpha(1f, 0.15f).setOnComplete(() => Utilities.FunctionTimer.Create(this, () => selectionBorder.LeanAlpha(0f, 0.15f), 0.2f, $"{transform.parent.gameObject.name}_{gameObject.name}_click_highlight_fade_delay", true, false));
    }
}
