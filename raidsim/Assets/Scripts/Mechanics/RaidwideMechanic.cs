using UnityEngine;
using dev.susybaka.raidsim.Core;
using dev.susybaka.raidsim.UI;
using static dev.susybaka.raidsim.Core.GlobalData;

namespace dev.susybaka.raidsim.Mechanics
{
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
                // TODO: Make this better but for now checking for character name should be fine and if the character is active
                if (party.members[i].name.ToLower().Contains("hidden") || !party.members[i].characterState.gameObject.activeInHierarchy)
                    continue;

                mechanic.TriggerMechanic(new ActionInfo(actionInfo.action, actionInfo.source, party.members[i].characterState));
            }
        }
    }
}