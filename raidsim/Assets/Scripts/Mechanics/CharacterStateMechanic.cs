// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using System.Collections;
using System.Collections.Generic;
using dev.susybaka.raidsim.Actions;
using dev.susybaka.raidsim.Characters;
using dev.susybaka.raidsim.Core;
using dev.susybaka.raidsim.Targeting;
using dev.susybaka.Shared;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Events;
using static dev.susybaka.raidsim.Core.GlobalData;
using static dev.susybaka.raidsim.StatusEffects.StatusEffectData;

namespace dev.susybaka.raidsim.Mechanics
{
    public class CharacterStateMechanic : FightMechanic
    {
        public enum Type { Enable, Disable, Spawn, Destroy, ExecuteAction, SetTarget, ExecuteMechanic }
        public enum ActionExecutionType { Standard, Hidden, Unrestricted }
        public enum TargetingType { Nearest, Furthest, OnePerEach, RandomPerEach, Random }

        [Header("Character State Mechanic")]
        [Tooltip("The main behavior type of the mechanic.")]
        public Type behaviorType = Type.Disable;
        [Tooltip("The characters to affect with the mechanic.")]
        public List<CharacterState> characters = new List<CharacterState>();
        [Space(10)]
        [Tooltip("If enabled, the mechanic will touch the character's nameplate visibility.")]
        [HideIf(nameof(_hideStateFields))] public bool setNameplateVisibility = false;
        [Tooltip("If setting the nameplate visibility, this will invert it from the main behavior.")]
        [ShowIf(nameof(setNameplateVisibility))] public bool invertNameplate = false;
        [Tooltip("If enabled, the mechanic will touch the character's party list entry visibility.")]
        [HideIf(nameof(_hideStateFields))] public bool setPartyListVisibility = false;
        [Tooltip("If setting the party list visibility, this will invert it from the main behavior.")]
        [ShowIf(nameof(setPartyListVisibility))] public bool invertPartyList = false;
        [Tooltip("If enabled, the mechanic will touch the character's targetable state.")]
        [HideIf(nameof(_hideStateFields))] public bool setTargetability = false;
        [Tooltip("If setting the character's targetability, this will invert it from the main behavior.")]
        [ShowIf(nameof(setTargetability))] public bool invertTargetable = false;
        [Tooltip("If enabled, the mechanic will touch the character's 3D model's visibility.")]
        [HideIf(nameof(_hideStateFields))] public bool setModelVisibility = false;
        [Tooltip("If setting the model's visibility, this will invert it from the main behavior.")]
        [ShowIf(nameof(setModelVisibility))] public bool invertModelVisibility = false;
        [ShowIf(nameof(setModelVisibility))] public bool fadeModelVisibility = false;
        [ShowIf(nameof(fadeModelVisibility))] public float modelVisibilityFadeTime = 0.5f;
        [Tooltip("If enabled, the mechanic will touch the GameObject's active state.")]
        [HideIf(nameof(_hideStateFields))] public bool setGameObjectState = true;
        [Tooltip("If setting the GameObject's active state, the delay before changing it.")]
        [HideIf(nameof(_hideGameObjectStateDelay))] public float gameObjectStateDelay = 0f;
        [Tooltip("If enabled, the mechanic will touch the CharacterState component's active state.")]
        [HideIf(nameof(_hideStateFields))] public bool setCharacterState = true;
        [Space(10)]
        [Tooltip("If enabled, the mechanic is allowed to accept runtime added characters as well as pre-defined ones.")]
        public bool allowDynamicCharacters = false;
        [Tooltip("If enabled, the mechanic will toggle the states instead of setting them to a specific value.")]
        [HideIf(nameof(_hideStateFields))] public bool toggleInstead = false;
        [Tooltip("If enabled, the mechanic will touch only characters that have certain status effects.")]
        public bool filterByStatusEffects = false;
        [Tooltip("If filtering by status effects, require all specified status effects to be present on the character.")]
        [ShowIf(nameof(filterByStatusEffects))] public bool requireAllStatusEffects = false;
        [Tooltip("If filtering by status effects, ignore and keep unfiltered characters as they were, instead of reverting their state.")]
        [ShowIf(nameof(filterByStatusEffects))] public bool ignoreUnfiltered = false;
        [Tooltip("If filtering by status effects, the status effects that are required.")]
        [ShowIf(nameof(filterByStatusEffects))] public StatusEffectInfo[] requiredStatusEffects;
        [Tooltip("If enabled, the mechanic will change the characters model to a specific variant.")]
        public bool changeModel = false;
        [Tooltip("If changing the model variant, the index of the model to change to.")]
        [ShowIf(nameof(changeModel))] public int modelIndex = 0;
        [Space(10)]
        //public int dummy;
        [Tooltip("The location to spawn characters at. If null, will use the action source or target.")]
        [ShowIf(nameof(behaviorType), Type.Spawn)] public Transform spawnLocation;
        [Tooltip("If executing an action, the type of execution to perform.")]
        [ShowIf(nameof(behaviorType), Type.ExecuteAction)] public ActionExecutionType executionType = ActionExecutionType.Standard;
        [Tooltip("If executing an action, whether to use already queued actions or execute a specified one immediately.")]
        [ShowIf(nameof(behaviorType), Type.ExecuteAction)] public bool useQueuedActions = true;
        [Tooltip("If executing an action and not using queued actions, the action the characters should execute.")]
        [HideIf(nameof(useQueuedActions))] public CharacterAction action;
        [Tooltip("If executing an action and not using queued actions, the action the characters should execute.")]
        [HideIf(nameof(useQueuedActions))] public CharacterActionData actionData;
        [Tooltip("If setting targets, the targeting method to use.")]
        [ShowIf(nameof(behaviorType), Type.SetTarget)] public TargetingType targetingType = TargetingType.Nearest;
        [Tooltip("If setting targets or executing actions, whether to use any defined filtering on the targets instead of the characters that are targeting or performing actions.")]
        [ShowIf(nameof(_hideStateFields))] public bool filterTargetsInstead = false;
        [Tooltip("If setting targets, whether to set the rotation of the character to face the target.")]
        [ShowIf(nameof(behaviorType), Type.SetTarget)] public bool setRotationTarget = false;
        [Tooltip("If setting targets, the list of targets available.")]
        [ShowIf(nameof(behaviorType), Type.SetTarget)] public List<TargetNode> targetList;
        [Tooltip("If executing a mechanic, the mechanic to execute on the characters.")]
        [ShowIf(nameof(behaviorType), Type.ExecuteMechanic)] public FightMechanic mechanic;
        [Tooltip("If executing a mechanic, an optional override for the source character of the mechanic.")]
        [ShowIf(nameof(behaviorType), Type.ExecuteMechanic)] public CharacterState overrideMechanicSource = null;
        [Space(10)]
        public UnityEvent<CharacterState> onProcessCharacter;

