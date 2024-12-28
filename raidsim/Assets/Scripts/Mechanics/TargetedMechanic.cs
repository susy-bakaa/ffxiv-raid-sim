using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
using UnityEngine;
using static ActionController;
using static StatusEffectData;

public class TargetedMechanic : FightMechanic
{
    public enum TargetingType { None, Nearest, Furthest, LowestHealth, HighestHealth, StatusEffect, Random }

    public PartyList party;
    public bool autoFindParty = false;
    public TargetingType m_type = TargetingType.None;
    public int amountOfTargets = 1;
    [ShowIf("m_type", TargetingType.StatusEffect)] public StatusEffectInfo effect;
    [ShowIf("m_type", TargetingType.StatusEffect)] public bool fallBackToRandom = false;
    public bool makeSourceFaceTarget = false;
    public FightMechanic resultingMechanic;

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
                candidates.Clear();
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
                // Implement targeting furthest logic
                break;
            case TargetingType.LowestHealth:
                // Implement targeting lowest health logic
                break;
            case TargetingType.HighestHealth:
                // Implement targeting highest health logic
                break;
            case TargetingType.StatusEffect:
                candidates.Clear();
                candidates = party.GetActiveMembers();

                // Sort candidates by whether they have the status effect, then randomly within each group
                candidates = candidates.OrderByDescending(c => c.HasAnyVersionOfEffect(effect.data.statusName))
                                       .ThenBy(c => Random.value)
                                       .ToList();

                ExecuteOnTargets(actionInfo, candidates);
                break;
            case TargetingType.Random:
                // Implement targeting random logic
                break;
            default:
                Debug.LogError("Targeting type not implemented: " + m_type);
                break;
        }
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
