using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ActionController;
using static GlobalStructs;

public class RaidwideDamageMechanic : FightMechanic
{
    public PartyList party;
    public bool useActionDamage = true;
    public Damage damage = new Damage(0, true, false, Damage.DamageType.magical, Damage.ElementalAspect.unaspected, Damage.PhysicalAspect.none, Damage.DamageApplicationType.normal, string.Empty);

    void Awake()
    {
        if (party == null)
        {
            party = FightTimeline.Instance.partyList;
        }
    }

    public override void TriggerMechanic(ActionInfo actionInfo)
    {
        base.TriggerMechanic(actionInfo);

        List<CharacterState> members = new List<CharacterState>(party.GetActiveMembers());

        foreach (CharacterState character in members)
        {
            if (!string.IsNullOrEmpty(mechanicName))
            {
                if (actionInfo.action != null && actionInfo.action.data != null && useActionDamage)
                {
                    character.ModifyHealth(new Damage(actionInfo.action.data.damage, mechanicName));
                }
                else if (!useActionDamage)
                {
                    character.ModifyHealth(new Damage(damage, mechanicName));
                }
            }
            else
            {
                if (actionInfo.action != null && actionInfo.action.data != null && useActionDamage)
                {
                    character.ModifyHealth(actionInfo.action.data.damage);
                }
                else if (!useActionDamage)
                {
                    character.ModifyHealth(damage);
                }
            }
        }
    }
}