        private List<CharacterState> originalCharacters = new List<CharacterState>();
        private readonly List<TargetNode> _remainingTargets = new();
#pragma warning disable CS0414
        // Editor only
        private bool _hideStateFields = false;
        private bool _hideGameObjectStateDelay => (_hideStateFields || !setGameObjectState);

#if UNITY_EDITOR
        public void OnValidate()
        {
            if (behaviorType != Type.Spawn)
            {
                spawnLocation = null;
            }
            if (behaviorType != Type.ExecuteAction)
            {
                useQueuedActions = true;
                action = null;
            }
            if (behaviorType != Type.ExecuteAction && behaviorType != Type.SetTarget && behaviorType != Type.ExecuteMechanic)
            {
                _hideStateFields = false;
            }
            if (behaviorType == Type.ExecuteAction || behaviorType == Type.SetTarget || behaviorType == Type.ExecuteMechanic)
            {
                setCharacterState = false;
                setGameObjectState = false;
                setNameplateVisibility = false;
                setPartyListVisibility = false;
                setTargetability = false;
                setModelVisibility = false;
                fadeModelVisibility = false;
                _hideStateFields = true;
            }
        }
#endif
#pragma warning restore CS0414

        private void Awake()
        {
            originalCharacters = new List<CharacterState>(characters);
        }

