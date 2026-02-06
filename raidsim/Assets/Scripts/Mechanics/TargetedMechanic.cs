// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using NaughtyAttributes;
using dev.susybaka.raidsim.Characters;
using dev.susybaka.raidsim.Core;
using dev.susybaka.raidsim.UI;
using static dev.susybaka.raidsim.Core.GlobalData;
using static dev.susybaka.raidsim.StatusEffects.StatusEffectData;
using Random = UnityEngine.Random;

namespace dev.susybaka.raidsim.Mechanics
{
    public class TargetedMechanic : FightMechanic
    {
        public enum TargetingType { None, Nearest, Furthest, LowestHealth, HighestHealth, StatusEffect, Role, FullParty, Random, PreDefined }

        public CharacterState overrideSource = null;
        [HideIf(nameof(noParty))] public PartyList party;
        [ShowIf(nameof(noParty))] public List<CharacterState> targetList = new List<CharacterState>();
        [HideIf(nameof(noParty))] public bool autoFindParty = false;
        public bool noParty = false;
        public bool skipSelf = false;
        public TargetingType m_type = TargetingType.None;
        [HideIf(nameof(_hideAmountOfTargets))] public int amountOfTargets = 1;
        [ShowIf(nameof(m_type), TargetingType.StatusEffect)] public StatusEffectInfo effect;
        [ShowIf(nameof(m_type), TargetingType.Role)] public List<RoleSelection> availableRoles = new List<RoleSelection>();
        [ShowIf(nameof(m_type), TargetingType.Role)] public bool pickRandomSubList = false;
        [ShowIf(nameof(m_type), TargetingType.Role)] public bool pickEquallyFromAllSubLists = false;
        [ShowIf(nameof(_showFallbackToRandom))] public bool fallBackToRandom = false;
        [ShowIf(nameof(m_type), TargetingType.StatusEffect)] public bool pickAllWithEffect = false;
        [ShowIf(nameof(m_type), TargetingType.PreDefined)] public List<CharacterState> preDefinedTargets = new List<CharacterState>();
        [ShowIf(nameof(_showDynamicTargets))] public bool allowDynamicTargets = false;
        [ShowIf(nameof(m_type), TargetingType.PreDefined)] public bool targetAllPreDefined = true;
        public bool makeSourceFaceTarget = false;
        [Min(-1)] public int baseFightTimelineEventId = -1;
        [ShowIf(nameof(_showSubListSave))] public bool saveSubListPickInstead = false;
        public FightMechanic resultingMechanic;

        private List<CharacterState> originalTargetList = new List<CharacterState>();
        private List<CharacterState> originalPreDefinedTargets = new List<CharacterState>();
        private List<CharacterState> lastCandidates = new List<CharacterState>();

        private bool _showFallbackToRandom => (m_type == TargetingType.StatusEffect || m_type == TargetingType.Role);
        private bool _hideAmountOfTargets => (m_type == TargetingType.FullParty || (m_type == TargetingType.PreDefined && targetAllPreDefined) || pickAllWithEffect);
        private bool _showDynamicTargets => (m_type == TargetingType.PreDefined || (m_type == TargetingType.Role && noParty));
        private bool _showSubListSave => (m_type == TargetingType.Role && pickRandomSubList);

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (m_type != TargetingType.StatusEffect)
                pickAllWithEffect = false;
            if (noParty)
                autoFindParty = false;
            if (m_type != TargetingType.Role)
            {
                pickRandomSubList = false;
                pickEquallyFromAllSubLists = false;
                saveSubListPickInstead = false;
            }
            if (baseFightTimelineEventId < 0 || pickEquallyFromAllSubLists)
            {
                saveSubListPickInstead = false;
            }
        }
#endif

