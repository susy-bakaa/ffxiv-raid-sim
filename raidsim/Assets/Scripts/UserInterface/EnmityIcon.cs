// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using dev.susybaka.raidsim.Characters;
using dev.susybaka.Shared;
using static dev.susybaka.raidsim.UI.PartyList;

namespace dev.susybaka.raidsim.UI
{
    public class EnmityIcon : MonoBehaviour
    {
        Image image;

        public List<Sprite> icons = new List<Sprite>();
        public CharacterState player;
        public CharacterState enemy;
        public float percentageForChange = 0.5f;

        private PartyList party;
        private List<EnmityInfo> enmityList = new List<EnmityInfo>();

        int id = 0;
        int rateLimit = 0;
        int rateLimit2 = 0;

        private void Awake()
        {
            id = Random.Range(0, 10000);
            rateLimit = Random.Range(33, 43);
            rateLimit2 = rateLimit + 5;
            image = GetComponentInChildren<Image>();
            if (player == null)
                player = Utilities.FindAnyByName("Player").GetComponent<CharacterState>();
            if (player != null)
                party = player.partyList;
        }

        private void Start()
        {
            if (enemy == null)
            {
                Utilities.FunctionTimer.Create(this, () => enemy = transform.parent.GetComponent<HudElement>().characterState, 1.1f, $"EnmityIcon_{id}_start_delay", true, false);
            }
        }

        private void Update()
        {
            if (enemy == null || party == null)
                return;

            if (Utilities.RateLimiter(rateLimit))
            {
                // Get the enmity list sorted by enmity values (highest first)
                enmityList = party.GetEnmityValuesList(enemy);

                // Filter out dead players from the enmity list
                enmityList = enmityList.FindAll(info => !info.state.dead);
            }
            if (Utilities.RateLimiter(rateLimit2))
            {
                // If no enmity values are available, set to the last icon (lowest)
                if (enmityList.Count == 0)
                {
                    image.sprite = icons[icons.Count - 1];
                    return;
                }

                // Get the highest enmity value in the party
                int highestEnmity = enmityList[0].enmity;

                // Get the player's enmity value
                EnmityInfo? playerEnmityInfo = enmityList.Find(info => info.state == player);
                int playerEnmity = playerEnmityInfo?.enmity ?? 0;

                // If the player has no enmity, set to the last icon (lowest)
                if (playerEnmity == 0)
                {
                    image.sprite = icons[icons.Count - 1];
                    return;
                }

                // Calculate the threshold where the icon starts changing
                float threshold = percentageForChange * highestEnmity;

                // If the player's enmity is below the threshold, set the last icon (lowest)
                if (playerEnmity < threshold)
                {
                    image.sprite = icons[icons.Count - 1];
                }
                else
                {
                    // Calculate how much the player's enmity exceeds the threshold
                    float progress = (float)(playerEnmity - threshold) / (highestEnmity - threshold);

                    // Scale the icon based on the player's progress towards the highest enmity
                    int iconIndex = Mathf.FloorToInt((1f - progress) * (icons.Count - 1));
                    iconIndex = Mathf.Clamp(iconIndex, 0, icons.Count - 1);

                    // Set the icon
                    image.sprite = icons[iconIndex];
                }
            }
        }
    }
}