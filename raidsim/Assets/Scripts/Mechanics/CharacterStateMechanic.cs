// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using dev.susybaka.raidsim.Characters;
using static dev.susybaka.raidsim.Core.GlobalData;
using static dev.susybaka.raidsim.StatusEffects.StatusEffectData;

namespace dev.susybaka.raidsim.Mechanics
{
    public class CharacterStateMechanic : FightMechanic
    {
        public enum Type { Enable, Disable, Spawn, Destroy }

        [Header("Character State Mechanic")]
        [Tooltip("The main behavior type of the mechanic.")]
        public Type behaviorType = Type.Disable;
        [Tooltip("The characters to affect with the mechanic.")]
        public List<CharacterState> characters = new List<CharacterState>();
        [Space(10)]
        [Tooltip("If enabled, the mechanic will touch the character's nameplate visibility.")]
        public bool setNameplateVisibility = false;
        [Tooltip("If setting the nameplate visibility, this will invert it from the main behavior.")]
        [ShowIf(nameof(setNameplateVisibility))] public bool invertNameplate = false;
        [Tooltip("If enabled, the mechanic will touch the character's party list entry visibility.")]
        public bool setPartyListVisibility = false;
        [Tooltip("If setting the party list visibility, this will invert it from the main behavior.")]
        [ShowIf(nameof(setPartyListVisibility))] public bool invertPartyList = false;
        [Tooltip("If enabled, the mechanic will touch the character's targetable state.")]
        public bool setTargetability = false;
        [Tooltip("If setting the character's targetability, this will invert it from the main behavior.")]
        [ShowIf(nameof(setTargetability))] public bool invertTargetable = false;
        [Tooltip("If enabled, the mechanic will touch the character's 3D model's visibility.")]
        public bool setModelVisibility = false;
        [Tooltip("If setting the model's visibility, this will invert it from the main behavior.")]
        [ShowIf(nameof(setModelVisibility))] public bool invertModelVisibility = false;
        [Tooltip("If enabled, the mechanic will touch the GameObject's active state.")]
        public bool setGameObjectState = true;
        [Tooltip("If enabled, the mechanic will touch the CharacterState component's active state.")]
        public bool setCharacterState = true;
        [Space(10)]
        [Tooltip("If enabled, the mechanic will toggle the states instead of setting them to a specific value.")]
        public bool toggleInstead = false;
        [Tooltip("If enabled, the mechanic will touch only characters that have certain status effects.")]
        public bool filterByStatusEffects = false;
        [Tooltip("If filtering by status effects, require all specified status effects to be present on the character.")]
        [ShowIf(nameof(filterByStatusEffects))] public bool requireAllStatusEffects = false;
        [Tooltip("If filtering by status effects, ignore and keep unfiltered characters as they were, instead of reverting their state.")]
        [ShowIf(nameof(filterByStatusEffects))] public bool ignoreUnfiltered = false;
        [Tooltip("If filtering by status effects, the status effects that are required.")]
        [ShowIf(nameof(filterByStatusEffects))] public StatusEffectInfo[] requiredStatusEffects;
        [Space(10)]
        public int dummy;
        [Tooltip("The location to spawn characters at. If null, will use the action source or target.")]
        [ShowIf(nameof(behaviorType), Type.Spawn)] public Transform spawnLocation;

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
            }
        }

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
                    character.gameObject.SetActive(enable);
                }

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

                        if (setGameObjectState && !ignoreUnfiltered)
                        {
                            character.gameObject.SetActive(!enable);
                        }
                        else if (setCharacterState && !ignoreUnfiltered)
                        {
                            character.gameObject.SetActive(originalState);
                        }
                        continue;
                    }
                    if (!requireAllStatusEffects && !hasAnyStatusEffect)
                    {
                        if (log)
                            Debug.Log($"[CharacterStateMechanic ({gameObject.name})] {character.characterName} does not have any of the required status effects, skipping.");

                        if (setGameObjectState && !ignoreUnfiltered)
                        {
                            character.gameObject.SetActive(!enable);
                        }
                        else if (setCharacterState && !ignoreUnfiltered)
                        {
                            character.gameObject.SetActive(originalState);
                        }
                        continue;
                    }
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

            character.ToggleState(state);

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
                    if (setModelVisibility && !invertModelVisibility)
                        character.SetModelVisibility(false);
                    else if (setModelVisibility && invertModelVisibility)
                        character.SetModelVisibility(true);
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
                    if (setModelVisibility && !invertModelVisibility)
                        character.SetModelVisibility(true);
                    else if (setModelVisibility && invertModelVisibility)
                        character.SetModelVisibility(false);
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
            }
        }
    }
}