// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using UnityEngine;
using TMPro;
using NaughtyAttributes;
using dev.susybaka.raidsim.Characters;
using dev.susybaka.raidsim.Core;
using dev.susybaka.raidsim.Nodes;
using static dev.susybaka.raidsim.Core.GlobalData;
using static dev.susybaka.raidsim.UI.PartyList;
using System.Collections;

namespace dev.susybaka.raidsim.UI
{
    public class SpotSelector : MonoBehaviour
    {
        FightTimeline timeline;
        TMP_Dropdown dropdown;

        public PartyList partyList;
        public bool autoObtainPlayer;
        public PartyMember player;
        public AIController[] bots;
        public BotNode[] spots;
        public bool changePlayerRole = false;
        [ShowIf("changePlayerRole")] public Role[] spotRoles;
        public bool changePlayerGroup = false;
        [ShowIf("changePlayerGroup")] public int[] spotGroups;
        public int defaultSpot = 4;
        private int lastSelected = 4;
        public bool aoz = false;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (autoObtainPlayer && partyList != null && partyList.members != null && partyList.members.Count > 0)
            {
                player = partyList.members[0];
            }
        }
#endif

        private void Awake()
        {
            if (autoObtainPlayer && partyList != null && partyList.members != null && partyList.members.Count > 0)
            {
                player = partyList.members[0];
            }

            timeline = FightTimeline.Instance;
        }

        private void Start()
        {
            dropdown = GetComponentInChildren<TMP_Dropdown>();
            Select(defaultSpot);

            if (FightTimeline.Instance != null)
                FightTimeline.Instance.onReset.AddListener(() => { StopAllCoroutines(); Select(lastSelected); });

            StartCoroutine(IE_DelayedSelect(new WaitForSecondsRealtime(1.1f)));
        }

        private void Update()
        {
            if (timeline == null)
                timeline = FightTimeline.Instance;

            dropdown.interactable = !FightTimeline.Instance.playing;
        }

        private IEnumerator IE_DelayedSelect(WaitForSecondsRealtime wait)
        {
            yield return wait;
            Select(lastSelected);
        }

        public void Select()
        {
            Select(lastSelected);
        }

        public void Select(int value)
        {
            if (player.playerController == null)
                Debug.LogError("PlayerController is unassigned!");

            player.playerController.clockSpot = spots[value];

            if (changePlayerRole)
            {
                if (spotRoles != null && spotRoles.Length == spots.Length)
                {
                    player.characterState.role = spotRoles[value];
                }
            }
            if (changePlayerGroup)
            {
                if (spotGroups != null && spotGroups.Length == spots.Length)
                {
                    player.characterState.group = spotGroups[value];
                }
            }

            int botNameIndex = 0;
            int playerIndex = -1;

            // First loop to assign clock spots and find player index for naming
            for (int i = 0; i < bots.Length; i++)
            {
                bots[i].clockSpot = spots[i];

                if (bots[i].clockSpot == player.playerController.clockSpot)
                {
                    playerIndex = i;
                }
            }
            // Second loop to set active state and names based on player index
            for (int i = 0; i < bots.Length; i++)
            {
                bots[i].clockSpot = spots[i];

                if (bots[i].clockSpot == player.playerController.clockSpot)
                {
                    bots[i].state.characterName = "Disabled";
                    bots[i].gameObject.SetActive(false);
                }
                else
                {
                    bots[i].state.characterName = GetBotName(botNameIndex, playerIndex, bots[i].state);
                    bots[i].gameObject.SetActive(true);
                    botNameIndex++;
                }
            }

            partyList.UpdatePartyList();
            lastSelected = value;
        }

        public string GetBotName(int index, int pIndex, CharacterState state)
        {
            string bName = $"AI{index + 1}";

            if (timeline != null)
            {
                if (!aoz)
                {
                    if (pIndex != -1 && index >= pIndex)
                        index++; // If the player is in the list of bots, we need to shift the index for naming since one of the bots will be disabled and not named

                    switch (timeline.botNameType)
                    {
                        case 1:
                            bName = GlobalVariables.botRoleNames[index];
                            break;
                        case 2:
                            bName = GlobalVariables.botRoleWesternShortNames[index];
                            break;
                        case 3:
                            bName = GlobalVariables.botRoleEasternShortNames[index];
                            break;
                    }
                }
                else
                    bName = $"BLU {index + 1}"; // If this is a blue mage fight, just name the bots BLU X since the normal names would not make sense

                if (timeline.colorBotNamesByRole)
                {
                    state.colorNameplateBasedOnAggression = false;

                    if (!aoz)
                        state.nameplateColor = GlobalVariables.botRoleColors[index]; // If this is not a blue mage fight, use the normal bot role colors
                    else
                        state.nameplateColor = GlobalVariables.botRoleColors[GlobalVariables.botRoleColors.Length - 1]; // If this is a blue mage fight, use the last role color for everyone, since all the bots are casters in that case
                }
                else
                {
                    state.colorNameplateBasedOnAggression = true;
                }
            }
            else
            {
                state.colorNameplateBasedOnAggression = true;
            }

            return bName;
        }
    }
}