        public override void TriggerMechanic(ActionInfo actionInfo)
        {
            if (!CanTrigger(actionInfo))
                return;

            if (characters == null || characters.Count < 1)
                return;

            switch (behaviorType)
            {
                case Type.Enable:
                    if (log)
                        Debug.Log($"[CharacterStateMechanic ({gameObject.name})] Enabling {characters.Count} characters.");

                    HandleCharacters(true);
                    break;
                case Type.Disable:
                    if (log)
                        Debug.Log($"[CharacterStateMechanic ({gameObject.name})] Disabling {characters.Count} characters.");

                    HandleCharacters(false);
                    break;
                case Type.Spawn:
                    if (log)
                        Debug.Log($"[CharacterStateMechanic ({gameObject.name})] Spawning {characters.Count} characters.");

                    SpawnCharacters(actionInfo);
                    break;
                case Type.Destroy:
                    if (log)
                        Debug.Log($"[CharacterStateMechanic ({gameObject.name})] Destroying {characters.Count} characters.");

                    DestroyCharacters();
                    break;
                case Type.ExecuteAction:
                    if (log)
                        Debug.Log($"[CharacterStateMechanic ({gameObject.name})] Executing requested action for {characters.Count} characters.");

                    ActionCharacters();
                    break;
                case Type.SetTarget:
                    if (log)
                        Debug.Log($"[CharacterStateMechanic ({gameObject.name})] Setting targets for {characters.Count} characters.");

                    TargetCharacters();
                    break;
                case Type.ExecuteMechanic:
                    if (log)
                        Debug.Log($"[CharacterStateMechanic ({gameObject.name})] Executing requested mechanic for {characters.Count} characters.");

                    MechanicCharacters(actionInfo);
                    break;
            }
        }

        // Interrupt does nothing for this mechanic, but it is called on timeline reset, so we can use it to reset dynamic characters
        public override void InterruptMechanic(ActionInfo actionInfo)
        {
            base.InterruptMechanic(actionInfo);

            if (allowDynamicCharacters)
            {
                ResetCharacters();
            }
        }

        protected override bool UsesPCG()
        {
            return true;
        }

        #region Dynamic Characters
        public void AddActionSourceCharacter(ActionInfo actionInfo)
        {
            if (actionInfo.source == null)
            {
                if (log)
                    Debug.Log($"[CharacterStateMechanic ({gameObject.name})] Attempted to add a null CharacterState, skipping.");
                return;
            }

            AddCharacter(actionInfo.source);
        }

        public void AddActionTargetCharacter(ActionInfo actionInfo)
        {
            if (actionInfo.target == null)
            {
                if (log)
                    Debug.Log($"[CharacterStateMechanic ({gameObject.name})] Attempted to add a null CharacterState, skipping.");
                return;
            }

            AddCharacter(actionInfo.target);
        }

        public void AddActionCharacters(ActionInfo actionInfo)
        {
            AddActionSourceCharacter(actionInfo);
            AddActionTargetCharacter(actionInfo);
        }

        public void AddCharacter(CharacterState character)
        {
            if (character == null)
            {
                if (log)
                    Debug.Log($"[CharacterStateMechanic ({gameObject.name})] Attempted to add a null CharacterState, skipping.");
                return;
            }

            if (!allowDynamicCharacters)
            {
                if (log)
                    Debug.Log($"[CharacterStateMechanic ({gameObject.name})] Dynamic characters are not allowed, skipping addition of {character.characterName} ({character.gameObject.name}).");
                return;
            }

            if (characters.Contains(character))
            {
                if (log)
                    Debug.Log($"[CharacterStateMechanic ({gameObject.name})] CharacterState {character.characterName} ({character.gameObject.name}) is already in the list, skipping.");
                return;
            }

            if (TryFilter(character))
            {
                characters.Add(character);
            }
        }

        public void RemoveActionSourceCharacter(ActionInfo actionInfo)
        {
            if (actionInfo.source == null)
            {
                if (log)
                    Debug.Log($"[CharacterStateMechanic ({gameObject.name})] Attempted to remove a null CharacterState, skipping.");
                return;
            }

            RemoveCharacter(actionInfo.source);
        }

        public void RemoveActionTargetCharacter(ActionInfo actionInfo)
        {
            if (actionInfo.target == null)
            {
                if (log)
                    Debug.Log($"[CharacterStateMechanic ({gameObject.name})] Attempted to remove a null CharacterState, skipping.");
                return;
            }
            RemoveCharacter(actionInfo.target);
        }

        public void RemoveActionCharacters(ActionInfo actionInfo)
        {
            RemoveActionSourceCharacter(actionInfo);
            RemoveActionTargetCharacter(actionInfo);
        }

