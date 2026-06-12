// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using System.Collections.Generic;
using UnityEngine;
using dev.susybaka.raidsim.Characters;
using dev.susybaka.raidsim.UI;
using dev.susybaka.Shared;
using static dev.susybaka.raidsim.Core.GlobalData;
using static dev.susybaka.raidsim.StatusEffects.StatusEffectData;
using NaughtyAttributes;

namespace dev.susybaka.raidsim.Mechanics
{
    public class RaidwideDebuffPairsRandomMechanic : FightMechanic
    {
        public PartyList party;
        public StatusEffectContextArrayRandom playerEffect;
        public List<StatusEffectContextArrayRandom> effects = new List<StatusEffectContextArrayRandom>();
        [HideIf(nameof(ignoreRoles))] public RoleSelection roles;
        public bool ignoreRoles = true;
        public bool cleansEffects = false;

        List<StatusEffectContextArrayRandom> statusEffects;
        List<CharacterState> partyMembers;
        private readonly List<CharacterState> _candidatesCopy = new List<CharacterState>();

        CharacterState player;

        private void Awake()
        {
            for (int i = 0; i < party.members.Count; i++)
            {
                if (party.members[i].characterState.characterName.ToLower().Contains("player"))
                {
                    player = party.members[i].characterState;
                }
            }
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

            statusEffects = new List<StatusEffectContextArrayRandom>(effects); // Copy the effects list
            partyMembers = new List<CharacterState>(party.GetActiveMembers()); // Copy the party members list

            if (!ignoreRoles && roles.roles != null && roles.roles.Count > 0)
                partyMembers = partyMembers.FindAll(member => roles.roles.Contains(member.role));

            partyMembers.ShufflePCG(timeline.random.Stream($"{GetUniqueName()}_Shuffle_PartyMembersList"));
            statusEffects.ShufflePCG(timeline.random.Stream($"{GetUniqueName()}_Shuffle_StatusEffectsList"));

            //throw new System.Exception("Temp");

            if (!string.IsNullOrEmpty(playerEffect.name) && playerEffect.effectArrays.Length > 0 && player != null)
            {
                if (statusEffects.ContainsInfoPair(playerEffect))
                {
                    statusEffects.RemoveInfoPair(playerEffect);
                    partyMembers.Remove(player);

                    StatusEffectContextArray effectContexts;

                    if (playerEffect.random)
                    {
                        effectContexts = playerEffect.effectArrays[timeline.random.Pick($"{GetUniqueName()}_Pick_PlayerEffectContextArray", playerEffect.effectArrays.Length, timeline.GlobalRngMode)];
                    }
                    else
                    {
                        effectContexts = playerEffect.effectArrays[0];
                    }

                    for (int i = 0; i < effectContexts.effectInfos.Length; i++)
                    {
                        player.AddEffect(effectContexts.effectInfos[i].data, from, false, effectContexts.effectInfos[i].tag, effectContexts.effectInfos[i].stacks);
                    }
                }
            }

            // Iterate through each status effect
            for (int i = 0; i < statusEffects.Count; i++)
            {
                StatusEffectContextArray effectContexts;

                if (statusEffects[i].random)
                {
                    effectContexts = statusEffects[i].effectArrays[timeline.random.Pick($"{GetUniqueName()}_Pick_StatusEffectContextArray_{i}", statusEffects[i].effectArrays.Length, timeline.GlobalRngMode)];
                }
                else
                {
                    effectContexts = statusEffects[i].effectArrays[0];
                }

                // Does this clean the specified status effects or inflict them?
                if (!cleansEffects)
                {
                    // Find a suitable party member for the effect
                    CharacterState target = FindSuitableTarget(effectContexts.effectInfos[0], partyMembers);

                    if (log)
                        Debug.Log($"Status Effect '{effectContexts.effectInfos[0].data.statusName}' with tag '{effectContexts.effectInfos[0].tag}' is being applied. Suitable target found: {(target != null ? target.characterName : "None")}, index {i}");

                    // If no suitable target found, apply to a random member
                    if (target == null && partyMembers.Count > 0)
                        target = partyMembers[timeline.random.Pick($"{GetUniqueName()}_RandomTarget", partyMembers.Count, timeline.GlobalRngMode)]; // Random.Range(0, partyMembers.Count)
                    else if (partyMembers.Count == 0)
                    {
                        Debug.LogWarning($"No valid targets found for status effect '{effectContexts.effectInfos[0].data.statusName}' with tag '{effectContexts.effectInfos[0].tag}'. Skipping this effect.");
                        continue; // No targets available, skip to the next effect
                    }

                    // Apply the effects to the target
                    for (int j = 0; j < effectContexts.effectInfos.Length; j++)
                    {
                        target.AddEffect(effectContexts.effectInfos[j].data, from, false, effectContexts.effectInfos[j].tag, effectContexts.effectInfos[j].stacks);
                    }

                    // Remove the effect and player from the possible options
                    partyMembers.Remove(target);
                }
                else
                {
                    // Loop through each member and remove this status effect from them if they have it.
                    for (int m = 0; m < partyMembers.Count; m++)
                    {
                        for (int j = 0; j < effectContexts.effectInfos.Length; j++)
                        {
                            if (partyMembers[m].HasEffect(effectContexts.effectInfos[j].data.statusName, effectContexts.effectInfos[j].tag))
                            {
                                partyMembers[m].RemoveEffect(effectContexts.effectInfos[j].data, false, from, effectContexts.effectInfos[j].tag, effectContexts.effectInfos[j].stacks);
                            }
                        }
                    }
                }
                statusEffects.Remove(statusEffects[i]);
                i--;
            }
        }

        protected override bool UsesPCG()
        {
            return true;
        }

        private CharacterState FindSuitableTarget(StatusEffectContext effect, List<CharacterState> candidates)
        {
            foreach (Role role in effect.data.assignedRoles)
            {
                // Create a copy of the candidates list
                _candidatesCopy.Clear();
                if (_candidatesCopy.Capacity < candidates.Count)
                    _candidatesCopy.Capacity = candidates.Count;
                _candidatesCopy.AddRange(candidates);

                // Iterate through the copy of candidates
                for (int i = 0; i < _candidatesCopy.Count; i++)
                {
                    var candidate = _candidatesCopy[i];
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
