// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using dev.susybaka.raidsim.Characters;
using dev.susybaka.raidsim.Core;
using dev.susybaka.raidsim.StatusEffects;
using dev.susybaka.raidsim.Targeting;
using dev.susybaka.raidsim.UI;
using dev.susybaka.Shared;
using dev.susybaka.Shared.Audio;
using NaughtyAttributes;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static System.Collections.Specialized.BitVector32;
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
#if UNITY_EDITOR
        public List<CharacterAction> _actionQueue = new List<CharacterAction>();
#endif
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

        [Header("Audio")]
        public bool playAudio = false;
        [Range(0f, 1f)] public float audioVolume = 1f;
        private Transform audioParent;

#if UNITY_EDITOR
        [Header("Editor")]
        public CharacterAction actionToPerform;
        [Button("Perform Action")]
        public void PerformDebugCharacterAction()
        {
            PerformAction(actionToPerform);
        }
        [Button("Reset All Actions")]
        public void ResetAllActions()
        {
            ResetController();
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
        private Queue<CharacterAction> actionQueue = new Queue<CharacterAction>();

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
                autoAttackTimer = autoAttack.Data.recast;
            }
            
            audioParent = transform.Find("Audio");

            if (audioParent != null)
                speechBubbleAudio = audioParent.GetComponentInChildren<AudioSource>(true);
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
                        actions[i].lastAction = lastAction?.Data;
                    }
                }
                if (autoActions != null && autoActions.Count > 1)
                {
                    for (int i = 0; i < autoActions.Count; i++)
                    {
                        if (hasTarget)
                            autoActions[i].distanceToTarget = distanceToTarget;
                        autoActions[i].hasTarget = hasTarget;
                        autoActions[i].lastAction = lastAction?.Data;
                    }
                }
                else if (autoActions.Count == 1 && autoAttack != null)
                {
                    if (hasTarget)
                        autoAttack.distanceToTarget = distanceToTarget;
                    autoAttack.hasTarget = hasTarget;
                    autoAttack.lastAction = lastAction?.Data;
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
                    if (faceTargetWhenCasting && lastAction.Data.isTargeted && targetController.currentTarget != null)
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

                    if ((!characterState.still || characterState.dead || (lastAction.Data.range > 0f && lastAction.Data.isTargeted && (distanceToTarget > lastAction.Data.range))) && castTime > 0.5f)
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
                        if (!lastAction.Data.canBeSlideCast)
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
                            autoAttackTimer = autoAttack.Data.recast;
                        }
                    }
                }
                else if (autoAttackTimer > 0)
                {
                    autoAttackTimer -= FightTimeline.deltaTime;
                }
            }
            else if (!autoAttackEnabled && autoAttack != null && autoAttack.Data != null)
            {
                autoAttackTimer = autoAttack.Data.recast;
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
            actionQueue.Clear();
#if UNITY_EDITOR
            _actionQueue.Clear();
#endif
        }

        public void SetAnimator(Animator animator)
        {
            this.animator = animator;
        }

        public bool TryGetAction(CharacterActionData actionData, out CharacterAction action, StringComparison comparison = StringComparison.Ordinal)
        {
            return TryGetAction(actionData.actionName, out action, comparison);
        }

        public bool TryGetAction(string name, out CharacterAction action, StringComparison comparison = StringComparison.Ordinal)
        {
            for (int i = 0; i < actions.Count; i++)
            {
                if (name.Equals(actions[i].Data.actionName, comparison))
                {
                    action = actions[i];
                    return true;
                }
            }
            action = null;
            return false;
        }

        public bool HasAction(CharacterActionData actionData, StringComparison comparison = StringComparison.Ordinal)
        {
            return HasAction(actionData.actionName, comparison);
        }

        public bool HasAction(string name, StringComparison comparison = StringComparison.Ordinal)
        {
            for (int i = 0; i < actions.Count; i++)
            {
                if (name.Equals(actions[i].Data.actionName, comparison))
                {
                    return true;
                }
            }
            return false;
        }

        public bool HasAction(CharacterAction action, StringComparison comparison = StringComparison.Ordinal)
        {
            for (int i = 0; i < actions.Count; i++)
            {
                if (action.Data.actionName.Equals(actions[i].Data.actionName, comparison))
                {
                    action = actions[i];
                }
            }
            return false;
        }

        public void PerformAutoAction(CharacterAction autoAction)
        {
            if (!gameObject.activeSelf)
                return;

            // Try to perform the action immediately
            if (!TryPerformAutoAction(autoAction))
            {
                // If it fails, enqueue it for later
                //Debug.Log($"Failed to perform auto action {autoAction.Data.actionName} {autoAction}!");
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

            if (autoAction.Data.range > 0f && autoAction.Data.isTargeted && (distanceToTarget > autoAction.Data.range))
                return false;

            if (autoAction.Data.isTargeted && !hasTarget)
                return false;

            if (autoAction.Data.isTargeted && targetController.currentTarget != null && !autoAction.Data.targetGroups.Contains(targetController.currentTarget.Group))
                return false;

            if (autoAction.Data.charges > 1 && autoAction.chargesLeft < 1)
                return false;

            if (autoAction.Data.hasMovement && (characterState.bound.value || characterState.uncontrollable.value))
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
                if (autoAction.Data.actionType == CharacterActionData.ActionType.Auto)
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

                    if (currentTarget.targetController != null && currentTarget.targetController.self != null && autoAction.Data.targetGroups.Length > 0 && autoAction.Data.isTargeted)
                    {
                        if (!autoAction.Data.targetGroups.Contains(currentTarget.targetController.self.Group))
                        {
                            return false;
                        }
                    }

                    autoAction.Data.damage = new Damage(autoAction.Data.damage, characterState);

                    autoAction.chargesLeft--;

                    ActionInfo newActionInfo = new ActionInfo(autoAction, characterState, currentTarget);
                    autoAction.ExecuteAction(newActionInfo);

                    onCast.Invoke(new CastInfo(newActionInfo, instantCast, characterState.GetEffects()));

                    HandleSpeech(autoAction);
                    HandleActionAudio(autoAction, false);
                    HandleActionAudio(autoAction, true);

                    if (animator != null && !string.IsNullOrEmpty(autoAction.Data.animationName) && !autoAction.Data.playAnimationDirectly)
                    {
                        if (autoAction.Data.animationDelay > 0f)
                        {
                            Utilities.FunctionTimer.Create(this, () => animator.SetTrigger(autoAction.Data.animationName), autoAction.Data.animationDelay, $"{autoAction.Data.actionName}_animation_{autoAction.Data.animationName}_delay");
                        }
                        else
                        {
                            animator.SetTrigger(autoAction.Data.animationName);
                        }
                    }
                    else if (animator != null && !string.IsNullOrEmpty(autoAction.Data.animationName) && autoAction.Data.playAnimationDirectly)
                    {
                        if (autoAction.Data.animationDelay > 0f)
                        {
                            Utilities.FunctionTimer.Create(this, () => animator.CrossFadeInFixedTime(autoAction.Data.animationName, autoAction.Data.animationCrossFade), autoAction.Data.animationDelay, $"{autoAction.Data.actionName}_animation_{autoAction.Data.animationName}_delay");
                        }
                        else
                        {
                            animator.CrossFadeInFixedTime(autoAction.Data.animationName, autoAction.Data.animationCrossFade);
                        }
                    }
                    if (animator != null && autoAction.Data.onAnimationFinishId >= 0)
                    {
                        if (autoAction.Data.animationDelay > 0f)
                        {
                            Utilities.FunctionTimer.Create(this, () => animator.SetInteger(animatorParameterCastFinishId, autoAction.Data.onAnimationFinishId), autoAction.Data.animationDelay, $"{autoAction.Data.actionName}_animation_{autoAction.Data.animationName}_delay");
                        }
                        else
                        {
                            animator.SetInteger(animatorParameterCastFinishId, autoAction.Data.onAnimationFinishId);
                        }
                    }
                    else if (animator != null)
                    {
                        if (autoAction.Data.animationDelay > 0f)
                        {
                            Utilities.FunctionTimer.Create(this, () => animator.SetInteger(animatorParameterCastFinishId, 0), autoAction.Data.animationDelay, $"{autoAction.Data.actionName}_animation_{autoAction.Data.animationName}_delay");
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

        public void QueueAction(string name)
        {
            if (!gameObject.activeSelf)
                return;

            for (int i = 0; i < actions.Count; i++)
            {
                if (actions[i].Data.actionName == name)
                {
                    QueueAction(actions[i]);
                }
            }
        }

        public void QueueAction(CharacterAction action)
        {
            if (!gameObject.activeSelf)
                return;

#if UNITY_EDITOR
            _actionQueue.Add(action);
#endif
            actionQueue.Enqueue(action);
        }

        public void ClearActionQueue()
        {
            if (!gameObject.activeSelf)
                return;

#if UNITY_EDITOR
            _actionQueue.Clear();
#endif
            actionQueue.Clear();
        }

        public void PerformQueuedAction()
        {
            PerformQueuedActionInternal(false, false);
        }

        public void PerformQueuedActionHidden()
        {
            PerformQueuedActionInternal(false, true);
        }

        public void PerformQueuedActionUnrestricted()
        {
            PerformQueuedActionInternal(true, false);
        }

        private void PerformQueuedActionInternal(bool unrestricted, bool hidden)
        {
            if (!gameObject.activeSelf)
                return;

            if (characterState.dead)
                return;

            if (Time.timeScale <= 0f)
                return;

            if (actionQueue == null || actionQueue.Count < 1)
                return;

            CharacterAction action = actionQueue.Dequeue();

#if UNITY_EDITOR
            for (int i = 0; i < _actionQueue.Count; i++)
            {
                if (_actionQueue[i] == action)
                {
                    _actionQueue.RemoveAt(i);
                    break;
                }
            }
#endif

            if (action == null)
                return;

            if (!unrestricted && !hidden)
                PerformAction(action);
            else if (hidden && !unrestricted)
                PerformActionHidden(action);
            else if (unrestricted && !hidden)
                PerformActionUnrestricted(action);
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
                if (actions[i].Data.actionName == name)
                {
                    PerformAction(actions[i]);
                    break;
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
                if (actions[i].Data.actionName == name)
                {
                    PerformActionUnrestricted(actions[i]);
                    break;
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
                if (actions[i].Data.actionName == name)
                {
                    PerformActionHidden(actions[i]);
                    break;
                }
            }
        }

        public void PerformActionHidden(CharacterAction action)
        {
            PerformActionInternal(action, true);
        }

        private void PerformActionInternal(CharacterAction action, bool hidden)
        {
            //Debug.Log($"[ActionController ({gameObject.name})] Performing action {action.Data.actionName} ({action}), hidden {hidden}");

            if (action == null)
                return;

            if (action.unavailable)
                return;

            //Debug.Log($"[ActionController ({gameObject.name})] Action {action.Data.actionName} passed 1. checks");

            action.OnPointerClick(null);

            if (lockActionsWhenCasting && isCasting && !hidden)
                return;

            if ((targetController.currentTarget != null && action.Data.targetGroups.Contains(targetController.self.Group)) && action.Data.range > 0f && action.Data.isTargeted && (distanceToTarget > action.Data.range) && !hidden)
                return;

            //Debug.Log($"[ActionController ({gameObject.name})] Action {action.Data.actionName} passed 2. checks");

            if ((targetController.currentTarget != null && action.Data.targetGroups.Contains(targetController.self.Group)) && action.Data.isTargeted && !hasTarget && !hidden)
                return;

            if (action.Data.isTargeted && targetController.currentTarget != null && !action.Data.targetGroups.Contains(targetController.currentTarget.Group) && !hidden)
                return;

            //Debug.Log($"[ActionController ({gameObject.name})] Action {action.Data.actionName} passed 3. checks");

            if (action.Data.charges > 1 && action.chargesLeft < 1 && !hidden)
                return;

            if (action.Data.hasMovement && (characterState.bound.value || characterState.uncontrollable.value) && !hidden)
                return;

            //Debug.Log($"[ActionController ({gameObject.name})] Action {action.Data.actionName} passed 4. checks");

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
                // Handle target of the cast
                CharacterState currentTarget = null;

                if (targetController != null && targetController.currentTarget != null)
                {
                    currentTarget = targetController.currentTarget.GetCharacterState();
                }
                if (currentTarget == null)
                {
                    currentTarget = characterState;
                }

                if (currentTarget.targetController != null && currentTarget.targetController.self != null && action.Data.targetGroups.Length > 0 && action.Data.isTargeted)
                {
                    if (!action.Data.targetGroups.Contains(currentTarget.targetController.self.Group))
                    {
                        return;
                    }
                }
                // ---

                if (action.Data.cast <= 0f || (action.Data.cast > 0f && instantCast))
                {
                    Utilities.FunctionTimer.StopTimer($"{characterState.characterName}_{this}_castBar_fade_out_if_interrupted");
                    //Utilities.FunctionTimer.StopTimer($"{this}_castBarParty_fade_out_if_interrupted");
                    Utilities.FunctionTimer.StopTimer($"{characterState.characterName}_{this}_interrupted_status");

                    /*CharacterState currentTarget = null;

                    if (targetController != null && targetController.currentTarget != null)
                    {
                        currentTarget = targetController.currentTarget.GetCharacterState();
                    }
                    if (currentTarget == null)
                    {
                        currentTarget = characterState;
                    } */

                    action.Data.damage = new Damage(action.Data.damage, characterState);
                    if (!action.Data.isGroundTargeted && action.Data.recast > 0f && !hidden)
                        action.chargesLeft--;
                    lastAction = action;

                    ActionInfo newActionInfo = new ActionInfo(action, characterState, currentTarget);
                    action.onCast.Invoke(newActionInfo);
                    action.ExecuteAction(newActionInfo);

                    if (action.Data.animationLock > 0f && !action.Data.isGroundTargeted)
                        action.ActivateAnimationLock();
                    if (action.Data.rollsGcd && action.Data.recast > 0f && !action.Data.isGroundTargeted)
                        action.ActivateCooldown();

                    onCast.Invoke(new CastInfo(newActionInfo, instantCast, characterState.GetEffects()));

                    if (animator != null)
                    {
                        animator.SetBool(animatorParameterCasting, false);
                    }

                    if (!action.Data.isGroundTargeted)
                        HandleAnimation(action);

                    HandleSpeech(action);
                    HandleActionAudio(action, false);
                    HandleActionAudio(action, true);

                    if (action.Data.cast > 0f && instantCast && instantCastEffect != null && !action.Data.isGroundTargeted)
                    {
                        characterState.RemoveEffect(instantCastEffect, false, characterState, instantCastEffect.uniqueTag, 1);
                    }
                    else if (action.Data.cast > 0f && instantCast && instantCastEffect == null && !action.Data.isGroundTargeted)
                    {
                        instantCast = false;
                    }

                    // Handle rotation when casting
                    if (faceTargetWhenCasting && action.Data.isTargeted && targetController.currentTarget != null && !action.Data.isGroundTargeted)
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

                    if (action.Data.isGroundTargeted)
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
                    castTime = action.Data.cast;
                    lastCastTime = castTime;

                    if (action.Data.animationLock > 0f && !action.Data.isGroundTargeted)
                        action.ActivateAnimationLock();
                    if (action.Data.rollsGcd && action.Data.recast > 0f && !action.Data.isGroundTargeted)
                        action.ActivateCooldown();

                    /*CharacterState currentTarget = null;

                    if (targetController != null && targetController.currentTarget != null)
                    {
                        currentTarget = targetController.currentTarget.GetCharacterState();
                    }
                    if (currentTarget == null || !action.Data.isTargeted)
                    {
                        currentTarget = characterState;
                    }*/

                    action.Data.damage = new Damage(action.Data.damage, characterState);

                    ActionInfo newActionInfo = new ActionInfo(action, characterState, currentTarget);
                    action.onCast.Invoke(newActionInfo);
                    StartCoroutine(IE_Cast(castTime, () => { if (!action.Data.isGroundTargeted && action.Data.recast > 0f && !hidden) { action.chargesLeft--; } action.ExecuteAction(newActionInfo); if (action.Data.playAnimationOnFinish) { HandleAnimation(action); } HandleActionAudio(action, true); }));
                    onCast.Invoke(new CastInfo(newActionInfo, instantCast, characterState.GetEffects()));

                    if (!action.Data.playAnimationOnFinish && !action.Data.isGroundTargeted)
                        HandleAnimation(action);

                    UpdateCharacterName();
                    UpdateUserInterface(newActionInfo);
                    HandleSpeech(action);
                    HandleActionAudio(action, false);

                    if (animator != null && !action.Data.isGroundTargeted)
                    {
                        if (action.Data.playCastingAnimationDirectly)
                        {
                            animator.CrossFadeInFixedTime(action.Data.castingAnimationName, action.Data.castingAnimationCrossFade);
                        }
                        else
                        {
                            animator.SetBool(animatorParameterCasting, true);
                        }
                    }

                    if (action.Data.isGroundTargeted)
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
                if (lastAction.Data.animationLock > 0f)
                    lastAction.ResetAnimationLock();
                if (lastAction.Data.recast > 0f)
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
            if (animator != null && !string.IsNullOrEmpty(action.Data.animationName))
            {
                if (!action.Data.playAnimationDirectly)
                {
                    if (action.Data.animationDelay > 0f)
                    {
                        Utilities.FunctionTimer.Create(this, () => animator.SetTrigger(action.Data.animationName), action.Data.animationDelay, $"{action.Data.actionName}_animation_{action.Data.animationName}_delay");
                    }
                    else
                    {
                        animator.SetTrigger(action.Data.animationName);
                    }
                }
                else
                {
                    if (action.Data.animationDelay > 0f)
                    {
                        Utilities.FunctionTimer.Create(this, () => animator.CrossFadeInFixedTime(action.Data.animationName, action.Data.animationCrossFade), action.Data.animationDelay, $"{action.Data.actionName}_animation_{action.Data.animationName}_delay");
                    }
                    else
                    {
                        animator.CrossFadeInFixedTime(action.Data.animationName, action.Data.animationCrossFade);
                    }
                }
                if (action.Data.onAnimationFinishId >= 0)
                {
                    if (action.Data.animationDelay > 0f)
                    {
                        Utilities.FunctionTimer.Create(this, () => animator.SetInteger(animatorParameterCastFinishId, action.Data.onAnimationFinishId), action.Data.animationDelay, $"{action.Data.actionName}_animation_{action.Data.animationName}_delay");
                    }
                    else
                    {
                        animator.SetInteger(animatorParameterCastFinishId, action.Data.onAnimationFinishId);
                    }
                }
                else
                {
                    if (action.Data.animationDelay > 0f)
                    {
                        Utilities.FunctionTimer.Create(this, () => animator.SetInteger(animatorParameterCastFinishId, 0), action.Data.animationDelay, $"{action.Data.actionName}_animation_{action.Data.animationName}_delay");
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
                castBar.maxValue = actionInfo.action.Data.cast;
                castBar.value = 0f;
            }
            if (castLengthText != null)
            {
                castLengthText.text = castTime.ToString("00.00").Replace(',', '.').Replace(':', '.').Replace(';', '.');
            }
            if (castNameText != null)
            {
                castNameText.text = actionInfo.action.Data.GetActionName();
            }
            if (castBarParty != null)
            {
                castBarParty.maxValue = actionInfo.action.Data.cast;
                castBarParty.value = 0f;
            }
            if (castNameTextParty != null)
            {
                if (actionInfo.target != null && showCastTargetLetter)
                {
                    castNameTextParty.text = $"{actionInfo.action.Data.GetActionName()}<sprite=\"{actionInfo.target.letterSpriteAsset}\" name=\"{actionInfo.target.characterLetter}\">";
                }
                else if (actionInfo.source != null && showCastTargetLetter)
                {
                    castNameTextParty.text = $"{actionInfo.action.Data.GetActionName()}<sprite=\"{actionInfo.source.letterSpriteAsset}\" name=\"{actionInfo.source.characterLetter}\">";
                }
                else
                {
                    castNameTextParty.text = actionInfo.action.Data.GetActionName();
                }
            }
            if (interruptText != null)
            {
                interruptText.alpha = 0f;
            }
        }

        private void HandleSpeech(CharacterAction action)
        {
            if (!string.IsNullOrEmpty(action.Data.speech))
            {
                if (speechBubbleText != null)
                {
                    speechBubbleText.text = action.Data.speech;
                }
                if (speechBubbleGroup != null)
                {
                    speechBubbleGroup.LeanAlpha(1f, 0.25f);
                    Utilities.FunctionTimer.Create(this, () => speechBubbleGroup.LeanAlpha(0f, 0.25f), speechBubbleDuration, $"{characterState.characterName}_{this}_speech_bubble_fade_out", true);
                }
            }
            if (action.Data.speechAudio != null)
            {
                if (speechBubbleAudio != null)
                {
                    if (FightTimeline.Instance != null && FightTimeline.Instance.jon && action.Data.jonSpeechAudio != null)
                    {
                        speechBubbleAudio.clip = action.Data.jonSpeechAudio;
                    }
                    else if (action.Data.speechAudio != null)
                    {
                        speechBubbleAudio.clip = action.Data.speechAudio;
                    }
                    if (speechBubbleAudio.clip != null)
                        speechBubbleAudio.Play();
                }
            }
        }

        private void HandleActionAudio(CharacterAction action, bool onExecute)
        {
            if (!playAudio)
                return;

            if (onExecute)
            {
                if (!string.IsNullOrEmpty(action.Data.onExecuteAudio) && AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlayAt(action.Data.onExecuteAudio, transform.position, audioParent, audioVolume);
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(action.Data.onCastAudio) && AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlayAt(action.Data.onCastAudio, transform.position, audioParent, audioVolume);
                }
            }
        }

        private void UpdateCharacterName()
        {
            if (characterState == null)
                characterState = GetComponent<CharacterState>();
            if (targetController == null)
                targetController = GetComponent<TargetController>();

            if (characterState == null)
                return;

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

        public void RefreshUserInterface()
        {
            if (castBar != null)
                castBarGroup = castBar.GetComponent<CanvasGroup>();
            if (castBarParty != null)
                castBarGroupParty = castBarParty.GetComponentInParent<CanvasGroup>();

            UpdateCharacterName();
            ResetCastBar();

            if (castBarGroup != null)
            {
                castBarGroup.alpha = 0f;
            }
            if (castBarGroupParty != null)
            {
                castBarGroupParty.alpha = 0f;
            }
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