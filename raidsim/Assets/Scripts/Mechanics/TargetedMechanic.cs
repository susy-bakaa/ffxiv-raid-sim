using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using NaughtyAttributes;
using dev.susybaka.raidsim.Characters;
using dev.susybaka.raidsim.Core;
using dev.susybaka.raidsim.UI;
using static dev.susybaka.raidsim.Core.GlobalData;
using static dev.susybaka.raidsim.StatusEffects.StatusEffectData;

namespace dev.susybaka.raidsim.Mechanics
{
    public class TargetedMechanic : FightMechanic
    {
        public enum TargetingType { None, Nearest, Furthest, LowestHealth, HighestHealth, StatusEffect, Role, FullParty, Random }

        public PartyList party;
        public bool autoFindParty = false;
        public TargetingType m_type = TargetingType.None;
        [HideIf("m_type", TargetingType.FullParty)] public int amountOfTargets = 1;
        [ShowIf("m_type", TargetingType.StatusEffect)] public StatusEffectInfo effect;
        [ShowIf("m_type", TargetingType.Role)] public List<RoleSelection> availableRoles = new List<RoleSelection>();
        [ShowIf("m_type", TargetingType.Role)] public bool pickRandomSubList = false;
        [ShowIf("showFallbackToRandom")] public bool fallBackToRandom = false;
        public bool makeSourceFaceTarget = false;
        public FightMechanic resultingMechanic;

        private bool showFallbackToRandom => (m_type == TargetingType.StatusEffect || m_type == TargetingType.Role);

        private void Awake()
        {
            if (autoFindParty && FightTimeline.Instance != null)
            {
                party = FightTimeline.Instance.partyList;
            }
        }

        public override void TriggerMechanic(ActionInfo actionInfo)
        {
            if (!CanTrigger(actionInfo) || party == null)
                return;

            List<CharacterState> candidates = new List<CharacterState>();

            switch (m_type)
            {
                case TargetingType.Nearest:
                    candidates = party.GetActiveMembers();

                    if (actionInfo.source != null)
                    {
                        // Check distance of each candidate to source
                        candidates.Sort((a, b) =>
                            Vector3.Distance(actionInfo.source.transform.position, a.transform.position)
                            .CompareTo(Vector3.Distance(actionInfo.source.transform.position, b.transform.position)));

                        ExecuteOnTargets(actionInfo, candidates);
                    }
                    break;
                case TargetingType.Furthest:
                    candidates = party.GetActiveMembers();

                    if (actionInfo.source != null)
                    {
                        // Sort distance descending (furthest first)
                        candidates.Sort((a, b) =>
                            Vector3.Distance(actionInfo.source.transform.position, b.transform.position)
                            .CompareTo(Vector3.Distance(actionInfo.source.transform.position, a.transform.position)));

                        ExecuteOnTargets(actionInfo, candidates);
                    }
                    break;
                case TargetingType.LowestHealth:
                    candidates = party.GetActiveMembers()
                                      .OrderBy(c => c.health)
                                      .ToList();

                    ExecuteOnTargets(actionInfo, candidates);
                    break;
                case TargetingType.HighestHealth:
                    candidates = party.GetActiveMembers()
                                      .OrderByDescending(c => c.health)
                                      .ToList();

                    ExecuteOnTargets(actionInfo, candidates);
                    break;
                case TargetingType.StatusEffect:
                    candidates = party.GetActiveMembers();

                    // Sort candidates by whether they have the status effect, then randomly within each group
                    List<CharacterState> sorted = candidates.OrderByDescending(c => c.HasAnyVersionOfEffect(effect.data.statusName)).ThenBy(c => Random.value).ToList();

                    if (fallBackToRandom && sorted.Count < amountOfTargets)
                        sorted = FillWithRandom(sorted, candidates, amountOfTargets);

                    ExecuteOnTargets(actionInfo, sorted);
                    break;
                case TargetingType.Role:
                    candidates = party.GetActiveMembers();
                    int subListIndex = 0;

                    if (availableRoles == null || availableRoles.Count <= 0)
                    {
                        Debug.LogError("No roles specified for Role targeting type.");
                        return;
                    }

                    if (pickRandomSubList)
                    {
                        subListIndex = Random.Range(0, availableRoles.Count);
                    }

                    List<CharacterState> filtered = candidates.Where(c => availableRoles[subListIndex].roles.Contains(c.role)).ToList();

                    if (fallBackToRandom && filtered.Count < amountOfTargets)
                        filtered = FillWithRandom(filtered, candidates, amountOfTargets);

                    ExecuteOnTargets(actionInfo, filtered);
                    break;
                case TargetingType.FullParty:
                    candidates = party.GetActiveMembers();

                    amountOfTargets = candidates.Count; // Ensure we target all members in the party

                    if (candidates.Count < amountOfTargets)
                    {
                        Debug.LogWarning("Not enough party members to fill the target count. Falling back to random selection.");
                        candidates = GetRandomCandidates(candidates);
                    }

                    ExecuteOnTargets(actionInfo, candidates);
                    break;
                case TargetingType.Random:
                    candidates.Clear();
                    candidates = party.GetActiveMembers();
                    candidates = GetRandomCandidates(candidates);
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

            // Trigger mechanic with each candidate in final list
            foreach (CharacterState target in finalTargets)
            {
#if UNITY_EDITOR
                if (log)
                    Debug.Log($"[TargetedMechanic ({gameObject.name})] executing resulting mechanic for target: '{target.gameObject.name}' {finalTargets.IndexOf(target) + 1}/{finalTargets.Count}");
#endif
                resultingMechanic.TriggerMechanic(new ActionInfo(actionInfo.action, actionInfo.source, target));

                if (makeSourceFaceTarget)
                {
                    actionInfo.source.transform.LookAt(target.transform);
                    actionInfo.source.transform.eulerAngles = new Vector3(0f, actionInfo.source.transform.eulerAngles.y, 0f);
                }
            }
        }
    }
}