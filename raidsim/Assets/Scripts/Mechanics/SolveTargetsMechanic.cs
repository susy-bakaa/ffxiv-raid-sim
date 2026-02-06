using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using dev.susybaka.raidsim.Characters;
using dev.susybaka.raidsim.UI;
using static dev.susybaka.raidsim.Core.GlobalData;

namespace dev.susybaka.raidsim.Mechanics
{
    public class SolveTargetsMechanic : FightMechanic
    {
        [Header("Solve Targets Mechanic Settings")]
        public List<CharacterState> charactersToSolve;
        public PartyList targetList;
        public bool matchBasedOnRoles = false;
        [ShowIf(nameof(matchBasedOnRoles))] public bool accountForGroups = false;
        public bool fallbackToRandom = true;

        public override void TriggerMechanic(ActionInfo actionInfo)
        {
            if (!CanTrigger(actionInfo))
                return;

            foreach (CharacterState character in charactersToSolve)
            {
                if (character == null || character.targetController == null)
                    continue;

                List<CharacterState> candidates = new List<CharacterState>(targetList.GetActiveMembers());

                if (log)
                {
                    for (int i = 0; i < candidates.Count; i++)
                    {
                        Debug.Log($"[SolveTargetsMechanic ({gameObject.name})] --> {character.characterName} found candidate {candidates[i].characterName} ({i}) out of {candidates.Count} total.");
                    }
                }

                if (matchBasedOnRoles && candidates != null && candidates.Count > 0)
                {
                    for (int i = candidates.Count - 1; i >= 0; i--)
                    {
                        CharacterState candidate = candidates[i];

                        if (log)
                            Debug.Log($"[SolveTargetsMechanic ({gameObject.name})] {character.characterName} evaluating {candidate.characterName} ({i}) as candidate target out of {candidates.Count} total.");

                        if (candidate.role != character.role)
                        {
                            candidates.RemoveAt(i);
                            if (log)
                                Debug.Log($"[SolveTargetsMechanic ({gameObject.name})] {character.characterName} rejected {candidate.characterName} ({i}) due to role mismatch.");
                            continue;
                        }
                        if (accountForGroups && candidate.group != character.group)
                        {
                            candidates.RemoveAt(i);
                            if (log)
                                Debug.Log($"[SolveTargetsMechanic ({gameObject.name})] {character.characterName} rejected {candidate.characterName} ({i}) due to group mismatch.");
                            continue;
                        }
                        if (log)
                            Debug.Log($"[SolveTargetsMechanic ({gameObject.name})] {character.characterName} considering {candidate.characterName} as candidate target.");
                    }
                }
                else
                {
                    candidates.RemoveAll(c => c == character);
                }

                if (candidates == null || candidates.Count == 0)
                {
                    Debug.LogWarning($"[SolveTargetsMechanic ({gameObject.name})] No valid candidates found for {character.characterName}.");
                    continue;
                }

                CharacterState picked = null;

                if (candidates.Count == 1 || !fallbackToRandom)
                {
                    picked = candidates[0];
                    if (log)
                        Debug.Log($"[SolveTargetsMechanic ({gameObject.name})] Only one candidate found for {character.characterName}, picking {picked.characterName}.");
                }
                else if (fallbackToRandom)
                {
                    picked = candidates[Random.Range(0, candidates.Count)];
                    if (log)
                        Debug.Log($"[SolveTargetsMechanic ({gameObject.name})] Multiple candidates found for {character.characterName}, randomly picked {picked.characterName}.");
                }

                if (picked == null || picked.targetController == null || picked.targetController.self == null)
                {
                    Debug.LogWarning($"[SolveTargetsMechanic ({gameObject.name})] Picked target is invalid for {character.characterName}.");
                    continue;
                }

                character.targetController.SetTarget(picked.targetController.self);
            }
        }
    }
}