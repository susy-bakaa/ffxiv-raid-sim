using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ActionController : MonoBehaviour
{
    Animator animator;
    CharacterState characterState;

    public List<CharacterAction> actions = new List<CharacterAction>();
    public bool instantCast = false;
    public bool isAnimationLocked = false;
    public bool isCasting = false;
    public bool waitInterruptedCasts = false;
    public bool lockActionsWhenCasting = true;
    private bool previousCanDoActions;
    private bool previousIsCasting;

    public CharacterState currentTarget;

    [Header("Personal")]
    public bool useCastBar;
    public Slider castBar;
    public TextMeshProUGUI castNameText;
    public TextMeshProUGUI castLengthText;
    public CanvasGroup interruptText;
    public HudElement castBarElement;

    [Header("Party")]
    public bool usePartyCastBar;
    public Slider castBarParty;
    public TextMeshProUGUI castNameTextParty;
    public bool hideNameWhenCasting = false;

    private CanvasGroup castBarGroupParty;
    private CanvasGroup castBarGroup;

    private CharacterAction lastAction;
    public CharacterAction LastAction { get { return lastAction; } }
    private float castTime;
    public float CastTime { get { return castTime; } }
    private float lastCastTime;
    public float LastCastTime { get { return lastCastTime; } }
    private bool interrupted;
    public bool Interrupted { get { return interrupted; } }

    [Header("Events")]
    public UnityEvent<CastInfo> onCast;
    public UnityEvent onResetCastBar;

    void Awake()
    {
        animator = GetComponent<Animator>();
        characterState = GetComponent<CharacterState>();

        if (castBar != null)
            castBarGroup = castBar.GetComponent<CanvasGroup>();
        if (castBarParty != null)
            castBarGroupParty = castBarParty.GetComponentInParent<CanvasGroup>();

        if (interruptText != null)
        {
            interruptText.alpha = 0f;
        }

        if (characterState != null )
        {
            for (int i = 0; i < actions.Count; i++)
            {
                actions[i].Initialize(this);
            }
        }
        previousCanDoActions = characterState.canDoActions;
    }

    void Update()
    {
        if (!gameObject.activeSelf)
            return;

        if (Time.timeScale <= 0f)
            return;

        if (previousCanDoActions != characterState.canDoActions || previousIsCasting != isCasting)
        {
            previousCanDoActions = characterState.canDoActions;
            previousIsCasting = isCasting;
            if (characterState.canDoActions && ((!isCasting && lockActionsWhenCasting) || (!lockActionsWhenCasting)))
            {
                for (int i = 0; i < actions.Count; i++)
                {
                    actions[i].isDisabled = false;
                }
            }
            else
            {
                for (int i = 0; i < actions.Count; i++)
                {
                    actions[i].isDisabled = true;
                }
            }
        }

        if (castTime > 0f && !interrupted)
        {
            // Simulate FFXIV slidecasting, which is 500ms
            if ((!characterState.still || characterState.dead) && castTime > 0.5f)
            {
                Interrupt();
            }
            if (!characterState.still)
            {
                if (lastAction != null)
                {
                    if (!lastAction.data.canBeSlideCast)
                        Interrupt();
                }
            }

            castTime -= FightTimeline.deltaTime;
            if (castBar != null)
            {
                castBar.value = lastCastTime - castTime;

                if (castBarGroup.alpha == 0f)
                {
                    castBarGroup.LeanAlpha(1f, 0.1f);
                }
                if (interruptText != null)
                {
                    interruptText.alpha = 0f;
                }
            }
            if (castBarParty != null)
            {
                castBarParty.value = lastCastTime - castTime;

                if (castBarGroupParty.alpha == 0f)
                {
                    castBarGroupParty.alpha = 1f;
                }
            }
            if (castLengthText != null)
            {
                castLengthText.text = castTime.ToString("00.00");
            }
        }
        else
        {
            if (interrupted)
            {
                if (castBar != null && castBarGroup.alpha == 1f)
                {
                    castBarGroup.alpha = 0.99f;
                    Utilities.FunctionTimer.Create(() => castBarGroup.LeanAlpha(0f, 0.5f), 2f, $"{this}_castBar_fade_out_if_interrupted", true);
                }
                if (castBarParty != null && castBarGroupParty.alpha == 1f)
                {
                    castBarGroupParty.alpha = 0f;
                    //Utilities.FunctionTimer.Create(() => castBarGroupParty.alpha = 0f, 2f, $"{this}_castBarParty_fade_out_if_interrupted", true);
                }
                Utilities.FunctionTimer.Create(() => ResetCastBar(), 2.5f, $"{this}_interrupted_status", true);
            }
            else
            {
                if (castBar != null && castBarGroup.alpha == 1f)
                {
                    castBarGroup.alpha = 0.99f;
                    castBarGroup.LeanAlpha(0f, 0.5f);
                }
                if (castBarParty != null && castBarGroupParty.alpha == 1f)
                {
                    castBarGroupParty.alpha = 0f;
                }
            }
        }

        if (hideNameWhenCasting)
        {
            if (characterState.characterNameTextParty != null)
            {
                characterState.hidePartyName = isCasting;
            }
        }
        else
        {
            characterState.hidePartyName = false;
        }
    }

    public void PerformAction(string name)
    {
        if (!gameObject.activeSelf)
            return;

        if (characterState.dead)
            return;

        if (Time.timeScale <= 0f)
            return;

        for (int i = 0; i < actions.Count; i++)
        {
            if (actions[i].data.actionName == name)
            {
                PerformAction(actions[i]);
            }
        }
    }

    public void PerformAction(CharacterAction action)
    {
        if (!gameObject.activeSelf)
            return;

        if (characterState.dead)
            return;

        if (Time.timeScale <= 0f)
            return;

        if (action == null)
            return;

        action.OnPointerClick(null);

        if (lockActionsWhenCasting && isCasting)
            return;

        interrupted = false;
        if (castBarElement != null)
            castBarElement.ChangeColors(false);

        if (action.isAvailable && !action.isDisabled && !action.isAnimationLocked)
        {
            if (action.data.cast <= 0f || instantCast || (action.instantUnderEffect != null && characterState.HasEffect(action.instantUnderEffect.statusName)))
            {
                ActionInfo newActionInfo = new ActionInfo(action, characterState, currentTarget);
                action.ExecuteAction(newActionInfo);

                action.ActivateAnimationLock();
                if (action.instantUnderEffect == null)
                    action.ActivateCooldown();
                else if (action.instantUnderEffect.rollsCooldown)
                    action.ActivateCooldown();

                onCast.Invoke(new CastInfo(newActionInfo, instantCast, characterState.GetEffects()));
                if (animator != null)
                {
                    animator.SetBool("Casting", false);
                }
            }
            else
            {
                Utilities.FunctionTimer.StopTimer($"{gameObject}_{GetHashCode()}_castBar_fade_out_if_interrupted");
                //Utilities.FunctionTimer.StopTimer($"{this}_castBarParty_fade_out_if_interrupted");
                Utilities.FunctionTimer.StopTimer($"{gameObject}_{GetHashCode()}_interrupted_status");
                ResetCastBar();
                if (castBarGroup != null)
                    castBarGroup.alpha = 0f;
                if (castBarGroupParty != null)
                    castBarGroupParty.alpha = 0f;

                isCasting = true;
                lastAction = action;
                castTime = action.data.cast;
                lastCastTime = castTime;

                action.ActivateAnimationLock();
                if (action.instantUnderEffect == null)
                    action.ActivateCooldown();
                else if (action.instantUnderEffect.rollsCooldown)
                    action.ActivateCooldown();

                ActionInfo newActionInfo = new ActionInfo(action, characterState, currentTarget);
                StartCoroutine(Cast(castTime, () => { action.ExecuteAction(newActionInfo); }));
                onCast.Invoke(new CastInfo(newActionInfo, instantCast, characterState.GetEffects()));
                characterState.UpdateCharacterName();
                if (castBar != null)
                {
                    castBar.maxValue = action.data.cast;
                    castBar.value = 0f;
                }
                if (castLengthText != null)
                {
                    castLengthText.text = castTime.ToString("00.00");
                }
                if (castNameText != null)
                {
                    castNameText.text = Utilities.InsertSpaceBeforeCapitals(action.data.actionName);
                }
                if (castBarParty != null)
                {
                    castBarParty.maxValue = action.data.cast;
                    castBarParty.value = 0f;
                }
                if (castNameTextParty != null)
                {
                    castNameTextParty.text = Utilities.InsertSpaceBeforeCapitals(action.data.actionName);
                }
                if (interruptText != null)
                {
                    interruptText.alpha = 0f;
                }
                if (animator != null)
                {
                    animator.SetBool("Casting", true);
                }
            }
        }
        else if (characterState.canDoActions && !action.isDisabled && !action.isAnimationLocked)
        {
            FailAction(action, "Action not ready yet.");
        }
        else if (characterState.canDoActions && action.isDisabled && !action.isAnimationLocked)
        {
            FailAction(action, "Action not available right now.");
        }
        else if (characterState.canDoActions && action.isAnimationLocked)
        {
            FailAction(action, "Action not finished and available yet.");
        }
        else
        {
            FailAction(action, "Actions not available right now.");
        }
    }

    public void FailAction(CharacterAction action, string reason)
    {
        if (!gameObject.activeSelf)
            return;

        if (Time.timeScale <= 0f)
            return;

        if (characterState.dead)
            return;

        // IDK
    }

    private void Interrupt()
    {
        if (!gameObject.activeSelf)
            return;

        if (Time.timeScale <= 0f)
            return;

        interrupted = true;
        isCasting = false;
        if (interruptText != null)
        {
            interruptText.LeanAlpha(1f, 0.5f);
        }
        if (lastAction != null)
        {
            lastAction.ResetAnimationLock();
            lastAction.ResetCooldown();
            lastAction = null;
        }
        if (animator != null)
        {
            animator.SetBool("Casting", false);
        }
        if (castBarElement != null)
            castBarElement.ChangeColors(true);
        StopAllCoroutines();
        characterState.UpdateCharacterName();
        //Utilities.FunctionTimer.Create(() => { ResetCastBar(); }, 4f, $"{this}_interrupt", true);
    }

    private IEnumerator Cast(float length, Action action)
    {
        yield return new WaitForSeconds(length);
        action.Invoke();
        isCasting = false;
        lastAction = null;
        characterState.UpdateCharacterName();
        if (animator != null)
        {
            animator.SetBool("Casting", false);
        }
    }

    private void ResetCastBar()
    {
        if (!gameObject.activeSelf)
            return;

        if (Time.timeScale <= 0f)
            return;

        interrupted = false;
        castTime = 0f;

        onResetCastBar.Invoke();

        if (castBar != null)
        {
            castBar.value = 0f;
        }
        if (castBarParty != null)
        {
            castBarParty.value = 0f;
        }
        if (interruptText != null)
        {
            interruptText.alpha = 0f;
        }
    }

    public struct ActionInfo
    {
        public CharacterAction action;
        public CharacterState source;
        public CharacterState target;
        public bool sourceIsPlayer;
        public bool targetIsPlayer;

        public ActionInfo(CharacterAction action, CharacterState source, CharacterState target)
        {
            this.action = action;
            this.source = source;
            this.target = target;
            sourceIsPlayer = false;
            targetIsPlayer = false;

            if (source != null && source == FightTimeline.Instance.player)
            {
                sourceIsPlayer = true;
            }
            if (target != null && target == FightTimeline.Instance.player)
            {
                targetIsPlayer = true;
            }
        }
    }

    public struct CastInfo
    {
        public ActionInfo action;
        public bool wasInstant;
        public StatusEffect[] effects;

        public CastInfo(ActionInfo action, bool wasInstant, StatusEffect[] effects)
        {
            this.action = action;
            this.wasInstant = wasInstant;
            this.effects = effects;
        }
    }
}
