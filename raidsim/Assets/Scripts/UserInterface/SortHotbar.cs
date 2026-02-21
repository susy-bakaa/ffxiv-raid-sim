// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using dev.susybaka.raidsim.Actions;
using dev.susybaka.raidsim.Characters;
using dev.susybaka.raidsim.Core;
using dev.susybaka.Shared;
using static dev.susybaka.raidsim.Core.GlobalData;

namespace dev.susybaka.raidsim.UI
{
    public class SortHotbar : MonoBehaviour
    {
        private CharacterState character;
        private List<CharacterAction> actions = new List<CharacterAction>();
        private Coroutine ieUpdateSortingDelayed;
        [SerializeField] private float updateSortingOnStartDelay = 0f;

        private void Start()
        {
            actions.Clear();
            foreach (Transform child in transform)
            {
                CharacterAction action = child.GetComponent<CharacterAction>();
                if (action != null)
                {
                    actions.Add(action);
                }
            }

            //if (actions != null && actions.Count > 0)
            //    character = actions[0].GetCharacter();

            OnStart();

            if (FightTimeline.Instance != null)
            {
                FightTimeline.Instance.onReset.AddListener(OnStart);
            }
        }

        private void OnStart()
        {
            if (updateSortingOnStartDelay <= 0f)
                UpdateSorting();
            else
                Utilities.FunctionTimer.Create(this, () => UpdateSorting(), updateSortingOnStartDelay, "SortHotbar_UpdateSorting_DelayOnStart", false, true);
        }

        public void UpdateSorting()
        {
            UpdateSortingInternal();
            if (ieUpdateSortingDelayed == null)
                ieUpdateSortingDelayed = StartCoroutine(IE_UpdateSortingDelayed(new WaitForSecondsRealtime(0.05f)));
        }

        private IEnumerator IE_UpdateSortingDelayed(WaitForSecondsRealtime wait)
        {
            yield return wait;
            UpdateSortingInternal();
            ieUpdateSortingDelayed = null;
        }

        private void UpdateSortingInternal()
        {
            if (actions != null && actions.Count > 0)
            {
                for (int i = 0; i < actions.Count; i++)
                {
                    if (actions[i].unavailable)
                        actions[i].gameObject.SetActive(false);
                    else
                        actions[i].gameObject.SetActive(true);

                    foreach (Role role in actions[i].availableForRoles)
                    {
                        if (role == character.role)
                        {
                            actions[i].gameObject.SetActive(true);
                            break;
                        }
                    }
                }
            }
        }
    }
}