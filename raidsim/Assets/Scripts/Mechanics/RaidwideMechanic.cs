using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GlobalData;

public class RaidwideMechanic : FightMechanic
{
    [Header("Raidwide Settings")]
    public PartyList party;
    public FightMechanic mechanic;

    public override void TriggerMechanic(ActionInfo actionInfo)
    {
        if (!CanTrigger(actionInfo))
            return;

        if (FightTimeline.Instance == null)
            return;

        if (mechanic == null)
            return;

        if (party == null)
            party = FightTimeline.Instance.partyList;

        for (int i = 0; i < party.members.Count; i++)
        {
            mechanic.TriggerMechanic(new ActionInfo(actionInfo.action, actionInfo.source, party.members[i].characterState));
        }
    }
}
