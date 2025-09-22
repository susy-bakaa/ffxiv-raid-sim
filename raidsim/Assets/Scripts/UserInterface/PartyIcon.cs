// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using System.Collections.Generic;
using UnityEngine;
using dev.susybaka.raidsim.Characters;

namespace dev.susybaka.raidsim.UI
{
    public class PartyIcon : MonoBehaviour
    {
        PartyList partyList;

        public List<Transform> icons = new List<Transform>();
        public int icon = 5;
        public CharacterState member;
        public bool chooseAutomatically = true;

        private void Awake()
        {
            partyList = transform.parent.parent.GetComponent<PartyList>();
            icons.Clear();
            icons.AddRange(GetComponentsInChildren<Transform>(true));
            icons.RemoveAt(0);
        }

        public void UpdateIcon()
        {
            if (chooseAutomatically)
            {
                for (int i = 0; i < partyList.members.Count; i++)
                {
                    if (partyList.members[i].characterState == member)
                    {
                        icon = (int)partyList.members[i].characterState.role;
                        break;
                    }
                }
            }

            for (int i = 0; i < icons.Count; i++)
            {
                if (i != icon)
                    icons[i].gameObject.SetActive(false);
                else
                    icons[i].gameObject.SetActive(true);
            }
        }
    }
}