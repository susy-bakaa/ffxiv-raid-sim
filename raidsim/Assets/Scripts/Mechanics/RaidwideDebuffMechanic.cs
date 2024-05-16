using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ActionController;
using static StatusEffectData;

public class RaidwideDebuffMechanic : FightMechanic
{
    public PartyList party;
    public StatusEffectInfo effect;
    public bool ignoreRoles = true;
    public bool cleansEffect = false;

    List<CharacterState> partyMembers;

    public override void TriggerMechanic(ActionInfo action)
    {
        partyMembers = new List<CharacterState>(party.members); // Copy the party members list
        partyMembers.Shuffle();

        // Iterate through each status effect
        for (int i = 0; i < partyMembers.Count; i++)
        {
            // Find a suitable party member for the effect
            CharacterState target = partyMembers[i];

            if (!ignoreRoles || !cleansEffect)
            {
                target = FindSuitableTarget(effect.data, partyMembers);
            }

            // If no suitable target found, apply to a random member
            if (target == null)
                target = partyMembers[Random.Range(0, partyMembers.Count)];

            if (!cleansEffect)
            {
                // Apply the effect to the target
                target.AddEffect(effect.data, effect.tag);
            }
            else
            {
                // Remove the effect to the target
                target.RemoveEffect(effect.data, false, effect.tag);
            }

            // Remove the effect and player from the possible options
            partyMembers.Remove(target);
            i--;
        }
    }

    private CharacterState FindSuitableTarget(StatusEffectData effect, List<CharacterState> candidates)
    {
        foreach (CharacterState.Role role in effect.assignedRoles)
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
