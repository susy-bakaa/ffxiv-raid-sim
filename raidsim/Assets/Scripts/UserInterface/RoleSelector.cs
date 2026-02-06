// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using System;
using System.Collections.Generic;
using dev.susybaka.raidsim.Characters;
using dev.susybaka.raidsim.Core;
using dev.susybaka.raidsim.Nodes;
using TMPro;
using UnityEngine;
using static dev.susybaka.raidsim.Core.GlobalData;
using static dev.susybaka.raidsim.UI.PartyList;

namespace dev.susybaka.raidsim.UI
{
    public class RoleSelector : MonoBehaviour
    {
        FightTimeline timeline;
        TMP_Dropdown dropdown;

        public PartyList partyList;
        public bool autoObtainPlayer;
        public PartyMember player;
        public PartyMember[] bots;
        public BotNode[] spots;
        public List<RoleSelectorPair> roleSelectorPairs = new List<RoleSelectorPair>();
        private int lastSelected = 0;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (autoObtainPlayer && partyList != null && partyList.members != null && partyList.members.Count > 0)
            {
                player = partyList.members[0];
            }
            if (roleSelectorPairs != null && roleSelectorPairs.Count > 0)
            {
                for (int i = 0; i < roleSelectorPairs.Count; i++)
                {
                    RoleSelectorPair temp = roleSelectorPairs[i];

                    if (roleSelectorPairs[i].gameObject != null)
                        temp.name = roleSelectorPairs[i].gameObject.name;

                    roleSelectorPairs[i] = temp;
                }
            }
        }
#endif

        private void Awake()
        {
            timeline = FightTimeline.Instance;

            if (autoObtainPlayer && partyList != null && partyList.members != null && partyList.members.Count > 0)
            {
                for (int i = 0; i < partyList.members.Count; i++)
                {
                    if (partyList.members[i].name == "Player")
                    {
                        player = partyList.members[i];
                        return;
                    }
                }
            }
        }

        private void Start()
        {
            dropdown = GetComponentInChildren<TMP_Dropdown>();
            Select(0);
            if (FightTimeline.Instance != null)
                FightTimeline.Instance.onReset.AddListener(() => Select(lastSelected));
        }

        private void Update()
        {
            if (timeline == null)
                timeline = FightTimeline.Instance;

            dropdown.interactable = !FightTimeline.Instance.playing;
        }

        public void Select()
        {
            Select(lastSelected);
        }

        public void Select(int value)
        {
            if (player.playerController == null)
                Debug.LogError("PlayerController is unassigned!");

            player.characterState.role = (Role)value;

            int botNameIndex = 0;
            bool botDisabled = false;

            for (int i = 0; i < bots.Length; i++)
            {
                if (bots[i].characterState.role == player.characterState.role && !botDisabled)
                {
                    botDisabled = true;
                    bots[i].characterState.characterName = "Disabled";
                    bots[i].characterState.gameObject.SetActive(false);
                }
                else
                {
                    bots[i].characterState.characterName = GetBotName(botNameIndex, bots[i].characterState);
                    bots[i].characterState.gameObject.SetActive(true);
                    botNameIndex++;
                }
            }

            if (roleSelectorPairs != null && roleSelectorPairs.Count > 0)
            {
                for (int i = 0; i < roleSelectorPairs.Count; i++)
                {
                    if (roleSelectorPairs[i].roles.Contains((Role)value))
                    {
                        roleSelectorPairs[i].gameObject.SetActive(true);
                    }
                    else
                    {
                        roleSelectorPairs[i].gameObject.SetActive(false);
                    }
                }
            }

            lastSelected = value;

            if (partyList.SetupDone)
                partyList.UpdatePartyList();
        }

        public string GetBotName(int index, CharacterState state)
        {
            string bName = $"AI{index + 1}";

            if (timeline != null)
            {
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

                if (timeline.colorBotNamesByRole)
                {
                    state.colorNameplateBasedOnAggression = false;
                    state.nameplateColor = GlobalVariables.botRoleColors[index];
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

        [System.Serializable]
        public struct RoleSelectorPair
        {
            public string name;
            public List<Role> roles;
            public GameObject gameObject;

            public RoleSelectorPair(string name, List<Role> role, GameObject gameObject)
            {
                this.name = name;
                this.roles = role;
                this.gameObject = gameObject;
            }
        }
    }
}