        public void RemoveCharacter(CharacterState character)
        {
            if (character == null)
            {
                if (log)
                    Debug.Log($"[CharacterStateMechanic ({gameObject.name})] Attempted to remove a null CharacterState, skipping.");
                return;
            }

            if (!allowDynamicCharacters)
            {
                if (log)
                    Debug.Log($"[CharacterStateMechanic ({gameObject.name})] Dynamic characters are not allowed, skipping removal of {character.characterName} ({character.gameObject.name}).");
                return;
            }

            if (!characters.Contains(character))
            {
                if (log)
                    Debug.Log($"[CharacterStateMechanic ({gameObject.name})] CharacterState {character.characterName} ({character.gameObject.name}) is not present in the list, skipping.");
                return;
            }

            if (TryFilter(character))
            {
                characters.Remove(character);
            }
        }

        public void ResetCharacters()
        {
            if (!allowDynamicCharacters)
            {
                if (log)
                    Debug.Log($"[CharacterStateMechanic ({gameObject.name})] Dynamic characters are not allowed, skipping reset of character list.");
                return;
            }

            if (log)
                Debug.Log($"[CharacterStateMechanic ({gameObject.name})] Resetting character list of ({characters.Count}) to original pre-defined characters.");

            characters.Clear();
            characters = new List<CharacterState>(originalCharacters);

            if (log)
                Debug.Log($"[CharacterStateMechanic ({gameObject.name})] Character list reset, now containing {characters.Count} characters.");
        }
        #endregion

        private void SpawnCharacters(ActionInfo actionInfo)
        {
            foreach (CharacterState character in characters)
            {
                Transform location = spawnLocation;

                Transform parent = GameObject.Find("Enemies").transform;

                if (!character.isAggressive)
                    parent = GameObject.Find("Characters").transform;

                if (location == null)
                {
                    if (actionInfo.target != null)
                        location = actionInfo.target.transform;
                    else if (actionInfo.source != null && actionInfo.action != null)
                        location = actionInfo.source.transform;
                }

                CharacterState spawned = Instantiate(character, location.position, location.rotation, parent);

                UpdateCharacterState(spawned, true);
            }
        }

        private void DestroyCharacters()
        {
            foreach (CharacterState character in characters)
            {
                if (character == null)
                {
                    if (log)
                        Debug.Log($"[CharacterStateMechanic ({gameObject.name})] CharacterState is null or invalid, skipping.");
                    continue;
                }

                if (setCharacterState)
                {
                    UpdateCharacterState(character, false);
                }

                if (setGameObjectState)
                {
                    character.gameObject.SetActive(false);
                }

                if (log)
                    Debug.Log($"[CharacterStateMechanic ({gameObject.name})] Destroying character object {character.gameObject.name} for {character.characterName}.");
                
                Destroy(character.gameObject, 0.1f);
            }
        }

