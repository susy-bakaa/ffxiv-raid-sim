// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using UnityEngine;
using dev.susybaka.raidsim.Characters;
using dev.susybaka.Shared;
using static dev.susybaka.raidsim.Core.GlobalData;

namespace dev.susybaka.raidsim.Mechanics
{
    public class SpawnEnemyMechanic : FightMechanic
    {
        public string enemyObjectName = string.Empty;
        public GameObject enemyObject;
        public bool activateInstead = true;
        public bool despawnInstead = false;
        public bool toggleNameplate = false;
        public bool togglePartylistEntry = false;
        public bool toggleTargetable = false;
        public Transform spawnLocation;

        public void Awake()
        {
            if (activateInstead && enemyObject == null && !string.IsNullOrEmpty(enemyObjectName))
            {
                enemyObject = Utilities.FindAnyByName(enemyObjectName);
            }
        }

        public override void TriggerMechanic(ActionInfo actionInfo)
        {
            if (!CanTrigger(actionInfo))
                return;

            if (activateInstead && enemyObject == null && !string.IsNullOrEmpty(enemyObjectName))
            {
                enemyObject = Utilities.FindAnyByName(enemyObjectName);
            }

            if (enemyObject == null)
                return;

            if (activateInstead && enemyObject != null)
            {
                enemyObject.SetActive(true);

                if (enemyObject.TryGetComponent(out CharacterState state))
                {
                    ToggleCharacterState(state);
                }
            }
            else if (enemyObject != null)
            {
                if (spawnLocation == null)
                {
                    if (actionInfo.target != null)
                    {
                        GameObject spawned = Instantiate(enemyObject, actionInfo.target.transform.position, actionInfo.target.transform.rotation, GameObject.Find("Enemies").transform);
                        if (spawned.TryGetComponent(out CharacterState state))
                        {
                            ToggleCharacterState(state, true);
                        }
                    }
                    else if (actionInfo.source != null && actionInfo.action != null)
                    {
                        GameObject spawned = Instantiate(enemyObject, actionInfo.source.transform.position, actionInfo.source.transform.rotation, GameObject.Find("Enemies").transform);
                        if (spawned.TryGetComponent(out CharacterState state))
                        {
                            ToggleCharacterState(state, true);
                        }
                    }
                }
                else
                {
                    GameObject spawned = Instantiate(enemyObject, spawnLocation.position, spawnLocation.rotation, GameObject.Find("Enemies").transform);
                    if (spawned.TryGetComponent(out CharacterState state))
                    {
                        ToggleCharacterState(state, true);
                    }
                }
            }
        }

        private void ToggleCharacterState(CharacterState state, bool overrideState = false)
        {
            if (state == null)
                return;

            if (!overrideState)
            {
                state.ToggleState(!despawnInstead);
            }
            else
            {
                state.ToggleState(true);
            }

            if (toggleNameplate)
                state.ToggleNameplate(!state.hideNameplate);
            if (togglePartylistEntry)
                state.TogglePartyListEntry(!state.hidePartyListEntry);
            if (toggleTargetable)
                state.ToggleTargetable(!state.untargetable.value);
        }
    }
}