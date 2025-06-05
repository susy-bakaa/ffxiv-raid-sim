using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.UIElements;
using static GlobalData;
using static StatusEffectData;

public class RaidwideDebuffsMechanic : FightMechanic
{
    public PartyList party;
    public List<StatusEffectInfo> effects = new List<StatusEffectInfo>();
    public StatusEffectInfo playerEffect;
    [HideIf("onlySpecifiedRoles")] public bool ignoreRoles = true;
    [HideIf("ignoreRoles")] public bool onlySpecifiedRoles = false;
    [ShowIf("onlySpecifiedRoles")] public List<RoleSelection> specifiedRoles = new List<RoleSelection>();
    [ShowIf("onlySpecifiedRoles")] public bool randomizeRoleGroups = false;
    [ShowIf("showPickBasedOnPreviousRandomEventResultField")] public bool pickBasedOnPreviousRandomEventResult = false;
    [ShowIf("pickBasedOnPreviousRandomEventResult")] public bool useIndexMapping = false;
    [ShowIf("useIndexMapping")] public List<IndexMapping> indexMapping = new List<IndexMapping>();
    [ShowIf("randomizeRoleGroups")] public bool setRandomEventResult = false;
    [ShowIf("showRandomEventIdField")] public int randomEventResultId = -1;
    public bool cleansEffects = false;
    public bool fallbackToRandom = false;
    private bool showRandomEventIdField => (pickBasedOnPreviousRandomEventResult || setRandomEventResult);
    private bool showPickBasedOnPreviousRandomEventResultField => (!randomizeRoleGroups && onlySpecifiedRoles);

    List<StatusEffectInfo> statusEffects;
    List<CharacterState> partyMembers;

    CharacterState player;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (!randomizeRoleGroups)
        {
            setRandomEventResult = false;
        }
        else if (randomizeRoleGroups)
        {
            pickBasedOnPreviousRandomEventResult = false;
            useIndexMapping = false;
        }   
        if (pickBasedOnPreviousRandomEventResult)
        {
            setRandomEventResult = false;
            randomizeRoleGroups = false;
        }
        else if (!pickBasedOnPreviousRandomEventResult)
        {
            useIndexMapping = false;
        }
    }