        private void Awake()
        {
            if (noParty)
                originalTargetList = new List<CharacterState>(targetList);
            originalPreDefinedTargets = new List<CharacterState>(preDefinedTargets);
            lastCandidates = new List<CharacterState>();

            if (autoFindParty && FightTimeline.Instance != null)
            {
                party = FightTimeline.Instance.partyList;
            }
        }

        public override void TriggerMechanic(ActionInfo actionInfo)
        {
            if (!CanTrigger(actionInfo) || party == null)
                return;

            if (overrideSource != null)
            {
                actionInfo = new ActionInfo(actionInfo.action, overrideSource, actionInfo.target);
            }

            List<CharacterState> candidates = new List<CharacterState>();

            Transform sourceTransform = actionInfo.source != null ? actionInfo.source.transform : transform;

            if (log)
            {
                if (sourceTransform != actionInfo.source?.transform)
                {
                    Debug.Log($"[TargetedMechanic ({gameObject.name})] actionInfo source is null, using mechanic transform as source instead.");
                }
            }

            candidates.Clear();
            if (noParty)
            {
                candidates = new List<CharacterState>(targetList);
            }
            else
            {
                candidates = party.GetActiveMembers();
            }

            lastCandidates = new List<CharacterState>(candidates);

            switch (m_type)
            {
                case TargetingType.Nearest:
                    candidates = Filter(actionInfo, candidates);

                    // Check distance of each candidate to source
                    candidates.Sort((a, b) =>
                        Vector3.Distance(sourceTransform.position, a.transform.position)
                        .CompareTo(Vector3.Distance(sourceTransform.position, b.transform.position)));

                    ExecuteOnTargets(actionInfo, candidates);
                    break;
                case TargetingType.Furthest:
                    candidates = Filter(actionInfo, candidates);

                    // Sort distance descending (furthest first)
                    candidates.Sort((a, b) =>
                        Vector3.Distance(sourceTransform.position, b.transform.position)
                        .CompareTo(Vector3.Distance(sourceTransform.position, a.transform.position)));

                    ExecuteOnTargets(actionInfo, candidates);
                    break;
                case TargetingType.LowestHealth:
                    candidates = Filter(actionInfo, candidates)
                                      .OrderBy(c => c.health)
                                      .ToList();

                    ExecuteOnTargets(actionInfo, candidates);
                    break;
                case TargetingType.HighestHealth:
                    candidates = Filter(actionInfo, candidates)
                                      .OrderByDescending(c => c.health)
                                      .ToList();

                    ExecuteOnTargets(actionInfo, candidates);
                    break;
                case TargetingType.StatusEffect:
                    candidates = Filter(actionInfo, candidates);

                    List<CharacterState> sorted = candidates;

                    if (pickAllWithEffect)
                    {
                        for (int i = sorted.Count - 1; i >= 0; i--)
                        {
                            if (!sorted[i].HasAnyVersionOfEffect(effect.data.statusName))
                            {
                                sorted.RemoveAt(i);
                            }
                        }

                        amountOfTargets = sorted.Count;
                    }
                    else
                    {
                        // Sort candidates by whether they have the status effect, then randomly within each group
                        sorted = candidates.OrderByDescending(c => c.HasAnyVersionOfEffect(effect.data.statusName)).ThenBy(c => Random.value).ToList();
                    }

                    if (fallBackToRandom && sorted.Count < amountOfTargets)
                        sorted = FillWithRandom(sorted, candidates, amountOfTargets);

                    ExecuteOnTargets(actionInfo, sorted);
                    break;
                case TargetingType.Role:
                    int subListIndex = 0;

                    candidates = Filter(actionInfo, candidates);

                    if (availableRoles == null || availableRoles.Count <= 0)
                    {
                        Debug.LogError("No roles specified for Role targeting type.");
                        return;
                    }

                    List<CharacterState> filtered = new List<CharacterState>();

                    if (pickEquallyFromAllSubLists)
                    {
                        subListIndex = -1; // Indicate that we're picking equally from all lists

                        // Calculate how many to pick from each list
                        int perList = amountOfTargets / availableRoles.Count;
                        int remainder = amountOfTargets % availableRoles.Count;

                        // Pick from each role list
                        for (int i = 0; i < availableRoles.Count; i++)
                        {
                            // Get candidates matching this role list
                            List<CharacterState> roleCandidates = candidates.Where(c => availableRoles[i].roles.Contains(c.role)).ToList();

                            // Determine how many to pick from this list
                            // First 'remainder' lists get an extra pick
                            int countToPickFromThisList = perList + (i < remainder ? 1 : 0);

                            // Randomly pick the required amount from this role list
                            var picked = roleCandidates.OrderBy(c => Random.value).Take(countToPickFromThisList).ToList();

                            filtered.AddRange(picked);
                        }
                    }
                    else if (pickRandomSubList)
                    {
                        subListIndex = Random.Range(0, availableRoles.Count);
                        filtered = candidates.Where(c => availableRoles[subListIndex].roles.Contains(c.role)).ToList();
                    }
                    else
                    {
                        filtered = candidates.Where(c => availableRoles[subListIndex].roles.Contains(c.role)).ToList();
                    }

                    if (baseFightTimelineEventId > -1 && saveSubListPickInstead && FightTimeline.Instance != null)
                    {
                        FightTimeline.Instance.SetRandomEventResult(baseFightTimelineEventId, subListIndex);
                    }

                    if (fallBackToRandom && filtered.Count < amountOfTargets)
                        filtered = FillWithRandom(filtered, candidates, amountOfTargets);

                    ExecuteOnTargets(actionInfo, filtered);
                    break;
                case TargetingType.FullParty:
                    candidates = Filter(actionInfo, candidates);

                    amountOfTargets = candidates.Count; // Ensure we target all members in the party

                    if (candidates.Count < amountOfTargets)
                    {
                        Debug.LogWarning("Not enough party members to fill the target count. Falling back to random selection.");
                        candidates = GetRandomCandidates(candidates);
                    }

                    ExecuteOnTargets(actionInfo, candidates);
                    break;
                case TargetingType.Random:
                    candidates = Filter(actionInfo, candidates);
                    candidates = GetRandomCandidates(candidates);
                    ExecuteOnTargets(actionInfo, candidates);
                    break;
                case TargetingType.PreDefined:
                    candidates = new List<CharacterState>(preDefinedTargets);
                    if (targetAllPreDefined)
                        amountOfTargets = candidates.Count; // Ensure we target all predefined targets if specified
                    candidates = Filter(actionInfo, candidates);
                    ExecuteOnTargets(actionInfo, candidates);
                    break;
                default:
                    Debug.LogError("Targeting type not implemented: " + m_type);
                    break;
            }
        }

