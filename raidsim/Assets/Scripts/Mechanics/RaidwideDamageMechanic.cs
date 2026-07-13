// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
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
        public bool randomizeParty = true;
        [Min(0f)] public float delayBetweenMembers = 0f;
        public Damage damage = new Damage(0, true, false, Damage.DamageType.magical, Damage.ElementalAspect.unaspected, Damage.PhysicalAspect.none, Damage.DamageApplicationType.normal, string.Empty);
        private Coroutine ieDealDamage = null;

        private void Awake()
        {
            if (party == null && autoFindParty)
                party = FightTimeline.Instance.partyList;
        }

        public override void TriggerMechanic(ActionInfo actionInfo)
        {
            if (!CanTrigger(actionInfo))
                return;

            if (ieDealDamage == null)
                ieDealDamage = StartCoroutine(IE_DealDamage(actionInfo));
        }

        private IEnumerator IE_DealDamage(ActionInfo actionInfo)
        {
            List<CharacterState> members = new List<CharacterState>(party.GetActiveMembers());

            if (log)
                Debug.Log($"[RaidwideDamageMechanic ({gameObject.name})] Damage triggered on {members.Count} members of the party");

            if (randomizeParty)
            {
                // Fisher-Yates shuffle to randomize the party list in-place
                for (int i = members.Count - 1; i > 0; i--)
                {
                    int randomIndex = (int)(FightTimeline.Instance.random.Value01($"RaidwideDamageMechanic_{gameObject.name}_RandomizePartyList_{i}") * (i + 1));
                    // Swap
                    var temp = members[i];
                    members[i] = members[randomIndex];
                    members[randomIndex] = temp;
                }
            }

            foreach (CharacterState character in members)
            {
                if (log)
                    Debug.Log($"[RaidwideDamageMechanic ({gameObject.name})] Damaging '{character.GetCharacterName()}' ({character.gameObject.name}) with {damage.value} damage out of {members.Count} characters");

                if (!string.IsNullOrEmpty(mechanicName))
                {
                    if (actionInfo.action != null && actionInfo.action.Data != null && useActionDamage)
                    {
                        character.ModifyHealth(new Damage(actionInfo.action.Data.damage, mechanicName));
                    }
                    else if (!useActionDamage)
                    {
                        character.ModifyHealth(new Damage(damage, mechanicName));
                    }
                }
                else
                {
                    if (actionInfo.action != null && actionInfo.action.Data != null && useActionDamage)
                    {
                        character.ModifyHealth(actionInfo.action.Data.damage);
                    }
                    else if (!useActionDamage)
                    {
                        character.ModifyHealth(damage);
                    }
                }
                if (delayBetweenMembers > 0f)
                    yield return new WaitForSeconds(delayBetweenMembers);
                else
                    yield return null;
            }
        }
    }
}