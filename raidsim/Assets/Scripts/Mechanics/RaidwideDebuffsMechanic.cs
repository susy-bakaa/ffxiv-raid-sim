using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static ActionController;

public class RaidwideDebuffsMechanic : MonoBehaviour
{
    public PartyList party;
    public List<StatusEffectData> effects = new List<StatusEffectData>();

    List<StatusEffectData> statusEffects;
    List<CharacterState> partyMembers;

    public void SpreadDebuffs(ActionInfo action)
    {
        statusEffects = new List<StatusEffectData>(effects); // Copy the effects list
        partyMembers = new List<CharacterState>(party.members); // Copy the party members list
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
            target.AddEffect(statusEffects[i]);

            // Remove the effect and player from the possible options
            partyMembers.Remove(target);
            statusEffects.Remove(statusEffects[i]);
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
