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

namespace dev.susybaka.raidsim.UI
{
    public class SpotSelector : MonoBehaviour
    {
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
        }

        private void Start()
        {
            dropdown = GetComponentInChildren<TMP_Dropdown>();
            Select(defaultSpot);

            if (FightTimeline.Instance != null)
                FightTimeline.Instance.onReset.AddListener(() => Select(lastSelected));
        }

        private void Update()
        {
            dropdown.interactable = !FightTimeline.Instance.playing;
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

            for (int i = 0; i < bots.Length; i++)
            {
                if (bots[i].clockSpot == player.playerController.clockSpot)
                {
                    bots[i].state.characterName = "Disabled";
                    bots[i].gameObject.SetActive(false);
                }
                else
                {
                    bots[i].state.characterName = $"AI{botNameIndex + 1}";
                    bots[i].gameObject.SetActive(true);
                    botNameIndex++;
                }
            }

            partyList.UpdatePartyList();
            lastSelected = value;
        }
    }
}