using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ActionController;

public class RaidwideWipeMechanic : FightMechanic
{
    public PartyList party;

    public void TriggerMechanic()
    {
        TriggerMechanic(new ActionInfo(null, null, null));
    }

    public override void TriggerMechanic(ActionInfo actionInfo)
    {
        // We need to add a small delay to the party wipe or otherwise the status effect's expire event fires twice. This is a simple workaround with no noticable differences.
        if (party != null)
        {
            Utilities.FunctionTimer.Create(() => FightTimeline.Instance.WipeParty(party, mechanicName), 0.1f, $"TriggerMechanic_{gameObject}_{GetHashCode()}_PartyWipe_Delay", false, true);
        }
        else
        {
            Utilities.FunctionTimer.Create(() => FightTimeline.Instance.WipeParty(FightTimeline.Instance.partyList, mechanicName), 0.1f, $"TriggerMechanic_{gameObject}_{GetHashCode()}_PartyWipe_Delay", false, true);
        }
    }
}