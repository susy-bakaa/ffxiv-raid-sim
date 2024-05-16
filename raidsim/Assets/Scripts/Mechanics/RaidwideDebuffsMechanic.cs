using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static ActionController;
using static StatusEffectData;

public class RaidwideDebuffsMechanic : FightMechanic
{
    public PartyList party;
    public List<StatusEffectInfo> effects = new List<StatusEffectInfo>();

    List<StatusEffectInfo> statusEffects;
    List<CharacterState> partyMembers;

    public override void TriggerMechanic(ActionInfo action)
    {
        statusEffects = new List<StatusEffectInfo>(effects); // Copy the effects list
        partyMembers = new List<CharacterState>(party.GetActiveMembers()); // Copy the party members list
        partyMembers.Shuffle();
        statusEffects.Shuffle();

        // Iterate through each status effect
        for (int i = 0; i < statusEffects.Count; i++)
        {
            // Find a suitable party member for the effect
            CharacterState target = FindSuitableTarget(statusEffects[i], partyMembers);

            // If no suitable target found, apply to a random member
            if (target == null)
                target = partyMembers[Random.Range(0, partyMembers.Count)];

            // Apply the effect to the target
            target.AddEffect(statusEffects[i].data, statusEffects[i].tag);

            // Remove the effect and player from the possible options
            partyMembers.Remove(target);
            statusEffects.Remove(statusEffects[i]);
            i--;
        }
    }

    private CharacterState FindSuitableTarget(StatusEffectInfo effect, List<CharacterState> candidates)
    {
        foreach (CharacterState.Role role in effect.data.assignedRoles)
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
