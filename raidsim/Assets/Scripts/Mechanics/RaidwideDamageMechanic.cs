using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ActionController;

public class RaidwideDamageMechanic : FightMechanic
{
    public PartyList party;

    public override void TriggerMechanic(ActionInfo action)
    {
        List<CharacterState> members = new List<CharacterState>(party.GetActiveMembers());

        foreach (CharacterState character in members)
        {
            character.ModifyHealth(Mathf.RoundToInt(action.action.data.damage));
        }
    }
}
