using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;
using NaughtyAttributes;
using dev.susybaka.raidsim.Core;
using dev.susybaka.raidsim.Characters;
using dev.susybaka.raidsim.StatusEffects;
using dev.susybaka.raidsim.Targeting;
using dev.susybaka.raidsim.UI;
using dev.susybaka.Shared;
using static dev.susybaka.raidsim.Core.GlobalData;

namespace dev.susybaka.raidsim.Actions
{
    public class ActionController : MonoBehaviour
    {
        Animator animator;
        CharacterState characterState;
        TargetController targetController;

        public Transform actionParent;
        public List<CharacterAction> actions = new List<CharacterAction>();
        public List<CharacterAction> autoActions = new List<CharacterAction>();
        private CharacterAction autoAttack;
        public bool loadAutomatically = true;
        public bool autoAttackEnabled = true;
        public bool instantCast = false;
        public bool isAnimationLocked = false;
        public bool isCasting = false;
        public bool isGroundTargeting = false;
        public bool waitInterruptedCasts = false;
        public bool lockActionsWhenCasting = true;
        public bool faceTargetWhenCasting = false;
        private bool previousCanDoActions;
        private bool previousIsCasting;
        public float distanceToTarget = 0f;
        public float castingRotationSpeed = 5f;

        [Header("Personal")]
        public bool useCastBar;
        public Slider castBar;
        public TextMeshProUGUI castNameText;
        public TextMeshProUGUI castLengthText;
        public CanvasGroup interruptText;
        public HudElement castBarElement;
        public CanvasGroup speechBubbleGroup;
        public float speechBubbleDuration = 2f;
        public TextMeshProUGUI speechBubbleText;
        public AudioSource speechBubbleAudio;

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

#if UNITY_EDITOR
        [Header("Editor")]
        public CharacterAction actionToPerform;
        [Button("Perform Action")]
        public void PerformDebugCharacterAction()
        {
            PerformAction(actionToPerform);
        }
#endif

        private StatusEffect instantCastEffect;
        private CharacterAction lastAction;
        public CharacterAction LastAction { get { return lastAction; } }
        private float castTime;
        public float CastTime { get { return castTime; } }
        private float lastCastTime;
        public float LastCastTime { get { return lastCastTime; } }
        private bool interrupted;
        public bool Interrupted { get { return interrupted; } }
        private bool hasTarget;
        private int rateLimit;
        private float autoAttackTimer;
        private Queue<CharacterAction> queuedAutoActions = new Queue<CharacterAction>();

        private int animatorParameterActionLocked = Animator.StringToHash("ActionLocked");
        private int animatorParameterCasting = Animator.StringToHash("Casting");
        private int animatorParameterCastFinishId = Animator.StringToHash("CastFinishId");

        private void Awake()
        {
            animator = GetComponentInChildren<Animator>();
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

            if (loadAutomatically && actionParent != null)
            {
                actions.Clear();
                autoActions.Clear();
                CharacterAction[] allActions = actionParent.GetComponentsInChildren<CharacterAction>();
                for (int i = 0; i < allActions.Length; i++)
                {
                    if (allActions[i].isAutoAction)
                    {
                        autoActions.Add(allActions[i]);
                    }
                    else
                    {
                        actions.Add(allActions[i]);
                    }
                }
            }

            if (characterState != null)
            {
                List<CharacterAction> invalidActions = new List<CharacterAction>();

                for (int i = 0; i < actions.Count; i++)
                {
                    if (actions[i] != null)
                    {
                        actions[i].Initialize(this);
                    }
                    else
                    {
                        invalidActions.Add(actions[i]);
                        Debug.LogWarning($"Found an invalid (null) CharacterAction (Index {i}) from the Character {characterState.characterName} ({gameObject.name})!\nIt has been automatically removed from the list of available actions for now, but to get rid of this warning permanently please remove it manually from the list in the editor!");
                    }
                }

                if (invalidActions.Count > 0)
                {
                    for (int i = 0; i < invalidActions.Count; i++)
                    {
                        actions.Remove(invalidActions[i]);
                    }
                }
            }
            if (characterState != null)
                previousCanDoActions = characterState.canDoActions.value;
            else
                previousCanDoActions = true;

            rateLimit = UnityEngine.Random.Range(15, 26);

            if (autoActions != null && autoActions.Count > 0)
            {
                autoAttack = autoActions[0];
                autoAttackTimer = autoAttack.data.recast;
            }
        }

