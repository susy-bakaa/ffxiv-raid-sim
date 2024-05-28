using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static ActionController;
using static StatusEffectData;
using static UnityEngine.GraphicsBuffer;

public class RaidwideDebuffsMechanic : FightMechanic
{
    public PartyList party;
    public List<StatusEffectInfo> effects = new List<StatusEffectInfo>();
    public StatusEffectInfo playerEffect;
    public bool ignoreRoles = true;
    public bool cleansEffects = false;

    List<StatusEffectInfo> statusEffects;
    List<CharacterState> partyMembers;

    CharacterState player;

    void Awake()
    {
        for (int i = 0; i < party.members.Count; i++)
        {
            if (party.members[i].characterName.ToLower().Contains("player"))
            {
                player = party.members[i];
            }
        }
    }

    public override void TriggerMechanic(ActionInfo action)
    {
        statusEffects = new List<StatusEffectInfo>(effects); // Copy the effects list
        partyMembers = new List<CharacterState>(party.GetActiveMembers()); // Copy the party members list
        partyMembers.Shuffle();
        statusEffects.Shuffle();

        if (playerEffect.data != null && player != null)
        {
            if (statusEffects.Contains(playerEffect))
            {
                statusEffects.Remove(playerEffect);
                partyMembers.Remove(player);
                player.AddEffect(playerEffect.data, playerEffect.tag);
            }
        }

        // Iterate through each status effect
        for (int i = 0; i < statusEffects.Count; i++)
        {
            // Does this clean the specified status effects or inflict them?
            if (!cleansEffects)
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
            }
            else
            {
                // Loop through each member and remove this status effect from them if they have it.
                for (int m = 0; m < partyMembers.Count; m++)
                {
                    if (partyMembers[m].HasEffect(statusEffects[i].data.statusName, statusEffects[i].tag))
                    {
                        partyMembers[m].RemoveEffect(statusEffects[i].data, false, statusEffects[i].tag);
                    }
                }
            }
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
