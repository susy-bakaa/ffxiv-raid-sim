using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ActionController;
using static GlobalStructs;

public class RaidwideDamageMechanic : FightMechanic
{
    public PartyList party;

    public override void TriggerMechanic(ActionInfo action)
    {
        List<CharacterState> members = new List<CharacterState>(party.GetActiveMembers());

        foreach (CharacterState character in members)
        {
            if (!string.IsNullOrEmpty(mechanicName))
            {
                character.ModifyHealth(new Damage(action.action.data.damage, mechanicName));
            }
            else
            {
                character.ModifyHealth(action.action.data.damage);
            }
        }
    }
}
