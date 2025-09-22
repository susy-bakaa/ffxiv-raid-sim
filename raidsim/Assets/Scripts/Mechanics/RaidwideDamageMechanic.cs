// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using System.Collections.Generic;
using dev.susybaka.raidsim.Characters;
using dev.susybaka.raidsim.Core;
using dev.susybaka.raidsim.UI;
using static dev.susybaka.raidsim.Core.GlobalData;

namespace dev.susybaka.raidsim.Mechanics
{
    public class RaidwideDamageMechanic : FightMechanic
    {
        public PartyList party;
        public bool autoFindParty = true;
        public bool useActionDamage = true;
        public Damage damage = new Damage(0, true, false, Damage.DamageType.magical, Damage.ElementalAspect.unaspected, Damage.PhysicalAspect.none, Damage.DamageApplicationType.normal, string.Empty);

        private void Awake()
        {
            if (party == null && autoFindParty)
                party = FightTimeline.Instance.partyList;
        }

        public override void TriggerMechanic(ActionInfo actionInfo)
        {
            if (!CanTrigger(actionInfo))
                return;

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
}