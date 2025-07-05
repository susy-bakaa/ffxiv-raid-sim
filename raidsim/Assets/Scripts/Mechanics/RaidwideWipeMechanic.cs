using UnityEngine.Events;
using dev.susybaka.raidsim.Core;
using dev.susybaka.raidsim.UI;
using dev.susybaka.Shared;
using static dev.susybaka.raidsim.Core.GlobalData;

namespace dev.susybaka.raidsim.Mechanics
{
    public class RaidwideWipeMechanic : FightMechanic
    {
        public string wipeName;
        public PartyList party;
        public bool activatesWhenDead;
        public UnityEvent<ActionInfo> onExecuteWipe;

        public override void TriggerMechanic(ActionInfo actionInfo)
        {
            if (!CanTrigger(actionInfo))
                return;

            if (string.IsNullOrEmpty(wipeName) && !string.IsNullOrEmpty(mechanicName))
                wipeName = mechanicName;

            if (activatesWhenDead)
            {
                if (party != null)
                {
                    PartyList temp = FightTimeline.Instance.partyList;
                    if (!temp.HasDeadMembers())
                        return;
                }
                else
                {
                    if (!party.HasDeadMembers())
                        return;
                }
            }

            // We need to add a small delay to the party wipe or otherwise the status effect's expire event fires twice. This is a simple workaround with no noticable differences.
            if (party != null)
            {
                Utilities.FunctionTimer.Create(this, () => FightTimeline.Instance.WipeParty(party, wipeName), 0.1f, $"TriggerMechanic_{this}_{GetHashCode()}_PartyWipe_Delay", false, true);
            }
            else
            {
                Utilities.FunctionTimer.Create(this, () => FightTimeline.Instance.WipeParty(FightTimeline.Instance.partyList, wipeName), 0.1f, $"TriggerMechanic_{this}_{GetHashCode()}_PartyWipe_Delay", false, true);
            }

            onExecuteWipe.Invoke(actionInfo);
        }
    }
}