        private void Update()
        {
            if (animator == null)
            {
                if (Utilities.RateLimiter(56))
                {
                    animator = GetComponentInChildren<Animator>();
                }
            }

            if (characterState == null)
                return;

            if (!gameObject.activeSelf || !gameObject.scene.isLoaded)
                return;

            if (Time.timeScale <= 0f)
                return;

            hasTarget = false;

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
                        actions[i].lastAction = lastAction?.data;
                    }
                }
                if (autoActions != null && autoActions.Count > 1)
                {
                    for (int i = 0; i < autoActions.Count; i++)
                    {
                        if (hasTarget)
                            autoActions[i].distanceToTarget = distanceToTarget;
                        autoActions[i].hasTarget = hasTarget;
                        autoActions[i].lastAction = lastAction?.data;
                    }
                }
                else if (autoActions.Count == 1 && autoAttack != null)
                {
                    if (hasTarget)
                        autoAttack.distanceToTarget = distanceToTarget;
                    autoAttack.hasTarget = hasTarget;
                    autoAttack.lastAction = lastAction?.data;
                }
            }

            if (previousCanDoActions != characterState.canDoActions.value || previousIsCasting != isCasting)
            {
                previousCanDoActions = characterState.canDoActions.value;
                previousIsCasting = isCasting;
                if (characterState.canDoActions.value && ((!isCasting && lockActionsWhenCasting) || (!lockActionsWhenCasting)))
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
                    // Handle rotation when casting
                    if (faceTargetWhenCasting && lastAction.data.isTargeted && targetController.currentTarget != null)
                    {
                        // Get the direction to the target but ignore the vertical component (Y-axis)
                        Vector3 directionToTarget = targetController.currentTarget.transform.position - transform.position;
                        directionToTarget.y = 0; // Ensure we only rotate on the Y-axis

                        // If there's some direction (the target is not directly above or below)
                        if (directionToTarget != Vector3.zero)
                        {
                            Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
                            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * castingRotationSpeed); // Smooth rotation, optional
                        }
                    }

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
                        Utilities.FunctionTimer.Create(this, () => castBarGroup.LeanAlpha(0f, 0.5f), 2f, $"{characterState.characterName}_{this}_castBar_fade_out_if_interrupted", true);
                    }
                    if (castBarParty != null && castBarGroupParty.alpha == 1f)
                    {
                        castBarGroupParty.alpha = 0f;
                        //Utilities.FunctionTimer.Create(this, () => castBarGroupParty.alpha = 0f, 2f, $"{this}_castBarParty_fade_out_if_interrupted", true);
                    }
                    Utilities.FunctionTimer.Create(this, () => ResetCastBar(), 2.5f, $"{characterState.characterName}_{this}_interrupted_status", true, true);
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
            else if (!autoAttackEnabled && autoAttack != null && autoAttack.data != null)
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

        public void ResetController()
        {
            StopAllCoroutines();
            castTime = 0f;
            lastCastTime = 0f;
            interrupted = false;
            isCasting = false;
            isGroundTargeting = false;
            lastAction = null;
            for (int i = 0; i < actions.Count; i++)
            {
                actions[i].gameObject.SetActive(true);
                actions[i].ResetAction();
            }
            Utilities.FunctionTimer.StopTimer($"{characterState.characterName}_{this}_castBar_fade_out_if_interrupted");
            Utilities.FunctionTimer.StopTimer($"{characterState.characterName}_{this}_interrupted_status");
            ResetCastBar();
            if (castBarGroup != null)
                castBarGroup.alpha = 0f;
            if (castBarGroupParty != null)
                castBarGroupParty.alpha = 0f;
        }

        public void SetAnimator(Animator animator)
        {
            this.animator = animator;
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

            if (autoAction.unavailable)
                return false;

            autoAction.OnPointerClick(null);

            if (lockActionsWhenCasting && isCasting)
                return false;

            if (autoAction.data.range > 0f && autoAction.data.isTargeted && (distanceToTarget > autoAction.data.range))
                return false;

            if (autoAction.data.isTargeted && !hasTarget)
                return false;

            if (autoAction.data.isTargeted && targetController.currentTarget != null && !autoAction.data.targetGroups.Contains(targetController.currentTarget.Group))
                return false;

            if (autoAction.data.charges > 1 && autoAction.chargesLeft < 1)
                return false;

            if (autoAction.data.hasMovement && (characterState.bound.value || characterState.uncontrollable.value))
                return false;

            bool autoActionCondition = false;

            if (animator != null)
            {
                autoActionCondition = autoAction.isAvailable && !autoAction.isDisabled && !autoAction.isAnimationLocked && !autoAction.unavailable && !animator.GetBool(animatorParameterActionLocked);
            }
            else
            {
                autoActionCondition = autoAction.isAvailable && !autoAction.isDisabled && !autoAction.isAnimationLocked && !autoAction.unavailable;
            }

            if (autoActionCondition)
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

                    if (currentTarget.targetController != null && currentTarget.targetController.self != null && autoAction.data.targetGroups.Length > 0 && autoAction.data.isTargeted)
                    {
                        if (!autoAction.data.targetGroups.Contains(currentTarget.targetController.self.Group))
                        {
                            return false;
                        }
                    }

                    autoAction.data.damage = new Damage(autoAction.data.damage, characterState);

                    autoAction.chargesLeft--;

                    ActionInfo newActionInfo = new ActionInfo(autoAction, characterState, currentTarget);
                    autoAction.ExecuteAction(newActionInfo);

                    onCast.Invoke(new CastInfo(newActionInfo, instantCast, characterState.GetEffects()));

                    UpdateSpeechBubble(autoAction);

                    if (animator != null && !string.IsNullOrEmpty(autoAction.data.animationName) && !autoAction.data.playAnimationDirectly)
                    {
                        if (autoAction.data.animationDelay > 0f)
                        {
                            Utilities.FunctionTimer.Create(this, () => animator.SetTrigger(autoAction.data.animationName), autoAction.data.animationDelay, $"{autoAction.data.actionName}_animation_{autoAction.data.animationName}_delay");
                        }
                        else
                        {
                            animator.SetTrigger(autoAction.data.animationName);
                        }
                    }
                    else if (animator != null && !string.IsNullOrEmpty(autoAction.data.animationName) && autoAction.data.playAnimationDirectly)
                    {
                        if (autoAction.data.animationDelay > 0f)
                        {
                            Utilities.FunctionTimer.Create(this, () => animator.CrossFadeInFixedTime(autoAction.data.animationName, 0.2f), autoAction.data.animationDelay, $"{autoAction.data.actionName}_animation_{autoAction.data.animationName}_delay");
                        }
                        else
                        {
                            animator.CrossFadeInFixedTime(autoAction.data.animationName, 0.2f);
                        }
                    }
                    if (animator != null && autoAction.data.onAnimationFinishId >= 0)
                    {
                        if (autoAction.data.animationDelay > 0f)
                        {
                            Utilities.FunctionTimer.Create(this, () => animator.SetInteger(animatorParameterCastFinishId, autoAction.data.onAnimationFinishId), autoAction.data.animationDelay, $"{autoAction.data.actionName}_animation_{autoAction.data.animationName}_delay");
                        }
                        else
                        {
                            animator.SetInteger(animatorParameterCastFinishId, autoAction.data.onAnimationFinishId);
                        }
                    }
                    else if (animator != null)
                    {
                        if (autoAction.data.animationDelay > 0f)
                        {
                            Utilities.FunctionTimer.Create(this, () => animator.SetInteger(animatorParameterCastFinishId, 0), autoAction.data.animationDelay, $"{autoAction.data.actionName}_animation_{autoAction.data.animationName}_delay");
                        }
                        else
                        {
                            animator.SetInteger(animatorParameterCastFinishId, 0);
                        }
                    }

                    return true;
                }
            }
            else if (characterState.canDoActions.value && !autoAction.isDisabled && !autoAction.isAnimationLocked && !autoAction.unavailable)
            {
                FailAction(autoAction, "Action not ready yet.");
            }
            else if (characterState.canDoActions.value && (autoAction.isDisabled || autoAction.unavailable) && !autoAction.isAnimationLocked)
            {
                FailAction(autoAction, "Action not available right now.");
            }
            else if (characterState.canDoActions.value && autoAction.isAnimationLocked)
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

            PerformActionInternal(action, false);
        }

        public void PerformActionUnrestricted(string name)
        {
            for (int i = 0; i < actions.Count; i++)
            {
                if (actions[i].data.actionName == name)
                {
                    PerformActionUnrestricted(actions[i]);
                }
            }
        }

        public void PerformActionUnrestricted(CharacterAction action)
        {
            PerformActionInternal(action, false);
        }

        public void PerformActionHidden(string name)
        {
            for (int i = 0; i < actions.Count; i++)
            {
                if (actions[i].data.actionName == name)
                {
                    PerformActionHidden(actions[i]);
                }
            }
        }

        public void PerformActionHidden(CharacterAction action)
        {
            PerformActionInternal(action, true);
        }

        private void PerformActionInternal(CharacterAction action, bool hidden)
        {
            //Debug.Log($"[ActionController ({gameObject.name})] Performing action {action.data.actionName} ({action}), hidden {hidden}");

            if (action == null)
                return;

            if (action.unavailable)
                return;

            //Debug.Log($"[ActionController ({gameObject.name})] Action {action.data.actionName} passed 1. checks");

            action.OnPointerClick(null);

            if (lockActionsWhenCasting && isCasting && !hidden)
                return;

            if (action.data.range > 0f && action.data.isTargeted && (distanceToTarget > action.data.range) && !hidden)
                return;

            //Debug.Log($"[ActionController ({gameObject.name})] Action {action.data.actionName} passed 2. checks");

            if (action.data.isTargeted && !hasTarget && !hidden)
                return;

            if (action.data.isTargeted && targetController.currentTarget != null && !action.data.targetGroups.Contains(targetController.currentTarget.Group) && !hidden)
                return;

            //Debug.Log($"[ActionController ({gameObject.name})] Action {action.data.actionName} passed 3. checks");

            if (action.data.charges > 1 && action.chargesLeft < 1 && !hidden)
                return;

            if (action.data.hasMovement && (characterState.bound.value || characterState.uncontrollable.value) && !hidden)
                return;

            //Debug.Log($"[ActionController ({gameObject.name})] Action {action.data.actionName} passed 4. checks");

            interrupted = false;
            if (castBarElement != null)
                castBarElement.ChangeColors(false);

            bool actionCondition = false;

            if (animator != null)
            {
                actionCondition = ((action.isAvailable && !action.isDisabled && !action.isAnimationLocked && !action.unavailable && !animator.GetBool(animatorParameterActionLocked)) || hidden);
            }
            else
            {
                actionCondition = ((action.isAvailable && !action.isDisabled && !action.isAnimationLocked && !action.unavailable) || hidden);
            }

            if (actionCondition)
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

                    action.data.damage = new Damage(action.data.damage, characterState);
                    if (!action.data.isGroundTargeted && action.data.recast > 0f && !hidden)
                        action.chargesLeft--;
                    lastAction = action;

                    ActionInfo newActionInfo = new ActionInfo(action, characterState, currentTarget);
                    action.onCast.Invoke(newActionInfo);
                    action.ExecuteAction(newActionInfo);

                    if (action.data.animationLock > 0f && !action.data.isGroundTargeted)
                        action.ActivateAnimationLock();
                    if (action.data.rollsGcd && action.data.recast > 0f && !action.data.isGroundTargeted)
                        action.ActivateCooldown();

                    onCast.Invoke(new CastInfo(newActionInfo, instantCast, characterState.GetEffects()));

                    if (animator != null)
                    {
                        animator.SetBool(animatorParameterCasting, false);
                    }

                    if (!action.data.isGroundTargeted)
                        HandleAnimation(action);

                    UpdateSpeechBubble(action);

                    if (action.data.cast > 0f && instantCast && instantCastEffect != null && !action.data.isGroundTargeted)
                    {
                        characterState.RemoveEffect(instantCastEffect, false, characterState, instantCastEffect.uniqueTag, 1);
                    }
                    else if (action.data.cast > 0f && instantCast && instantCastEffect == null && !action.data.isGroundTargeted)
                    {
                        instantCast = false;
                    }

                    // Handle rotation when casting
                    if (faceTargetWhenCasting && action.data.isTargeted && targetController.currentTarget != null && !action.data.isGroundTargeted)
                    {
                        // Get the direction to the target but ignore the vertical component (Y-axis)
                        Vector3 directionToTarget = targetController.currentTarget.transform.position - transform.position;
                        directionToTarget.y = 0; // Ensure we only rotate on the Y-axis

                        // If there's some direction (the target is not directly above or below)
                        if (directionToTarget != Vector3.zero)
                        {
                            Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
                            transform.rotation = targetRotation;//Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * castingRotationSpeed); // Smooth rotation, optional
                        }
                    }

                    if (action.data.isGroundTargeted)
                    {
                        isGroundTargeting = true;
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

                    if (action.data.animationLock > 0f && !action.data.isGroundTargeted)
                        action.ActivateAnimationLock();
                    if (action.data.rollsGcd && action.data.recast > 0f && !action.data.isGroundTargeted)
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

                    action.data.damage = new Damage(action.data.damage, characterState);

                    ActionInfo newActionInfo = new ActionInfo(action, characterState, currentTarget);
                    action.onCast.Invoke(newActionInfo);
                    StartCoroutine(IE_Cast(castTime, () => { if (!action.data.isGroundTargeted && action.data.recast > 0f && !hidden) { action.chargesLeft--; } action.ExecuteAction(newActionInfo); if (action.data.playAnimationOnFinish) { HandleAnimation(action); } }));
                    onCast.Invoke(new CastInfo(newActionInfo, instantCast, characterState.GetEffects()));

                    if (!action.data.playAnimationOnFinish && !action.data.isGroundTargeted)
                        HandleAnimation(action);

                    UpdateCharacterName();
                    UpdateUserInterface(newActionInfo);
                    UpdateSpeechBubble(action);

                    if (animator != null && !action.data.isGroundTargeted)
                    {
                        if (action.data.playCastingAnimationDirectly)
                        {
                            animator.CrossFadeInFixedTime(action.data.castingAnimationName, 0.2f);
                        }
                        else
                        {
                            animator.SetBool(animatorParameterCasting, true);
                        }
                    }

                    if (action.data.isGroundTargeted)
                    {
                        isGroundTargeting = true;
                    }
                }
            }
            else if (characterState.canDoActions.value && !action.isDisabled && !action.isAnimationLocked && !action.unavailable)
            {
                FailAction(action, "Action not ready yet.");
            }
            else if (characterState.canDoActions.value && (action.isDisabled || action.unavailable) && !action.isAnimationLocked)
            {
                FailAction(action, "Action not available right now.");
            }
            else if (characterState.canDoActions.value && action.isAnimationLocked)
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
            isGroundTargeting = false;
            if (interruptText != null)
            {
                interruptText.LeanAlpha(1f, 0.5f);
            }
            if (lastAction != null)
            {
                if (lastAction.data.animationLock > 0f)
                    lastAction.ResetAnimationLock();
                if (lastAction.data.recast > 0f)
                    lastAction.ResetCooldown();
                lastAction.onInterrupt.Invoke(new ActionInfo(lastAction, characterState, null));
                lastAction = null;
            }
            if (animator != null)
            {
                animator.SetBool(animatorParameterCasting, false);
            }
            if (castBarElement != null)
                castBarElement.ChangeColors(true);
            StopAllCoroutines();
            UpdateCharacterName();
        }

        private IEnumerator IE_Cast(float length, Action action)
        {
            yield return new WaitForSeconds(length);
            action.Invoke();
            isCasting = false;
            lastAction = null;
            if (animator != null)
            {
                animator.SetBool(animatorParameterCasting, false);
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

        private void HandleAnimation(CharacterAction action)
        {
            if (animator != null && !string.IsNullOrEmpty(action.data.animationName))
            {
                if (!action.data.playAnimationDirectly)
                {
                    if (action.data.animationDelay > 0f)
                    {
                        Utilities.FunctionTimer.Create(this, () => animator.SetTrigger(action.data.animationName), action.data.animationDelay, $"{action.data.actionName}_animation_{action.data.animationName}_delay");
                    }
                    else
                    {
                        animator.SetTrigger(action.data.animationName);
                    }
                }
                else
                {
                    if (action.data.animationDelay > 0f)
                    {
                        Utilities.FunctionTimer.Create(this, () => animator.CrossFadeInFixedTime(action.data.animationName, 0.2f), action.data.animationDelay, $"{action.data.actionName}_animation_{action.data.animationName}_delay");
                    }
                    else
                    {
                        animator.CrossFadeInFixedTime(action.data.animationName, 0.2f);
                    }
                }
                if (action.data.onAnimationFinishId >= 0)
                {
                    if (action.data.animationDelay > 0f)
                    {
                        Utilities.FunctionTimer.Create(this, () => animator.SetInteger(animatorParameterCastFinishId, action.data.onAnimationFinishId), action.data.animationDelay, $"{action.data.actionName}_animation_{action.data.animationName}_delay");
                    }
                    else
                    {
                        animator.SetInteger(animatorParameterCastFinishId, action.data.onAnimationFinishId);
                    }
                }
                else
                {
                    if (action.data.animationDelay > 0f)
                    {
                        Utilities.FunctionTimer.Create(this, () => animator.SetInteger(animatorParameterCastFinishId, 0), action.data.animationDelay, $"{action.data.actionName}_animation_{action.data.animationName}_delay");
                    }
                    else
                    {
                        animator.SetInteger(animatorParameterCastFinishId, 0);
                    }
                }
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

        private void UpdateUserInterface(ActionInfo actionInfo)
        {
            if (castBar != null)
            {
                castBar.maxValue = actionInfo.action.data.cast;
                castBar.value = 0f;
            }
            if (castLengthText != null)
            {
                castLengthText.text = castTime.ToString("00.00").Replace(',', '.').Replace(':', '.').Replace(';', '.');
            }
            if (castNameText != null)
            {
                castNameText.text = actionInfo.action.data.GetActionName();
            }
            if (castBarParty != null)
            {
                castBarParty.maxValue = actionInfo.action.data.cast;
                castBarParty.value = 0f;
            }
            if (castNameTextParty != null)
            {
                if (actionInfo.target != null && showCastTargetLetter)
                {
                    castNameTextParty.text = $"{actionInfo.action.data.GetActionName()}<sprite=\"{actionInfo.target.letterSpriteAsset}\" name=\"{actionInfo.target.characterLetter}\">";
                }
                else if (actionInfo.source != null && showCastTargetLetter)
                {
                    castNameTextParty.text = $"{actionInfo.action.data.GetActionName()}<sprite=\"{actionInfo.source.letterSpriteAsset}\" name=\"{actionInfo.source.characterLetter}\">";
                }
                else
                {
                    castNameTextParty.text = actionInfo.action.data.GetActionName();
                }
            }
            if (interruptText != null)
            {
                interruptText.alpha = 0f;
            }
        }

        private void UpdateSpeechBubble(CharacterAction action)
        {
            if (!string.IsNullOrEmpty(action.data.speech))
            {
                if (speechBubbleText != null)
                {
                    speechBubbleText.text = action.data.speech;
                }
                if (speechBubbleGroup != null)
                {
                    speechBubbleGroup.LeanAlpha(1f, 0.25f);
                    Utilities.FunctionTimer.Create(this, () => speechBubbleGroup.LeanAlpha(0f, 0.25f), speechBubbleDuration, $"{characterState.characterName}_{this}_speech_bubble_fade_out", true);
                }
            }
            if (action.data.speechAudio != null)
            {
                if (speechBubbleAudio != null)
                {
                    if (FightTimeline.Instance != null && FightTimeline.Instance.jon && action.data.jonSpeechAudio != null)
                    {
                        speechBubbleAudio.clip = action.data.jonSpeechAudio;
                    }
                    else if (action.data.speechAudio != null)
                    {
                        speechBubbleAudio.clip = action.data.speechAudio;
                    }
                    if (speechBubbleAudio.clip != null)
                        speechBubbleAudio.Play();
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

        private void OnDestroy()
        {
            if (characterState != null)
                characterState.onInstantCastsChanged.RemoveListener(UpdateInstantCasts);
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
}