// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using TMPro;
using dev.susybaka.raidsim.Core;
using dev.susybaka.raidsim.Characters;
using dev.susybaka.raidsim.Inputs;
using dev.susybaka.raidsim.StatusEffects;
using dev.susybaka.raidsim.UI;
using dev.susybaka.Shared;
using static dev.susybaka.raidsim.Core.GlobalData;

namespace dev.susybaka.raidsim.Actions
{
    public class CharacterAction : MonoBehaviour
    {
        public enum ActionKind { Standard, Role, General, BlueMage, Hidden }

        private CharacterState character;
        private CharacterActionRegistry actionRegistry;

        [Header("Info")]
        [SerializeField] private string actionId; // stable unique string
        [SerializeField] private CharacterActionData data;
        [SerializeField] private ActionKind kind = ActionKind.Standard;
        [SerializeField] private RecastType recastType = RecastType.standard;

        public string ActionId => actionId;
        public CharacterActionData Data => data;
        public float RecastTimer => recastTimer;
        public float LastRecastTimer => lastRecast;
        public RecastType RecastType => recastType;
        public RecastType NormalRecastType => normalRecastType;
        public ActionKind Kind => kind;

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
        public bool showOutline = false;
        public float damageMultiplier = 1f;
        public float distanceToTarget;
        public int chargesLeft = 0;
        public CharacterAction lastAction;
        public KeyBind currentKeybind;
        public List<Role> availableForRoles;
        public List<string> sharedRecasts;
        public List<StatusEffectData> comboOutlineEffects;
        public List<StatusEffectData> hideWhileEffects;
        public bool showInsteadWithEffects = false;
        public bool invisibilityAlsoDisables = false;

        [Header("Events")]
        public UnityEvent<ActionInfo> onExecute;
        public UnityEvent<ActionInfo> onCast;
        public UnityEvent<ActionInfo> onInterrupt;

        // Private
        private float recastTimer = 0f;
        private float animationLockTimer = 0f;
        private List<CharacterAction> actionsWithSharedRecasts = new List<CharacterAction>();
        private RecastType normalRecastType;
        private bool chargeRestored = false;
        private bool permanentlyUnavailable = false;
        private float lastRecast = 0f;

        /*public CharacterState GetCharacter()
        {
            return character;
        }*/


#if UNITY_EDITOR
        [ContextMenu("Generate Random Action ID")]
        private void GenerateActionId()
        {
            if (!string.IsNullOrWhiteSpace(actionId))
                return;
            actionId = System.Guid.NewGuid().ToString("N");
            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif

        private void Awake()
        {
            chargesLeft = data.charges;
            permanentlyUnavailable = unavailable;
            normalRecastType = recastType;
        }

        private void Start()
        {
            if (unavailable)
                Utilities.FunctionTimer.Create(this, () => gameObject.SetActive(false), 0.2f, $"{actionId}_start_disable_delay");
        }

        private void Update()
        {
            // Role availability logic
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

            // Reset action when it is unavailable
            if (unavailable)
            {
                ResetAnimationLock();
                ResetCooldown();
                chargesLeft = data.charges;
            }

            // Invisibility logic
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

            // Disable logic
            if (invisible)
            {
                if (invisibilityAlsoDisables)
                {
                    isDisabled = true;
                }
            }
            else
            {
                if (invisibilityAlsoDisables)
                {
                    isDisabled = false;
                }
            }

            // Availability logic (Range, target and distance)
            if (data.range > 0f && data.isTargeted && (distanceToTarget > data.range) && hasTarget && !unavailable)
            {
                isAvailable = false;
            }
            else if (!unavailable)
            {
                if (data.range > 0f && data.isTargeted && (distanceToTarget <= data.range) && hasTarget && data.charges > 1 && chargesLeft > 0)
                {
                    isAvailable = true;
                }
            }

            // Cooldown, animation lock and charge logic
            if (animationLockTimer > 0f)
            {
                animationLockTimer -= Time.unscaledDeltaTime;
                isAnimationLocked = true;
            }
            else
            {
                animationLockTimer = 0f;
                isAnimationLocked = false;
            }
            if (recastTimer > 0f)
            {
                recastTimer -= FightTimeline.deltaTime;
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
            }
            else
            {
                recastTimer = 0f;
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
            }

            // Combo outline logic
            if ((lastAction != null && data.comboActionIds != null && data.comboActionIds.Length > 0) || (comboOutlineEffects != null && comboOutlineEffects.Count > 0))
            {
                showOutline = false;

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

                if (lastAction != null && !string.IsNullOrEmpty(lastAction.actionId) && data.comboActionIds != null && data.comboActionIds.Length > 0)
                {
                    foreach (string comboActionId in data.comboActionIds)
                    {
                        if (lastAction.actionId.Equals(comboActionId))
                        {
                            showOutline = true;
                            break;
                        }
                    }
                }
            }
        }

        public void Initialize(ActionController controller)
        {
            actionRegistry = controller.ActionRegistry;

            if (actionRegistry != null)
            {
                foreach (string actionId in sharedRecasts)
                {
                    CharacterAction sharedAction = actionRegistry.GetById(actionId);
                    if (sharedAction != null)
                    {
                        actionsWithSharedRecasts.Add(sharedAction);
                    }
                }
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
        }

        public void ActivateCooldown(bool shared = false)
        {
            if (unavailable)
                return;

            if (chargesLeft < 1)
            {
                isAvailable = false;
            }
            if (recastTimer <= 0f)
            {
                lastRecast = data.recast;
                recastTimer = data.recast;
            }

            if (!shared)
            {
                if (sharedRecasts != null && sharedRecasts.Count > 0)
                {
                    foreach (CharacterAction sharedRecast in actionsWithSharedRecasts)
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
            if (recastTimer <= 0f)
            {
                lastRecast = recast;
                recastTimer = recast;
            }
        }

        public void ActivateAnimationLock(bool shared = false)
        {
            if (unavailable)
                return;

            isAnimationLocked = true;
            animationLockTimer = data.animationLock;

            if (!shared)
            {
                if (sharedRecasts != null && sharedRecasts.Count > 0)
                {
                    foreach (CharacterAction sharedRecast in actionsWithSharedRecasts)
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
            animationLockTimer = duration;
        }

        public void ResetCooldown(bool shared = false)
        {
            chargesLeft++;
            if (chargesLeft > data.charges)
            {
                chargesLeft = data.charges;
            }
            isAvailable = true;
            recastTimer = 0f;

            if (!shared)
            {
                if (sharedRecasts != null && sharedRecasts.Count > 0)
                {
                    foreach (CharacterAction sharedRecast in actionsWithSharedRecasts)
                    {
                        sharedRecast.ResetCooldown(true);
                    }
                }
            }
        }

        public void ResetAnimationLock(bool shared = false)
        {
            isAnimationLocked = false;
            animationLockTimer = 0f;

            if (!shared)
            {
                if (sharedRecasts != null && sharedRecasts.Count > 0)
                {
                    foreach (CharacterAction sharedRecast in actionsWithSharedRecasts)
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
    }
}