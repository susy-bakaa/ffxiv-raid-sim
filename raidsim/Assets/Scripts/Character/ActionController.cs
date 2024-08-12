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
    TargetController targetController;

    public List<CharacterAction> actions = new List<CharacterAction>();
    public List<CharacterAction> autoActions = new List<CharacterAction>();
    private CharacterAction autoAttack;
    public bool autoAttackEnabled = true;
    public bool instantCast = false;
    public bool isAnimationLocked = false;
    public bool isCasting = false;
    public bool waitInterruptedCasts = false;
    public bool lockActionsWhenCasting = true;
    private bool previousCanDoActions;
    private bool previousIsCasting;
    public float distanceToTarget = 0f;

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
    public bool showCastTargetLetter = true;

    private CanvasGroup castBarGroupParty;
    private CanvasGroup castBarGroup;

    [Header("Events")]
    public UnityEvent<CastInfo> onCast;
    public UnityEvent onResetCastBar;

    private StatusEffect instantCastEffect;
    private CharacterAction lastAction;
    public CharacterAction LastAction { get { return lastAction; } }
    private float castTime;
    public float CastTime { get { return castTime; } }
    private float lastCastTime;
    public float LastCastTime { get { return lastCastTime; } }
    private bool interrupted;
    public bool Interrupted { get { return interrupted; } }

    private int rateLimit;
    private float autoAttackTimer;
    private Queue<CharacterAction> queuedAutoActions = new Queue<CharacterAction>();

    void Awake()
    {
        animator = GetComponent<Animator>();
        characterState = GetComponent<CharacterState>();
        targetController = GetComponent<TargetController>();

        if (characterState != null)
            characterState.onInstantCastsChanged.AddListener(UpdateInstantCasts);

        if (castBar != null)
            castBarGroup = castBar.GetComponent<CanvasGroup>();
        if (castBarParty != null)
            castBarGroupParty = castBarParty.GetComponentInParent<CanvasGroup>();

        if (interruptText != null)
        {
            interruptText.alpha = 0f;
        }

        if (characterState != null)
        {
            for (int i = 0; i < actions.Count; i++)
            {
                actions[i].Initialize(this);
            }
        }
        previousCanDoActions = characterState.canDoActions;

        rateLimit = UnityEngine.Random.Range(15, 26);

        if (autoActions != null && autoActions.Count > 0)
        {
            autoAttack = autoActions[0];
            autoAttackTimer = autoAttack.data.recast;
        }
    }

    void Update()
    {
        if (!gameObject.activeSelf)
            return;

        if (Time.timeScale <= 0f)
            return;

        bool hasTarget = false;

        if (targetController != null)
        {
            if (targetController.currentTarget != null)
            {
                hasTarget = true;
            }
        }
        if (Utilities.RateLimiter(rateLimit))
        {
            if (hasTarget)
            {
                // Get the position of the current target and the radius offset
                Vector3 targetPosition = targetController.currentTarget.transform.position;
                float radiusOffset = targetController.currentTarget.hitboxRadius;

                // Calculate the distance from the current position to the target position
                float rawDistanceToTarget = Vector3.Distance(transform.position, targetPosition);

                // Subtract the radius offset to account for the circular shape
                distanceToTarget = Mathf.Max(0, rawDistanceToTarget - radiusOffset); // Ensure the distance is not negative
            }
            
            if (actions != null && actions.Count > 0)
            {
                for (int i = 0; i < actions.Count; i++)
                {
                    if (hasTarget)
                        actions[i].distanceToTarget = distanceToTarget;
                    actions[i].hasTarget = hasTarget;
                }
            }
            if (autoActions != null && autoActions.Count > 1)
            {
                for (int i = 0; i < autoActions.Count; i++)
                {
                    if (hasTarget)
                        autoActions[i].distanceToTarget = distanceToTarget;
                    autoActions[i].hasTarget = hasTarget;
                }
            } 
            else if (autoActions.Count == 1 && autoAttack != null)
            {
                if (hasTarget)
                    autoAttack.distanceToTarget = distanceToTarget;
                autoAttack.hasTarget = hasTarget;
            }
        }

        if (previousCanDoActions != characterState.canDoActions || previousIsCasting != isCasting)
        {
            previousCanDoActions = characterState.canDoActions;
            previousIsCasting = isCasting;
            if (characterState.canDoActions && ((!isCasting && lockActionsWhenCasting) || (!lockActionsWhenCasting)))
            {
                if (actions != null && actions.Count > 0)
                {
                    for (int i = 0; i < actions.Count; i++)
                    {
                        actions[i].isDisabled = false;
                    }
                }
                if (autoActions != null && autoActions.Count > 1)
                {
                    for (int i = 0; i < autoActions.Count; i++)
                    {
                        autoActions[i].isDisabled = false;
                    }
                }
                else if (autoActions.Count == 1 && autoAttack != null)
                {
                    autoAttack.isDisabled = false;
                }
            }
            else
            {
                if (actions != null && actions.Count > 0)
                {
                    for (int i = 0; i < actions.Count; i++)
                    {
                        actions[i].isDisabled = true;
                    }
                }
                if (autoActions != null && autoActions.Count > 1)
                {
                    for (int i = 0; i < autoActions.Count; i++)
                    {
                        autoActions[i].isDisabled = true;
                    }
                }
                else if (autoActions.Count == 1 && autoAttack != null)
                {
                    autoAttack.isDisabled = true;
                }
            }
        }

        if (castTime > 0f && !interrupted)
        {
            // Simulate FFXIV slidecasting, which is 500ms
            if (lastAction != null)
            {
                if ((!characterState.still || characterState.dead || (lastAction.data.range > 0f && lastAction.data.isTargeted && (distanceToTarget > lastAction.data.range))) && castTime > 0.5f)
                {
                    Interrupt();
                }
            }
            else
            {
                if ((!characterState.still || characterState.dead) && castTime > 0.5f)
                {
                    Interrupt();
                }
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
                castLengthText.text = castTime.ToString("00.00").Replace(',', '.').Replace(':', '.').Replace(';', '.');
            }
        }
        else
        {
            if (interrupted)
            {
                if (castBar != null && castBarGroup.alpha == 1f)
                {
                    castBarGroup.alpha = 0.99f;
                    Utilities.FunctionTimer.Create(() => castBarGroup.LeanAlpha(0f, 0.5f), 2f, $"{characterState.characterName}_{this}_castBar_fade_out_if_interrupted", true);
                }
                if (castBarParty != null && castBarGroupParty.alpha == 1f)
                {
                    castBarGroupParty.alpha = 0f;
                    //Utilities.FunctionTimer.Create(() => castBarGroupParty.alpha = 0f, 2f, $"{this}_castBarParty_fade_out_if_interrupted", true);
                }
                Utilities.FunctionTimer.Create(() => ResetCastBar(), 2.5f, $"{characterState.characterName}_{this}_interrupted_status", true, true);
            }
            else
            {
                if (castBar != null && castBarGroup != null && castBarGroup.alpha == 1f)
                {
                    castBarGroup.alpha = 0.99f;
                    castBarGroup.LeanAlpha(0f, 0.5f);
                }
                if (castBarParty != null && castBarGroupParty != null && castBarGroupParty.alpha == 1f)
                {
                    castBarGroupParty.alpha = 0f;
                }
            }
        }

        if (autoAttack != null && hasTarget && autoAttackEnabled)
        {
            if (autoAttackTimer <= 0)
            {
                if (Utilities.RateLimiter(30))
                {
                    if (TryPerformAutoAction(autoAttack))
                    {
                        //Debug.Log($"Auto Attack performed! {autoAttack.data.actionName} {autoAttack}");
                        autoAttackTimer = autoAttack.data.recast;
                    }
                }
            }
            else if (autoAttackTimer > 0)
            {
                autoAttackTimer -= FightTimeline.deltaTime;
            }
        }
        else if (!autoAttackEnabled)
        {
            autoAttackTimer = autoAttack.data.recast;
        }

        // Try performing actions from the queue
        if (queuedAutoActions.Count > 0 && Utilities.RateLimiter(50))
        {
            // Peek at the next action without dequeuing it
            CharacterAction nextAction = queuedAutoActions.Peek();

            // Try to perform the action
            if (TryPerformAutoAction(nextAction))
            {
                // If the action succeeds, remove it from the queue
                queuedAutoActions.Dequeue();
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

    public void PerformAutoAction(CharacterAction autoAction)
    {
        if (!gameObject.activeSelf)
            return;

        // Try to perform the action immediately
        if (!TryPerformAutoAction(autoAction))
        {
            // If it fails, enqueue it for later
            //Debug.Log($"Failed to perform auto action {autoAction.data.actionName} {autoAction}!");
            queuedAutoActions.Enqueue(autoAction);
        }
    }

    public bool TryPerformAutoAction(CharacterAction autoAction)
    {
        if (!gameObject.activeSelf)
            return false;

        if (characterState.dead)
            return false;

        if (Time.timeScale <= 0f)
            return false;

        if (autoAction == null)
            return false;

        autoAction.OnPointerClick(null);

        if (lockActionsWhenCasting && isCasting)
            return false;

        if (autoAction.data.range > 0f && autoAction.data.isTargeted && (distanceToTarget > autoAction.data.range))
            return false;

        if (autoAction.isAvailable && !autoAction.isDisabled && !autoAction.isAnimationLocked && !autoAction.unavailable)
        {
            if (autoAction.data.actionType == CharacterActionData.ActionType.Auto)
            {
                CharacterState currentTarget = null;

                if (targetController != null && targetController.currentTarget != null)
                {
                    currentTarget = targetController.currentTarget.GetCharacterState();
                }
                if (currentTarget == null)
                {
                    currentTarget = characterState;
                }

                ActionInfo newActionInfo = new ActionInfo(autoAction, characterState, currentTarget);
                autoAction.ExecuteAction(newActionInfo);

                onCast.Invoke(new CastInfo(newActionInfo, instantCast, characterState.GetEffects()));
                if (animator != null && !string.IsNullOrEmpty(autoAction.data.animationName))
                {
                    animator.SetTrigger(autoAction.data.animationName);
                }

                return true;
            }
        }
        else if (characterState.canDoActions && !autoAction.isDisabled && !autoAction.isAnimationLocked && !autoAction.unavailable)
        {
            FailAction(autoAction, "Action not ready yet.");
        }
        else if (characterState.canDoActions && (autoAction.isDisabled || autoAction.unavailable) && !autoAction.isAnimationLocked)
        {
            FailAction(autoAction, "Action not available right now.");
        }
        else if (characterState.canDoActions && autoAction.isAnimationLocked)
        {
            FailAction(autoAction, "Action not finished and available yet.");
        }
        else
        {
            FailAction(autoAction, "Actions not available right now.");
        }

        return false;
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

        if (action.data.range > 0f && action.data.isTargeted && (distanceToTarget > action.data.range))
            return;

        interrupted = false;
        if (castBarElement != null)
            castBarElement.ChangeColors(false);

        if (action.isAvailable && !action.isDisabled && !action.isAnimationLocked && !action.unavailable)
        {
            if (action.data.cast <= 0f || (action.data.cast > 0f && instantCast))
            {
                Utilities.FunctionTimer.StopTimer($"{characterState.characterName}_{this}_castBar_fade_out_if_interrupted");
                //Utilities.FunctionTimer.StopTimer($"{this}_castBarParty_fade_out_if_interrupted");
                Utilities.FunctionTimer.StopTimer($"{characterState.characterName}_{this}_interrupted_status");
                
                CharacterState currentTarget = null;

                if (targetController != null && targetController.currentTarget != null)
                {
                    currentTarget = targetController.currentTarget.GetCharacterState();
                }
                if (currentTarget == null)
                {
                    currentTarget = characterState;
                }

                ActionInfo newActionInfo = new ActionInfo(action, characterState, currentTarget);
                action.ExecuteAction(newActionInfo);

                action.ActivateAnimationLock();
                if (action.data.rollsGcd)
                    action.ActivateCooldown();

                onCast.Invoke(new CastInfo(newActionInfo, instantCast, characterState.GetEffects()));
                if (animator != null)
                {
                    animator.SetBool("Casting", false);
                }

                if (action.data.cast > 0f && instantCast && instantCastEffect != null)
                {
                    characterState.RemoveEffect(instantCastEffect, false, instantCastEffect.uniqueTag, 1);
                }
                else if (action.data.cast > 0f && instantCast && instantCastEffect == null)
                {
                    instantCast = false;
                }
            }
            else
            {
                Utilities.FunctionTimer.StopTimer($"{characterState.characterName}_{this}_castBar_fade_out_if_interrupted");
                //Utilities.FunctionTimer.StopTimer($"{this}_castBarParty_fade_out_if_interrupted");
                Utilities.FunctionTimer.StopTimer($"{characterState.characterName}_{this}_interrupted_status");
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
                if (action.data.rollsGcd)
                    action.ActivateCooldown();

                CharacterState currentTarget = null;

                if (targetController != null && targetController.currentTarget != null)
                {
                    currentTarget = targetController.currentTarget.GetCharacterState();
                }
                if (currentTarget == null || !action.data.isTargeted)
                {
                    currentTarget = characterState;
                }

                ActionInfo newActionInfo = new ActionInfo(action, characterState, currentTarget);
                StartCoroutine(Cast(castTime, () => { action.ExecuteAction(newActionInfo); }));
                onCast.Invoke(new CastInfo(newActionInfo, instantCast, characterState.GetEffects()));

                if (animator != null && !string.IsNullOrEmpty(action.data.animationName) && action.data.playAnimationDirectly)
                {
                    animator.Play(action.data.animationName);
                }

                UpdateCharacterName();
                if (castBar != null)
                {
                    castBar.maxValue = action.data.cast;
                    castBar.value = 0f;
                }
                if (castLengthText != null)
                {
                    castLengthText.text = castTime.ToString("00.00").Replace(',', '.').Replace(':', '.').Replace(';', '.');
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
                    if (newActionInfo.target != null && showCastTargetLetter)
                    {
                        castNameTextParty.text = $"{Utilities.InsertSpaceBeforeCapitals(action.data.actionName)}<sprite=\"{newActionInfo.target.letterSpriteAsset}\" name=\"{newActionInfo.target.characterLetter}\">";
                    }
                    else if (newActionInfo.source != null && showCastTargetLetter)
                    {
                        castNameTextParty.text = $"{Utilities.InsertSpaceBeforeCapitals(action.data.actionName)}<sprite=\"{newActionInfo.source.letterSpriteAsset}\" name=\"{newActionInfo.source.characterLetter}\">";
                    }
                    else
                    {
                        castNameTextParty.text = Utilities.InsertSpaceBeforeCapitals(action.data.actionName);
                    }
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
        else if (characterState.canDoActions && !action.isDisabled && !action.isAnimationLocked && !action.unavailable)
        {
            FailAction(action, "Action not ready yet.");
        }
        else if (characterState.canDoActions && (action.isDisabled || action.unavailable) && !action.isAnimationLocked)
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
        UpdateCharacterName();
    }

    private IEnumerator Cast(float length, Action action)
    {
        yield return new WaitForSeconds(length);
        action.Invoke();
        isCasting = false;
        lastAction = null;
        if (animator != null)
        {
            animator.SetBool("Casting", false);
        }
        UpdateCharacterName();
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

    public void UpdateInstantCasts(List<StatusEffect> effects)
    {
        instantCast = false;

        for (int i = 0; i < effects.Count; i++)
        {
            if (effects[i].data.instantCasts)
            {
                instantCastEffect = effects[i];
                instantCast = true;
                break;
            }
        }
    }

    private void UpdateCharacterName()
    {
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
        characterState.UpdateCharacterName();
    }

    void OnDestroy()
    {
        if (characterState != null)
            characterState.onInstantCastsChanged.RemoveListener(UpdateInstantCasts);
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