        private void ActionCharacters()
        {
            foreach (CharacterState character in characters)
            {
                if (character == null || character.actionController == null)
                {
                    if (log)
                        Debug.Log($"[CharacterStateMechanic ({gameObject.name})] CharacterState or ActionController is null or invalid, skipping.");
                    continue;
                }

                if (filterTargetsInstead && character.targetController != null)
                {
                    if (character.targetController.currentTarget == null)
                    {
                        if (log)
                            Debug.Log($"[CharacterStateMechanic ({gameObject.name})] {character.characterName} has no current target to filter, skipping.");
                        continue;
                    }

                    CharacterState targetCharacter = character.targetController.currentTarget.GetCharacterState();

                    if (targetCharacter == null)
                    {
                        if (log)
                            Debug.Log($"[CharacterStateMechanic ({gameObject.name})] {character.characterName}'s target is null or invalid, skipping.");
                        continue;
                    }

                    if (!TryFilter(targetCharacter))
                    {
                        if (log)
                            Debug.Log($"[CharacterStateMechanic ({gameObject.name})] {character.characterName}'s target {targetCharacter.characterName} did not pass filtering, skipping action execution.");
                        continue;
                    }
                } 
                else if (filterTargetsInstead && character.targetController == null)
                {
                    if (log)
                        Debug.LogWarning($"[CharacterStateMechanic ({gameObject.name})] {character.characterName} has no TargetController to filter with, skipping.");
                }

                if (useQueuedActions)
                {
                    if (log)
                        Debug.Log($"[CharacterStateMechanic ({gameObject.name})] Having {character.characterName} execute the next queued action.");

                    switch (executionType)
                    {
                        case ActionExecutionType.Standard:
                            character.actionController.PerformQueuedAction();
                            break;
                        case ActionExecutionType.Hidden:
                            character.actionController.PerformQueuedActionHidden();
                            break;
                        case ActionExecutionType.Unrestricted:
                            character.actionController.PerformQueuedActionUnrestricted();
                            break;
                    }
                }
                else
                {
                    if (action == null && actionData == null)
                    {
                        if (log)
                            Debug.Log($"[CharacterStateMechanic ({gameObject.name})] No action specified for {character.characterName}, skipping.");
                        continue;
                    }
                    if (log)
                        Debug.Log($"[CharacterStateMechanic ({gameObject.name})] Having {character.characterName} execute action {(action != null ? action.Data.actionName : actionData != null ? actionData.actionName : "Unknown Action")}{(action != null ? $" ({action.gameObject.name})" : "")}.");

                    switch (executionType)
                    {
                        case ActionExecutionType.Standard:
                            if (action != null)
                                character.actionController.PerformAction(action);
                            else
                                character.actionController.PerformAction(actionData.actionName);
                            break;
                        case ActionExecutionType.Hidden:
                            if (action != null)
                                character.actionController.PerformActionHidden(action);
                            else
                                character.actionController.PerformActionHidden(actionData.actionName);
                            break;
                        case ActionExecutionType.Unrestricted:
                            if (action != null)
                                character.actionController.PerformActionUnrestricted(action);
                            else
                                character.actionController.PerformActionUnrestricted(actionData.actionName);
                            break;
                    }
                }

                if (changeModel)
                {
                    character.modelHandler.SwitchActiveCharacterModel(modelIndex);
                }

                onProcessCharacter.Invoke(character);
            }
        }

