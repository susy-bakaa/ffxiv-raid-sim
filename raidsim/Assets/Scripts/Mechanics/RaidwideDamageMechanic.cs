using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ActionController;
using static GlobalStructs;

public class RaidwideDamageMechanic : FightMechanic
{
    public PartyList party;

    public override void TriggerMechanic(ActionInfo actionInfo)
    {
        List<CharacterState> members = new List<CharacterState>(party.GetActiveMembers());

        foreach (CharacterState character in members)
        {
            if (!string.IsNullOrEmpty(mechanicName))
            {
                character.ModifyHealth(new Damage(actionInfo.action.data.damage, mechanicName));
            }
            else
            {
                character.ModifyHealth(actionInfo.action.data.damage);
            }
        }
    }
}
