using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using dev.susybaka.raidsim.Characters;
using dev.susybaka.raidsim.Core;
using dev.susybaka.raidsim.UI;
using static dev.susybaka.raidsim.Core.GlobalData;

namespace dev.susybaka.raidsim.Mechanics
{
    [RequireComponent(typeof(FollowTransform))]
    public class FollowCharacterMechanic : FightMechanic
    {
        FollowTransform followTransform;

        [HideIf("useActionInfoInstead")] public PartyList party;
        [HideIf("useActionInfoInstead")] public bool autoFindParty = true;
        [HideIf("useActionInfoInstead")] public int memberIndex = -1;
        [HideIf("useActionInfoInstead")] public bool useRandomMember = true;
        public bool useActionInfoInstead = false;

        private void Awake()
        {
            followTransform = GetComponent<FollowTransform>();

            if (useActionInfoInstead)
            {
                party = null;
                autoFindParty = false;
                memberIndex = -1;
                useRandomMember = false;
            }

            if (party == null && autoFindParty)
                party = FightTimeline.Instance.partyList;
        }

        public override void TriggerMechanic(ActionInfo actionInfo)
        {
            if (!CanTrigger(actionInfo))
                return;

            Transform finalTarget = null;

            if (party != null && memberIndex > -1)
            {
                List<CharacterState> members = new List<CharacterState>(party.GetActiveMembers());

                int member = 0;

                if (useRandomMember)
                {
                    member = Random.Range(0, members.Count);
                }
                else if (memberIndex > -1 && memberIndex < members.Count)
                {
                    member = memberIndex;
                }

                finalTarget = members[member].transform;
            }
            else if (useActionInfoInstead)
            {
                if (actionInfo.source != null)
                {
                    finalTarget = actionInfo.source.transform;
                }
                else if (actionInfo.target != null)
                {
                    finalTarget = actionInfo.target.transform;
                }
            }

            followTransform.target = finalTarget;
        }

        public override void InterruptMechanic(ActionInfo actionInfo)
        {
            followTransform.target = null;
        }
    }
}