        private List<CharacterState> GetRandomCandidates(List<CharacterState> candidates)
        {
            if (candidates == null || candidates.Count == 0 || amountOfTargets <= 0)
                return new List<CharacterState>();

            List<CharacterState> result = new List<CharacterState>(amountOfTargets);
            List<CharacterState> shuffled = candidates.OrderBy(c => Random.value).ToList();

            int fullSets = amountOfTargets / shuffled.Count;
            int remaining = amountOfTargets % shuffled.Count;

            for (int i = 0; i < fullSets; i++)
                result.AddRange(shuffled);

            if (remaining > 0)
                result.AddRange(shuffled.Take(remaining));

            return result.OrderBy(c => Random.value).ToList();
        }

        private List<CharacterState> FillWithRandom(List<CharacterState> currentList, List<CharacterState> pool, int targetCount)
        {
            if (currentList.Count >= targetCount)
                return currentList;

            // Get remaining candidates that aren't already in currentList
            var remaining = pool.Except(currentList).ToList();

            // Shuffle and take the difference
            var fill = remaining.OrderBy(c => Random.value).Take(targetCount - currentList.Count).ToList();

            currentList.AddRange(fill);
            return currentList;
        }

        private void ExecuteOnTargets(ActionInfo actionInfo, List<CharacterState> candidates)
        {
            // Select the first amountOfTargets candidates to a final list
            List<CharacterState> finalTargets = candidates.Take(amountOfTargets).ToList();

#if UNITY_EDITOR
            if (log)
                Debug.Log($"[TargetedMechanic ({gameObject.name})] finalTargets: '{finalTargets.Count}'");
#endif

            int index = 0;
            // Trigger mechanic with each candidate in final list
            foreach (CharacterState target in finalTargets)
            {
#if UNITY_EDITOR
                if (log)
                    Debug.Log($"[TargetedMechanic ({gameObject.name})] executing resulting mechanic for target: '{target.gameObject.name}' {finalTargets.IndexOf(target) + 1}/{finalTargets.Count}");
#endif
                resultingMechanic.TriggerMechanic(new ActionInfo(actionInfo.action, actionInfo.source, target));

                if (baseFightTimelineEventId > -1 && !saveSubListPickInstead && FightTimeline.Instance != null)
                {
                    if (lastCandidates.Contains(target))
                    {
                        FightTimeline.Instance.SetRandomEventResult(baseFightTimelineEventId + index, lastCandidates.IndexOf(target));
                    }
                }

                if (makeSourceFaceTarget && actionInfo.source != null)
                {
                    actionInfo.source.transform.LookAt(target.transform);
                    actionInfo.source.transform.eulerAngles = new Vector3(0f, actionInfo.source.transform.eulerAngles.y, 0f);
                }
                else if (log && makeSourceFaceTarget && actionInfo.source == null)
                {
                    Debug.LogWarning($"[TargetedMechanic ({gameObject.name})] makeSourceFaceTarget is true but actionInfo.source is null. Unable to rotate source.");
                }
                index++;
            }
        }

