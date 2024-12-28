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
using static GlobalData;

public class CharacterAction : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    Button button;
    public enum RecastType { standard, longGcd, stackedOgcd }

    [Header("Info")]
    public CharacterActionData data;
    private float timer = 0f;
    private float aTimer = 0f;
    public bool isAvailable { private set; get; }
    public bool isAnimationLocked { private set; get; }
    public bool isAutoAction = false;
    public bool isDisabled;
    public bool unavailable = false;
    public bool hasTarget = false;
    public float damageMultiplier = 1f;
    public float distanceToTarget;
    public int chargesLeft = 0;
    public CharacterActionData lastAction;
    public KeyBind currentKeybind;

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

    private bool pointer;
    private bool colorsFlag;
    private bool animFlag;
    private RecastType normalRecastType;
    private bool chargeRestored = false;

    void Awake()
    {
        button = GetComponent<Button>();

        chargesLeft = data.charges;

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
    }

    void Start()
    {
        if (unavailable)
            Utilities.FunctionTimer.Create(this, () => gameObject.SetActive(false), 0.2f, $"{data.actionName}_{this}_start_disable_delay");
    }

    void Update()
    {
        if (data.range > 0f && data.isTargeted && (distanceToTarget > data.range) && hasTarget)
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
        else
        {
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
            else
            {
                recastType = RecastType.stackedOgcd;
            }
            if (recastFillGroup != null)
            {
                recastFillGroup.alpha = 1f;
            }
            if (recastTimeText != null && data.recast > 2.5f)
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

        if (lastAction != null && data.comboAction != null)
        {
            if (lastAction == data.comboAction && comboOutlineGroup != null)
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
            recastFillAnimator.Play($"ui_hotbar_recast_type{(int)recastType + 1}_fill", 0, Utilities.Map(data.recast - timer, 0f, data.recast, 0f, 1f));
        }
        else if (recastFillAnimator != null && !animFlag)
        {
            animFlag = true;
            recastFillAnimator.Play("ui_hotbar_recast_type1_none");
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
    }

    public void ExecuteAction(ActionInfo actionInfo)
    {
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

        onExecute.Invoke(actionInfo);
    }

    public void ActivateCooldown()
    {
        if (chargesLeft < 1)
        {
            isAvailable = false;
        }
        if (timer <= 0f)
        {
            timer = data.recast;
        }
    }

    public void ActivateAnimationLock()
    {
        isAnimationLocked = true;
        aTimer = data.animationLock;
    }

    public void ResetCooldown()
    {
        chargesLeft++;
        if (chargesLeft > data.charges)
        {
            chargesLeft = data.charges;
        }
        isAvailable = true;
        timer = 0f;
    }

    public void ResetAnimationLock()
    {
        isAnimationLocked = false;
        aTimer = 0f;
    }

    public void ToggleState(bool state)
    {
        unavailable = !state;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        pointer = true;
        if (selectionBorder != null)
            selectionBorder.LeanAlpha(1f, 0.25f);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        pointer = false;
        if (selectionBorder != null)
            selectionBorder.LeanAlpha(0f, 0.25f);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
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