#endif

    void Awake()
    {
        if (randomizeRoleGroups)
            pickBasedOnPreviousRandomEventResult = false;

        for (int i = 0; i < party.members.Count; i++)
        {
            if (party.members[i].characterState.characterName.ToLower().Contains("player") && party.members[i].characterState.gameObject.CompareTag("Player"))
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

        statusEffects = new List<StatusEffectInfo>(effects); // Copy the effects list
        partyMembers = new List<CharacterState>(party.GetActiveMembers()); // Copy the party members list

        if (onlySpecifiedRoles && ((playerEffect.data == null && !playerEffect.name.Contains('!')) || pickBasedOnPreviousRandomEventResult))
        {
            if (specifiedRoles != null)
            {
                if (randomizeRoleGroups)
                    specifiedRoles.Shuffle();

                int selected = 0;

                if (pickBasedOnPreviousRandomEventResult && randomEventResultId >= 0 && FightTimeline.Instance != null)
                {
                    if (FightTimeline.Instance.TryGetRandomEventResult(randomEventResultId, out selected))
                    {
                        if (useIndexMapping && indexMapping != null && indexMapping.Count > 0)
                        {
                            if (log)
                                Debug.Log($"Using previous random event result ID {randomEventResultId}, selected index {selected}!");

                            for (int i = 0; i < indexMapping.Count; i++)
                            {
                                if (indexMapping[i].previousIndex == selected)
                                {
                                    selected = indexMapping[i].nextIndex;
                                    if (log)
                                        Debug.Log($"Using index mapping for random event result ID {randomEventResultId}, selected index {selected}!");
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        if (log)
                            Debug.LogWarning($"Failed to get random event result for ID {randomEventResultId}!");
                        if (fallbackToRandom)
                            selected = Random.Range(0, specifiedRoles.Count);
                    }
                }
                else
                {
                    selected = Random.Range(0, specifiedRoles.Count);
                }

                if (log)
                    Debug.Log($"Role group {specifiedRoles[selected].name} selected out of possible {specifiedRoles.Count}!");

                if (setRandomEventResult && randomEventResultId >= 0 && FightTimeline.Instance != null)
                    FightTimeline.Instance.SetRandomEventResult(randomEventResultId, selected);

                if (specifiedRoles.Count > 0)
                {
                    for (int i = 0; i < partyMembers.Count; i++)
                    {
                        if (log)
                            Debug.Log($"Processing party member {partyMembers[i].gameObject.name} ({i}/{specifiedRoles.Count})");
                        if (!specifiedRoles[selected].roles.Contains(partyMembers[i].role))
                        {
                            if (log)
                                Debug.Log($"Party member does not appear in currently selected role group, removing member from available pool.");
                            partyMembers.Remove(partyMembers[i]);
                            i--;
                        }
                    }
                }
                else
                {
                    Debug.LogWarning($"Not enough specified role groups found for {gameObject.name} to randomize selection!");
                }
            }
        }
        else if (onlySpecifiedRoles && playerEffect.data != null && !pickBasedOnPreviousRandomEventResult)
        {
            if (specifiedRoles != null)
            {
                if (randomizeRoleGroups)
                    specifiedRoles.Shuffle();

                RoleSelection playerRoleGroup = specifiedRoles.FirstOrDefault(group => group.roles.Contains(player.role));

                if (playerRoleGroup.roles != null)
                {
                    if (log)
                        Debug.Log($"Player's role group {playerRoleGroup.name} selected!");

                    if (setRandomEventResult && randomEventResultId >= 0 && FightTimeline.Instance != null)
                        FightTimeline.Instance.SetRandomEventResult(randomEventResultId, specifiedRoles.IndexOf(playerRoleGroup));

                    for (int i = 0; i < partyMembers.Count; i++)
                    {
                        if (!playerRoleGroup.roles.Contains(partyMembers[i].role))
                        {
                            partyMembers.Remove(partyMembers[i]);
                            i--;
                        }
                    }
                }
                else
                {
                    Debug.LogWarning($"Player's role group not found in specified roles for {gameObject.name}!");
                }
            }
        } 
        else if (onlySpecifiedRoles && playerEffect.name.Contains('!') && !pickBasedOnPreviousRandomEventResult)
        {
            if (specifiedRoles != null)
            {
                if (randomizeRoleGroups)
                    specifiedRoles.Shuffle();

                RoleSelection playerRoleGroup = specifiedRoles.FirstOrDefault(group => group.roles.Contains(player.role));

                if (playerRoleGroup.roles != null)
                {
                    if (log)
                        Debug.Log($"Player's role group {playerRoleGroup.name} found, selecting another group!");

                    List<RoleSelection> otherGroups = specifiedRoles.Where(group => group != playerRoleGroup).ToList();

                    if (otherGroups.Count > 0)
                    {
                        int selected = Random.Range(0, otherGroups.Count);

                        if (log)
                            Debug.Log($"Role group {otherGroups[selected].name} selected out of possible {otherGroups.Count}!");

                        if (setRandomEventResult && randomEventResultId >= 0 && FightTimeline.Instance != null)
                            FightTimeline.Instance.SetRandomEventResult(randomEventResultId, specifiedRoles.IndexOf(otherGroups[selected]));

                        for (int i = 0; i < partyMembers.Count; i++)
                        {
                            if (!otherGroups[selected].roles.Contains(partyMembers[i].role))
                            {
                                partyMembers.Remove(partyMembers[i]);
                                i--;
                            }
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"No other role groups found for {gameObject.name} to select!");
                    }
                }
                else
                {
                    Debug.LogWarning($"Player's role group not found in specified roles for {gameObject.name}!");
                }
            }
        }

        statusEffects.Shuffle();
        partyMembers.Shuffle();

        if (playerEffect.data != null && player != null)
        {
            if (statusEffects.Contains(playerEffect))
            {
                statusEffects.Remove(playerEffect);
                partyMembers.Remove(player);
                player.AddEffect(playerEffect.data, from, false, playerEffect.tag);
            }
        } 
        else if (player != null && !string.IsNullOrEmpty(playerEffect.name))
        {
            // Handle the case where this effect means that the player is exluded from getting an effect.
            if (playerEffect.name.Contains('!'))
            {
                if (log)
                    Debug.Log($"[FightMechanic.RaidwideDebuffsMechanic ({gameObject.name})] Player ({player.characterName}) successfully excluded from mechanic!");
                partyMembers.Remove(player);
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

                if (log)
                    Debug.Log($"FindSuitableTarget: '{target?.characterName}'");

                // If no suitable target found, apply to a random member if allowed
                if (target == null && fallbackToRandom)
                    target = partyMembers[Random.Range(0, partyMembers.Count)];

                if (log)
                    Debug.Log($"fallbackToRandomTarget: '{target?.characterName}'");

                // Make sure target is available
                if (target != null)
                {
                    if (log)
                        Debug.Log($"Applying {statusEffects[i].data.statusName} ({from.characterName}, {statusEffects[i].tag}, {statusEffects[i].stacks}) to {target.characterName}!");

                    // Apply the effect to the target
                    target.AddEffect(statusEffects[i].data, from, false, statusEffects[i].tag, statusEffects[i].stacks);

                    // Remove the effect and player from the possible options
                    partyMembers.Remove(target);
                }
                else
                {
                    Debug.LogError($"Failed to find a suitable target for {statusEffects[i].data.statusName} of index {i}!");
                }
            }
            else
            {
                // Loop through each member and remove this status effect from them if they have it.
                for (int m = 0; m < partyMembers.Count; m++)
                {
                    if (partyMembers[m].HasEffect(statusEffects[i].data.statusName, statusEffects[i].tag))
                    {
                        partyMembers[m].RemoveEffect(statusEffects[i].data, false, from, statusEffects[i].tag);
                    }
                }
            }
            statusEffects.Remove(statusEffects[i]);
            i--;
        }
    }

    private CharacterState FindSuitableTarget(StatusEffectInfo effect, List<CharacterState> candidates)
    {
        if (effect.data.assignedRoles != null && effect.data.assignedRoles.Count > 0 && !ignoreRoles)
        {
            // Create a copy of the assigned roles for this status effect
            List<Role> assignedRoles = effect.data.assignedRoles;

            // Shuffle the roles so it wont always end up picking the member with the same role that is defined first in effect.data.assignedRoles
            assignedRoles.Shuffle();

            foreach (Role role in assignedRoles)
            {
                // Create a copy of the candidates list
                List<CharacterState> candidatesCopy = new List<CharacterState>(candidates);

                // Shuffle candidates for more random behaviour as well
                candidatesCopy.Shuffle();

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
        }
        else if (ignoreRoles)
        {
            // Create a copy of all incompatable status effects
            List<StatusEffectData> incompatableEffects = effect.data.incompatableStatusEffects;

            // Create a copy of the candidates list
            List<CharacterState> candidatesCopy = new List<CharacterState>(candidates);

            // Shuffle candidates for more random behaviour as well
            candidatesCopy.Shuffle();

            // Iterate through the copy of candidates
            for (int i = 0; i < candidatesCopy.Count; i++)
            {
                var candidate = candidatesCopy[i];

                // Iterate through all of the incompatable status effects
                foreach (StatusEffectData effectData in incompatableEffects)
                {
                    // Check if this candidate has one of the incompatable effects
                    if (!candidate.HasEffect(effectData.statusName))
                    {
                        candidates.Remove(candidate); // If not, then remove the candidate from the original list
                        return candidate; // And return the suitable candidate
                    }
                }
            }
        }

        if (log)
            Debug.LogWarning($"No suitable candidate found for status effect {effect.name} from list of {candidates.Count} candidates!");
        return null; // No suitable candidate found
    }

    [System.Serializable]
    public struct RoleSelection
    {
        public string name;
        public List<Role> roles;

        public RoleSelection(string name, List<Role> roles)
        {
            this.name = name;
            this.roles = roles;
        }

        public RoleSelection(string name, Role role)
        {
            this.name = name;
            this.roles = new List<Role> { role };
        }

        public static bool operator ==(RoleSelection obj1, RoleSelection obj2)
        {
            return obj1.Equals(obj2);
        }

        public static bool operator !=(RoleSelection obj1, RoleSelection obj2)
        {
            return !obj1.Equals(obj2);
        }

        public override bool Equals(object obj)
        {
            if (obj is RoleSelection)
            {
                RoleSelection other = (RoleSelection)obj;
                return this.name == other.name && this.roles.SequenceEqual(other.roles);
            }
            return false;
        }

        public override int GetHashCode()
        {
            int hash = 17;
            hash = hash * 23 + (name != null ? name.GetHashCode() : 0);
            hash = hash * 23 + (roles != null ? roles.GetHashCode() : 0);
            return hash;
        }       
    }
}
