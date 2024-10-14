using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ActionController;
using static GlobalStructs;

[RequireComponent(typeof(FollowTransform))]
public class FollowCharacterMechanic : FightMechanic
{
    FollowTransform followTransform;

    public PartyList party;
    public bool autoFindParty = true;
    public bool useRandomMember = true;
    public int memberIndex = -1;

    void Awake()
    {
        followTransform = GetComponent<FollowTransform>();

        if (party == null && autoFindParty)
            party = FightTimeline.Instance.partyList;
    }

    public override void TriggerMechanic(ActionInfo actionInfo)
    {
        base.TriggerMechanic(actionInfo);

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

        followTransform.target = members[member].transform;
    }
}
