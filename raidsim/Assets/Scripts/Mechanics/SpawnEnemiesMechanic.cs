// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using UnityEngine;
using dev.susybaka.raidsim.Characters;
using static dev.susybaka.raidsim.Core.GlobalData;

namespace dev.susybaka.raidsim.Mechanics
{
    [AddComponentMenu("")] // This hides it from the "Add Component" menu in Unity Editor
    [System.Obsolete("This mechanic is retired and considered a legacy component. It will be kept for backwards compatibility but for any new timelines please use the CharacterStateMechanic component instead. It offers the same features and even more and is up to date.")]
    public class SpawnEnemiesMechanic : FightMechanic
    {
        public GameObject[] enemyObjects;
        public bool activateInstead = true;
        public bool despawnInstead = false;
        public bool toggleNameplate = false;
        public bool togglePartylistEntry = false;
        public bool toggleTargetable = false;
        public Transform spawnLocation;

        public override void TriggerMechanic(ActionInfo actionInfo)
        {
            if (!CanTrigger(actionInfo))
                return;

            if (enemyObjects == null || enemyObjects.Length < 1)
                return;

            if (activateInstead)
            {
                for (int i = 0; i < enemyObjects.Length; i++)
                {
                    enemyObjects[i].SetActive(true);

                    if (enemyObjects[i].TryGetComponent(out CharacterState state))
                    {
                        ToggleCharacterState(state);
                    }
                }
            }
            else
            {
                if (spawnLocation == null)
                {
                    if (actionInfo.target != null)
                    {
                        for (int i = 0; i < enemyObjects.Length; i++)
                        {
                            GameObject spawned = Instantiate(enemyObjects[i], actionInfo.target.transform.position, actionInfo.target.transform.rotation, GameObject.Find("Enemies").transform);
                            if (spawned.TryGetComponent(out CharacterState state))
                            {
                                ToggleCharacterState(state, true);
                            }
                        }
                    }
                    else if (actionInfo.source != null && actionInfo.action != null)
                    {
                        for (int i = 0; i < enemyObjects.Length; i++)
                        {
                            GameObject spawned = Instantiate(enemyObjects[i], actionInfo.source.transform.position, actionInfo.source.transform.rotation, GameObject.Find("Enemies").transform);
                            if (spawned.TryGetComponent(out CharacterState state))
                            {
                                ToggleCharacterState(state, true);
                            }
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < enemyObjects.Length; i++)
                    {
                        GameObject spawned = Instantiate(enemyObjects[i], spawnLocation.position, spawnLocation.rotation, GameObject.Find("Enemies").transform);
                        if (spawned.TryGetComponent(out CharacterState state))
                        {
                            ToggleCharacterState(state, true);
                        }
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