// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using System.Collections.Generic;
using UnityEngine;
using dev.susybaka.raidsim.Targeting;
using dev.susybaka.Shared;
using static dev.susybaka.raidsim.UI.PartyList;

namespace dev.susybaka.raidsim.UI
{
    [RequireComponent(typeof(PartyList))]
    public class PartyListHelper : MonoBehaviour
    {
        private PartyList party;
        public PartyList PartyList { get { return party; } }

        public int updateEnmityList = 130;
        public TargetController player;

        private List<EnmityInfo> enmityAgainstPlayerTarget = new List<EnmityInfo>();

        private void Awake()
        {
            party = GetComponent<PartyList>();
            if (player == null)
            {
                for (int i = 0; i < party.members.Count; i++)
                {
                    if (party.members[i].characterState == null)
                        continue;

                    if (party.members[i].characterState.characterName.ToLower().Contains("player"))
                    {
                        player = party.members[i].targetController;
                    }
                }
            }
        }

        private void Update()
        {
            if (Utilities.RateLimiter(updateEnmityList))
            {
                if (player != null && player.currentTarget != null && player.allowedGroups.Contains(player.currentTarget.Group))
                {
                    // Get the enmity list sorted by enmity values (highest first)
                    enmityAgainstPlayerTarget = party.GetEnmityValuesList(player.currentTarget.GetCharacterState());
                }
                else
                {
                    enmityAgainstPlayerTarget.Clear();
                }
            }
        }

        public List<EnmityInfo> GetCurrentPlayerTargetEnmityList()
        {
            return enmityAgainstPlayerTarget;
        }
    }
}