        private void TargetCharacters()
        {
            if (targetList == null || targetList.Count < 1)
            {
                if (log)
                    Debug.LogWarning($"[CharacterStateMechanic ({gameObject.name})] No targets specified!");
                return;
            }
            
            HashSet<TargetNode> usedTargets = new HashSet<TargetNode>();
            
            foreach (CharacterState character in characters)
            {
                if (character == null || character.targetController == null)
                {
                    if (log)
                        Debug.Log($"[CharacterStateMechanic ({gameObject.name})] CharacterState or TargetController is null or invalid, skipping.");
                    continue;
                }

                TargetNode finalTarget = null;
                List<TargetNode> availableTargets = new List<TargetNode>(targetList);

                if (filterTargetsInstead)
                {
                    for (int i = availableTargets.Count - 1; i >= 0; i--)
                    {
                        TargetNode targetNode = availableTargets[i];
                        CharacterState targetCharacter = targetNode.GetCharacterState();
                        if (targetCharacter == null)
                        {
                            availableTargets.RemoveAt(i);
                            continue;
                        }
                        if (!TryFilter(targetCharacter))
                        {
                            availableTargets.RemoveAt(i);
                        }
                    }
                }
                else
                {
                    if (!TryFilter(character))
                    {
                        if (log)
                            Debug.Log($"[CharacterStateMechanic ({gameObject.name})] {character.characterName} did not pass filtering, skipping target assignment.");
                        continue;
                    }
                }

                if (availableTargets == null || availableTargets.Count < 1)
                {
                    if (log)
                        Debug.Log($"[CharacterStateMechanic ({gameObject.name})] No targets specified for {character.characterName}, skipping.");
                    continue;
                }

                switch (targetingType)
                {
                    case TargetingType.Nearest:
                        float nearestDistance = float.MaxValue;

                        foreach (TargetNode targetNode in availableTargets)
                        {
                            float distance = Vector3.Distance(character.transform.position, targetNode.transform.position);
                            if (distance < nearestDistance)
                            {
                                nearestDistance = distance;
                                finalTarget = targetNode;
                            }
                        }
                        break;
                    case TargetingType.Furthest:
                        float furthestDistance = float.MinValue;
                        
                        foreach (TargetNode targetNode in availableTargets)
                        {
                            float distance = Vector3.Distance(character.transform.position, targetNode.transform.position);
                            if (distance > furthestDistance)
                            {
                                furthestDistance = distance;
                                finalTarget = targetNode;
                            }
                        }
                        break;
                    case TargetingType.OnePerEach:
                        if (availableTargets.Count >= characters.Count)
                        {
                            finalTarget = availableTargets[characters.IndexOf(character)];
                        }
                        else
                        {
                            if (log)
                                Debug.LogWarning($"[CharacterStateMechanic ({gameObject.name})] Not enough available targets for OnePerEach targeting, defaulting to first target.");
                            finalTarget = availableTargets[0];
                        }
                        break;
                    case TargetingType.RandomPerEach:
                        if (availableTargets.Count >= characters.Count)
                        {
                            _remainingTargets.Clear();
                            for (int i = 0; i < availableTargets.Count; i++)
                            {
                                var t = availableTargets[i];
                                if (!usedTargets.Contains(t))
                                    _remainingTargets.Add(t);
                            }

                            if (_remainingTargets.Count == 0)
                            {
                                if (log)
                                    Debug.LogWarning($"[CharacterStateMechanic ({gameObject.name})] No remaining targets for RandomPerEach, defaulting to first target.");
                                finalTarget = availableTargets[0];
                                break;
                            }

                            // Deterministic stream for this mechanic choice
                            var stream = timeline.random.Stream($"{GetUniqueName()}_TargetingType_RandomPerEach");

                            int pick = stream.NextInt(0, _remainingTargets.Count);
                            finalTarget = _remainingTargets[pick];

                            usedTargets.Add(finalTarget);
                        }
                        else
                        {
                            if (log)
                                Debug.LogWarning($"[CharacterStateMechanic ({gameObject.name})] Not enough available targets for RandomPerEach targeting, defaulting to first target.");
                            finalTarget = availableTargets[0];
                        }
                        break;
                    case TargetingType.Random:
                        if (availableTargets.Count > 1)
                        {
                            int r = timeline.random.Pick($"{GetUniqueName()}_TargetingType_Random", availableTargets.Count, timeline.GlobalRngMode);

                            finalTarget = availableTargets[r];
                        }
                        else
                        {
                            if (log)
                                Debug.LogWarning($"[CharacterStateMechanic ({gameObject.name})] Not enough available targets for Random targeting, defaulting to first target.");
                            finalTarget = availableTargets[0];
                        }
                        break;
                }

                if (log)
                    Debug.Log($"[CharacterStateMechanic ({gameObject.name})] Having {character.characterName} target {finalTarget.GetCharacterState()?.characterName} ({finalTarget.transform.parent.gameObject.name}).");

                // Set the target
                character.targetController.SetTarget(finalTarget);

                if (setRotationTarget && character.bossController != null)
                {
                    character.bossController.SetLookRotation(finalTarget);
                    character.bossController.SetRotationTarget(finalTarget);
                }

                if (changeModel)
                {
                    character.modelHandler.SwitchActiveCharacterModel(modelIndex);
                }

                onProcessCharacter.Invoke(character);
            }
        }

        private void MechanicCharacters(ActionInfo actionInfo)
        {
            if (mechanic == null)
            {
                if (log)
                    Debug.LogWarning($"[CharacterStateMechanic ({gameObject.name})] No mechanic specified to execute!");
                return;
            }

            CharacterState source = actionInfo.source;

            if (overrideMechanicSource != null)
            {
                source = overrideMechanicSource;
            }

            foreach (CharacterState character in characters)
            {
                if (character == null)
                {
                    if (log)
                        Debug.Log($"[CharacterStateMechanic ({gameObject.name})] CharacterState is null or invalid, skipping.");
                    continue;
                }

                if (filterTargetsInstead)
                {
                    if (character.targetController == null || character.targetController.currentTarget == null)
                    {
                        if (log)
                            Debug.Log($"[CharacterStateMechanic ({gameObject.name})] {character.characterName} has no current target to filter, skipping.");
                        continue;
                    }
                    CharacterState targetCharacter = character.targetController.currentTarget.GetCharacterState();
                    if (targetCharacter == null)
                    {
                        if (log)
                            Debug.Log($"[CharacterStateMechanic ({gameObject.name})] {character.characterName}'s target is null or invalid, skipping.");
                        continue;
                    }
                    if (!TryFilter(targetCharacter))
                    {
                        if (log)
                            Debug.Log($"[CharacterStateMechanic ({gameObject.name})] {character.characterName}'s target {targetCharacter.characterName} did not pass filtering, skipping mechanic execution.");
                        continue;
                    }
                }
                else
                {
                    if (!TryFilter(character))
                    {
                        if (log)
                            Debug.Log($"[CharacterStateMechanic ({gameObject.name})] {character.characterName} did not pass filtering, skipping mechanic execution.");
                        continue;
                    }
                }

                mechanic.TriggerMechanic(new ActionInfo(actionInfo.action, source, character));

                onProcessCharacter.Invoke(character);
            }
        }