        private List<CharacterState> Filter(ActionInfo actionInfo, List<CharacterState> candidates)
        {
            if (skipSelf && (actionInfo.source != null || actionInfo.target != null))
            {
                CharacterState self = actionInfo.source != null ? actionInfo.source : actionInfo.target;
                if (self != null)
                {
                    for (int i = candidates.Count - 1; i >= 0; i--)
                    {
                        if (candidates[i] == self)
                        {
                            candidates.RemoveAt(i);
                            break;
                        }
                    }
                }
            }
            return candidates;
        }

        #region Dynamic Characters
        public void AddActionSourceCharacter(ActionInfo actionInfo)
        {
            if (actionInfo.source == null)
            {
                if (log)
                    Debug.Log($"[TargetedMechanic ({gameObject.name})] Attempted to add a null CharacterState, skipping.");
                return;
            }

            AddCharacter(actionInfo.source);
        }

        public void AddActionTargetCharacter(ActionInfo actionInfo)
        {
            if (actionInfo.target == null)
            {
                if (log)
                    Debug.Log($"[TargetedMechanic ({gameObject.name})] Attempted to add a null CharacterState, skipping.");
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
            if (m_type != TargetingType.PreDefined && !noParty)
            {
                if (log)
                    Debug.Log($"[TargetedMechanic ({gameObject.name})] Targeting type is not PreDefined, skipping addition of character to pre defined target list.");
                return;
            }

            if (character == null)
            {
                if (log)
                    Debug.Log($"[TargetedMechanic ({gameObject.name})] Attempted to add a null CharacterState, skipping.");
                return;
            }

            if (!allowDynamicTargets)
            {
                if (log)
                    Debug.Log($"[TargetedMechanic ({gameObject.name})] Dynamic characters are not allowed, skipping addition of {character.characterName} ({character.gameObject.name}).");
                return;
            }

            if (m_type == TargetingType.PreDefined)
            {
                if (preDefinedTargets.Contains(character))
                {
                    if (log)
                        Debug.Log($"[TargetedMechanic ({gameObject.name})] CharacterState {character.characterName} ({character.gameObject.name}) is already in the list, skipping.");
                    return;
                }

                preDefinedTargets.Add(character);
            }
            else if (noParty)
            {
                if (targetList.Contains(character))
                {
                    if (log)
                        Debug.Log($"[TargetedMechanic ({gameObject.name})] CharacterState {character.characterName} ({character.gameObject.name}) is already in the target list, skipping.");
                    return;
                }

                targetList.Add(character);
            }
        }

        public void RemoveActionSourceCharacter(ActionInfo actionInfo)
        {
            if (actionInfo.source == null)
            {
                if (log)
                    Debug.Log($"[TargetedMechanic ({gameObject.name})] Attempted to remove a null CharacterState, skipping.");
                return;
            }

            RemoveCharacter(actionInfo.source);
        }

        public void RemoveActionTargetCharacter(ActionInfo actionInfo)
        {
            if (actionInfo.target == null)
            {
                if (log)
                    Debug.Log($"[TargetedMechanic ({gameObject.name})] Attempted to remove a null CharacterState, skipping.");
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
            if (m_type != TargetingType.PreDefined && !noParty)
            {
                if (log)
                    Debug.Log($"[TargetedMechanic ({gameObject.name})] Targeting type is not PreDefined, skipping removal of character from pre defined target list.");
                return;
            }

            if (character == null)
            {
                if (log)
                    Debug.Log($"[TargetedMechanic ({gameObject.name})] Attempted to remove a null CharacterState, skipping.");
                return;
            }

            if (!allowDynamicTargets)
            {
                if (log)
                    Debug.Log($"[TargetedMechanic ({gameObject.name})] Dynamic characters are not allowed, skipping removal of {character.characterName} ({character.gameObject.name}).");
                return;
            }

            if (m_type == TargetingType.PreDefined)
            {
                if (!preDefinedTargets.Contains(character))
                {
                    if (log)
                        Debug.Log($"[TargetedMechanic ({gameObject.name})] CharacterState {character.characterName} ({character.gameObject.name}) is not present in the list, skipping.");
                    return;
                }

                preDefinedTargets.Remove(character);
            }
            else if (noParty)
            {
                if (!targetList.Contains(character))
                {
                    if (log)
                        Debug.Log($"[TargetedMechanic ({gameObject.name})] CharacterState {character.characterName} ({character.gameObject.name}) is not present in the target list, skipping.");
                    return;
                }

                targetList.Remove(character);
            }
        }

        public void ResetPreDefinedTargets()
        {
            if (m_type != TargetingType.PreDefined)
            {
                if (log)
                    Debug.Log($"[TargetedMechanic ({gameObject.name})] Targeting type is not PreDefined, skipping reset of pre defined target list.");
                return;
            }

            if (!allowDynamicTargets)
            {
                if (log)
                    Debug.Log($"[TargetedMechanic ({gameObject.name})] Dynamic targets are not allowed, skipping reset of pre defined target list.");
                return;
            }

            preDefinedTargets = new List<CharacterState>(originalPreDefinedTargets);
        }

        public void ResetTargets()
        {
            if (m_type == TargetingType.PreDefined)
            {
                ResetPreDefinedTargets();
            }
            else
            {
                if (!noParty)
                {
                    if (log)
                        Debug.Log($"[TargetedMechanic ({gameObject.name})] Targeting type is not set to 'noParty', skipping reset of target list.");
                }

                if (!allowDynamicTargets)
                {
                    if (log)
                        Debug.Log($"[TargetedMechanic ({gameObject.name})] Dynamic targets are not allowed, skipping reset of pre defined target list.");
                    return;
                }

                targetList = new List<CharacterState>(originalTargetList);
            }
        }
        #endregion
    }
}