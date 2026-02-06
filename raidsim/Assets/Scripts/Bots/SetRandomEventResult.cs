// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using UnityEngine;
using NaughtyAttributes;
using dev.susybaka.raidsim.Core;
using System.Collections.Generic;
using dev.susybaka.raidsim.Characters;
using dev.susybaka.raidsim.UI;

namespace dev.susybaka.raidsim.Events
{
    public class SetRandomEventResult : MonoBehaviour
    {
        [Label("ID"), MinValue(0)]
        public int id = 0;
        [MinValue(0)]
        public int result = 0;
        public bool setBasedOnCharacter = false;
        [ShowIf(nameof(setBasedOnCharacter))] public PartyList party;
        [ShowIf(nameof(setBasedOnCharacter))] public bool autoFindParty = false;
        [ShowIf(nameof(setBasedOnCharacter))] public List<CharacterState> characters = new List<CharacterState>();

        private void Awake()
        {
            if (setBasedOnCharacter && autoFindParty)
            {
                PartyList foundParty = FightTimeline.Instance?.partyList;
                if (foundParty != null)
                {
                    party = foundParty;
                    characters = new List<CharacterState>(party.GetActiveMembers());
                }
            }

        }

        public void SetResult(CharacterState character)
        {
            if (setBasedOnCharacter)
            {
                int index = characters.IndexOf(character);
                if (index >= 0)
                {
                    result = index;
                }
                else
                {
                    result = 0;
                }
            }
            SetResult();
        }

        public void SetResult(int result)
        {
            this.result = result;
            SetResult();
        }

        public void SetResult()
        {
            if (FightTimeline.Instance != null)
            {
                FightTimeline.Instance.SetRandomEventResult(id, result);
            }
        }
    }
}