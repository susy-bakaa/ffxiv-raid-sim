// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using NaughtyAttributes;
using dev.susybaka.raidsim.Characters;
using dev.susybaka.raidsim.Core;
using dev.susybaka.raidsim.StatusEffects;
using dev.susybaka.raidsim.UI;
using dev.susybaka.Shared;
using static dev.susybaka.raidsim.Core.GlobalData;
using static dev.susybaka.raidsim.StatusEffects.StatusEffectData;

namespace dev.susybaka.raidsim.Mechanics
{
    public class RaidwideDebuffMechanic : FightMechanic
    {
        [Header("Raidwide Debuff Mechanic Settings")]
        public PartyList party;
        public bool autoFindParty = false;
        public bool randomizeParty = true;
        public StatusEffectInfo effect;
        public bool ignoreRoles = true;
        public bool incrementalTag = false;
        public bool cleansEffect = false;
        public bool killsEffect = false;
        public bool togglesEffect = false;
        [ShowIf("togglesEffect")] public bool beginsWithCleanse = false;
        [ShowIf("togglesEffect")] public UnityEvent<bool> onToggleEffect;

        List<CharacterState> partyMembers;
        private bool startingCleanseEnabled = true;

        private void Awake()
        {
            if (party == null && autoFindParty)
                party = FightTimeline.Instance.partyList;

            if (beginsWithCleanse)
                startingCleanseEnabled = true;
            else
                startingCleanseEnabled = false;
        }

        public override void TriggerMechanic(ActionInfo actionInfo)
        {
            if (!CanTrigger(actionInfo))
                return;

            CharacterState from = null;

            if (actionInfo.source != null)
                from = actionInfo.source;
            else if (actionInfo.target != null)
                from = actionInfo.target;

            if (party == null && autoFindParty)
                party = FightTimeline.Instance.partyList;

            if (party != null && party.members.Count > 0)
            {
                partyMembers = new List<CharacterState>(party.GetActiveMembers()); // Copy the party members list

                /*for (int i = 0; i < party.members.Count; i++)
                {
                    partyMembers.Add(party.members[i].characterState);
                }*/

                if (randomizeParty)
                    partyMembers.Shuffle();

                int currentTag = effect.tag;
                bool cleansEffect = this.cleansEffect;

                // Iterate through each status effect
                for (int i = 0; i < partyMembers.Count; i++)
                {
                    // Find a suitable party member for the effect
                    CharacterState target = partyMembers[i];

                    if (!ignoreRoles || !cleansEffect || !killsEffect)
                    {
                        target = FindSuitableTarget(effect.data, partyMembers);
                    }

                    // If no suitable target found, apply to a random member
                    if (target == null)
                        target = partyMembers[Random.Range(0, partyMembers.Count)];

                    if (log)
                        Debug.Log($"[RaidwideDebuffMechanic] Processing target {target.characterName} for effect {effect.data.statusName} with tag {currentTag} and stacks {effect.stacks}\n\nDoes target already have the debuff? {target.HasEffect(effect.data.statusName, currentTag)}\n");

                    if (killsEffect)
                    {
                        if (log)
                            Debug.Log($"[RaidwideDebuffMechanic] Trying to kill {target.characterName} if effect {effect.data.statusName} with tag {currentTag} is present");

                        if (target.HasEffect(effect.data.statusName, currentTag))
                        {
                            if (log)
                                Debug.Log($"[RaidwideDebuffMechanic] Killing {target.characterName} with effect {effect.data.statusName} and tag {currentTag}");

                            target.ModifyHealth(new Damage(100, true, true, Damage.DamageType.unique, Damage.ElementalAspect.unaspected, Damage.PhysicalAspect.none, Damage.DamageApplicationType.percentageFromMax, Utilities.InsertSpaceBeforeCapitals(effect.data.statusName)), true);
                        }
                    }
                    else
                    {
                        if (togglesEffect)
                        {
                            if (!startingCleanseEnabled)
                            {
                                if (!target.HasEffect(effect.data.statusName, currentTag))
                                    cleansEffect = false; // If the target does not have the effect, it cannot be cleansed
                                else
                                    cleansEffect = true; // If the target has the effect, it can be cleansed
                            }
                            else
                            {
                                cleansEffect = true;
                            }
                        }

                        if (!cleansEffect)
                        {
                            if (log)
                                Debug.Log($"[RaidwideDebuffMechanic] Applying effect {effect.data.statusName} to {target.characterName} with tag {currentTag} and stacks {effect.stacks}");
                            // Apply the effect to the target
                            target.AddEffect(effect.data, from, false, currentTag, effect.stacks);
                        }
                        else if (cleansEffect)
                        {
                            if (log)
                                Debug.Log($"[RaidwideDebuffMechanic] Removing effect {effect.data.statusName} from {target.characterName} with tag {currentTag} and stacks {effect.stacks}");
                            // Remove the effect to the target
                            target.RemoveEffect(effect.data, false, from, currentTag, effect.stacks);
                        }
                    }

                    if (incrementalTag)
                        currentTag++;

                    // Remove the effect and player from the possible options
                    partyMembers.Remove(target);
                    i--;
                }

                if (startingCleanseEnabled)
                    startingCleanseEnabled = false; // Reset the starting cleanse state after the first mechanic execution

                if (togglesEffect)
                {
                    onToggleEffect.Invoke(!cleansEffect);
                }
            }
            else if (actionInfo.source != null)
            {
                if (killsEffect)
                {
                    if (actionInfo.source.HasEffect(effect.data.statusName, effect.tag))
                    {
                        actionInfo.source.ModifyHealth(new Damage(100, true, true, Damage.DamageType.unique, Damage.ElementalAspect.unaspected, Damage.PhysicalAspect.none, Damage.DamageApplicationType.percentageFromMax, Utilities.InsertSpaceBeforeCapitals(effect.data.statusName)), true);
                    }
                }

                if ((!cleansEffect || (togglesEffect && !from.HasEffect(effect.data.statusName, effect.tag))) && !killsEffect)
                {
                    // Apply the effect to the target
                    actionInfo.source.AddEffect(effect.data, from, false, effect.tag, effect.stacks);
                }
                if ((cleansEffect || (togglesEffect && from.HasEffect(effect.data.statusName, effect.tag))) && !killsEffect)
                {
                    // Remove the effect to the target
                    actionInfo.source.RemoveEffect(effect.data, false, from, effect.tag, effect.stacks);
                }
            }
        }

        private CharacterState FindSuitableTarget(StatusEffectData effect, List<CharacterState> candidates)
        {
            foreach (Role role in effect.assignedRoles)
            {
                // Create a copy of the candidates list
                List<CharacterState> candidatesCopy = new List<CharacterState>(candidates);

                // Iterate through the copy of candidates
                for (int i = 0; i < candidatesCopy.Count; i++)
                {
                    var candidate = candidatesCopy[i];
                    if (candidate.role == role)
                    {
                        candidates.Remove(candidate); // Remove the candidate from the original list
                        return candidate; // Return the suitable candidate
                    }
                }
            }
            return null; // No suitable candidate found
        }
    }
}