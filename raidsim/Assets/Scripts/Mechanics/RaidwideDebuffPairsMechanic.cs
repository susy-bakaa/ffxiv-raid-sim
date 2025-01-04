using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static GlobalData;
using static StatusEffectData;

public class RaidwideDebuffPairsMechanic : FightMechanic
{
    public PartyList party;
    public List<StatusEffectInfoArray> effects = new List<StatusEffectInfoArray>();
    public StatusEffectInfoArray playerEffect;
    public bool ignoreRoles = true;
    public bool cleansEffects = false;

    List<StatusEffectInfoArray> statusEffects;
    List<CharacterState> partyMembers;

    CharacterState player;

    void Awake()
    {
        for (int i = 0; i < party.members.Count; i++)
        {
            if (party.members[i].characterState.characterName.ToLower().Contains("player"))
            {
                player = party.members[i].characterState;
            }
        }
    }

#if UNITY_EDITOR
    public void OnValidate()
    {
        for (int i = 0; i < effects.Count; i++)
        {
            if (effects[i].effectInfos[0].data != null)
            {
                StatusEffectInfoArray array = effects[i];
                array.name = effects[i].effectInfos[0].name;
                effects[i] = array;
            }
        }
    }
#endif
    public override void TriggerMechanic(ActionInfo actionInfo)
    {
        if (!CanTrigger(actionInfo))
            return;

        CharacterState from = null;

        if (actionInfo.source != null)
            from = actionInfo.source;
        else if (actionInfo.target != null)
            from = actionInfo.target;

        statusEffects = new List<StatusEffectInfoArray>(effects); // Copy the effects list
        partyMembers = new List<CharacterState>(party.GetActiveMembers()); // Copy the party members list
        partyMembers.Shuffle();
        statusEffects.Shuffle();

        if (!string.IsNullOrEmpty(playerEffect.name) && playerEffect.effectInfos.Length > 0 && player != null)
        {
            if (statusEffects.ContainsInfoPair(playerEffect))
            {
                statusEffects.RemoveInfoPair(playerEffect);
                partyMembers.Remove(player);
                for (int i = 0; i < playerEffect.effectInfos.Length; i++)
                {
                    player.AddEffect(playerEffect.effectInfos[i].data, from, false, playerEffect.effectInfos[i].tag, playerEffect.effectInfos[i].stacks);
                }
            }
        }

        // Iterate through each status effect
        for (int i = 0; i < statusEffects.Count; i++)
        {
            // Does this clean the specified status effects or inflict them?
            if (!cleansEffects)
            {
                // Find a suitable party member for the effect
                CharacterState target = FindSuitableTarget(statusEffects[i].effectInfos[0], partyMembers);

                // If no suitable target found, apply to a random member
                if (target == null)
                    target = partyMembers[Random.Range(0, partyMembers.Count)];

                // Apply the effects to the target
                for (int j = 0; j < statusEffects[i].effectInfos.Length; j++)
                {
                    target.AddEffect(statusEffects[i].effectInfos[j].data, from, false, statusEffects[i].effectInfos[j].tag, statusEffects[i].effectInfos[j].stacks);
                }

                // Remove the effect and player from the possible options
                partyMembers.Remove(target);
            }
            else
            {
                // Loop through each member and remove this status effect from them if they have it.
                for (int m = 0; m < partyMembers.Count; m++)
                {
                    for (int j = 0; j < statusEffects[i].effectInfos.Length; j++)
                    {
                        if (partyMembers[m].HasEffect(statusEffects[i].effectInfos[j].data.statusName, statusEffects[i].effectInfos[j].tag))
                        {
                            partyMembers[m].RemoveEffect(statusEffects[i].effectInfos[j].data, false, from, statusEffects[i].effectInfos[j].tag, statusEffects[i].effectInfos[j].stacks);
                        }
                    }
                }
            }
            statusEffects.Remove(statusEffects[i]);
            i--;
        }
    }

    private CharacterState FindSuitableTarget(StatusEffectInfo effect, List<CharacterState> candidates)
    {
        foreach (Role role in effect.data.assignedRoles)
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