        private void HandleCharacters(bool enable)
        {
            foreach (CharacterState character in characters)
            {
                if (character == null)
                {
                    if (log)
                        Debug.Log($"[CharacterStateMechanic ({gameObject.name})] CharacterState is null or invalid, skipping.");
                    continue;
                }

                bool originalState = character.gameObject.activeSelf;

                if (setGameObjectState)
                {
                    if (gameObjectStateDelay <= 0f)
                        character.gameObject.SetActive(enable);
                    else
                        StartCoroutine(IE_SetGameObjectDelayed(character.gameObject, enable, new WaitForSeconds(gameObjectStateDelay)));
                }

                if (!TryFilter(character, enable, originalState))
                {
                    continue;
                }

                if (log)
                    Debug.Log($"[CharacterStateMechanic ({gameObject.name})] {(enable == true ? "Activating" : "Disabling")} character object {character.gameObject.name} for {character.characterName}.");

                if (setCharacterState)
                {
                    UpdateCharacterState(character, enable);
                }
            }
        }

        private void UpdateCharacterState(CharacterState character, bool state)
        {
            if (character == null)
                return;

            // Only toggle the character state here, as the GameObject active state is handled by this script directly
            // And we don't want to interfere with that logic
            character.ToggleCharacterState(state);

            if (log)
                Debug.Log($"[SpawnEnemiesMechanic ({gameObject.name})] Updated the state of {character.characterName}.\nToggle Statuses (If relevant): Nameplate state '{!character.hideNameplate}' PartylistEntry '{!character.hidePartyListEntry}' Untargetable '{!character.untargetable.value}' ModelVisibility '{!character.hidden.value}'");

            if (!toggleInstead)
            {
                if (!state)
                {
                    if (setNameplateVisibility && !invertNameplate)
                        character.ToggleNameplate(false);
                    else if (setNameplateVisibility && invertNameplate)
                        character.ToggleNameplate(true);
                    if (setPartyListVisibility && !invertPartyList)
                        character.TogglePartyListEntry(false);
                    else if (setPartyListVisibility && invertPartyList)
                        character.TogglePartyListEntry(true);
                    if (setTargetability && !invertTargetable)
                        character.ToggleTargetable(false);
                    else if (setTargetability && invertTargetable)
                        character.ToggleTargetable(true);
                    if (setModelVisibility)
                    {
                        bool to = false;

                        if (!invertModelVisibility)
                            to = false;
                        else
                            to = true;

                        if (log)
                            Debug.Log($"[CharacterStateMechanic ({gameObject.name})] Setting model visibility for {character.characterName} to {to} over {modelVisibilityFadeTime}.");

                        if (fadeModelVisibility && modelVisibilityFadeTime > 0f)
                            character.FadeModelVisibility(to, modelVisibilityFadeTime);
                        else
                            character.SetModelVisibility(to);
                    }
                }
                else
                {
                    if (setNameplateVisibility && !invertNameplate)
                        character.ToggleNameplate(true);
                    else if (setNameplateVisibility && invertNameplate)
                        character.ToggleNameplate(false);
                    if (setPartyListVisibility && !invertPartyList)
                        character.TogglePartyListEntry(true);
                    else if (setPartyListVisibility && invertPartyList)
                        character.TogglePartyListEntry(false);
                    if (setTargetability && !invertTargetable)
                        character.ToggleTargetable(true);
                    else if (setTargetability && invertTargetable)
                        character.ToggleTargetable(false);
                    if (setModelVisibility)
                    {
                        bool to = false;

                        if (!invertModelVisibility)
                            to = true;
                        else
                            to = false;

                        if (log)
                            Debug.Log($"[CharacterStateMechanic ({gameObject.name})] Setting model visibility for {character.characterName} to {to} over {modelVisibilityFadeTime}.");

                        if (fadeModelVisibility && modelVisibilityFadeTime > 0f)
                            character.FadeModelVisibility(to, modelVisibilityFadeTime);
                        else
                            character.SetModelVisibility(to);
                    }
                }
            }
            else
            {
                if (setNameplateVisibility)
                    character.ToggleNameplate(!character.hideNameplate);
                if (setPartyListVisibility)
                    character.TogglePartyListEntry(!character.hidePartyListEntry);
                if (setTargetability)
                    character.ToggleTargetable(!character.untargetable.value); // 1. Both of these are a bit risky to check like this because something else could have toggled them but should be fine hopefully
                if (setModelVisibility)
                    character.SetModelVisibility(!character.hidden.value); // 2. Also don't want to touch them now since they have been working fine like this for a while
                if (setModelVisibility)
                {
                    bool to = !character.hidden.value;

                    if (fadeModelVisibility && modelVisibilityFadeTime > 0f)
                        character.FadeModelVisibility(to, modelVisibilityFadeTime);
                    else
                        character.SetModelVisibility(to);
                }
            }

            if (changeModel)
            {
                if (fadeModelVisibility && modelVisibilityFadeTime > 0f)
                {
                    Utilities.FunctionTimer.Create(character, () => { character.modelHandler.SwitchActiveCharacterModel(modelIndex); }, modelVisibilityFadeTime, $"{character.characterName.Replace(' ','_')}_{character.gameObject.name}_ChangeModelFadeDelay", false, false);
                }
                else
                {
                    character.modelHandler.SwitchActiveCharacterModel(modelIndex);
                }
            }

            onProcessCharacter.Invoke(character);

            if (log)
                Debug.Log($"[CharacterStateMechanic ({gameObject.name})] Updated the visibility states for {character.characterName}.", character.gameObject);
        }

        private IEnumerator IE_SetGameObjectDelayed(GameObject gameObject, bool state, WaitForSeconds wait)
        {
            yield return wait;
            gameObject.SetActive(state);
        }

        private bool TryFilter(CharacterState character)
        {
            return TryFilter(character, true, character.gameObject.activeSelf);
        }

        private bool TryFilter(CharacterState character, bool enable, bool originalState)
        {
            if (filterByStatusEffects)
            {
                if (log)
                    Debug.Log($"[CharacterStateMechanic ({gameObject.name})] Filtering {character.characterName} for status effects.");

                bool hasAllStatusEffects = true;
                bool hasAnyStatusEffect = false;

                foreach (StatusEffectInfo statusEffect in requiredStatusEffects)
                {
                    if (character.HasEffect(statusEffect.data.statusName, statusEffect.tag))
                    {
                        hasAnyStatusEffect = true;
                    }
                    else
                    {
                        hasAllStatusEffects = false;
                    }
                }

                // Skip if requirements not met
                if (requireAllStatusEffects && !hasAllStatusEffects)
                {
                    if (log)
                        Debug.Log($"[CharacterStateMechanic ({gameObject.name})] {character.characterName} is missing required status effects, skipping.");

                    if (behaviorType != Type.ExecuteAction && behaviorType != Type.Destroy)
                    {
                        if (setGameObjectState && !ignoreUnfiltered)
                        {
                            character.gameObject.SetActive(!enable);
                        }
                        else if (setCharacterState && !ignoreUnfiltered)
                        {
                            character.gameObject.SetActive(originalState);
                        }
                    }
                    return false;
                }
                if (!requireAllStatusEffects && !hasAnyStatusEffect)
                {
                    if (log)
                        Debug.Log($"[CharacterStateMechanic ({gameObject.name})] {character.characterName} does not have any of the required status effects, skipping.");

                    if (behaviorType != Type.ExecuteAction && behaviorType != Type.Destroy)
                    {
                        if (setGameObjectState && !ignoreUnfiltered)
                        {
                            character.gameObject.SetActive(!enable);
                        }
                        else if (setCharacterState && !ignoreUnfiltered)
                        {
                            character.gameObject.SetActive(originalState);
                        }
                    }
                    return false;
                }
            }
            return true;
        }
    }
}