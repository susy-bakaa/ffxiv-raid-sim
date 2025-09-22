// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using dev.susybaka.raidsim.Characters;
using dev.susybaka.Shared;
using static dev.susybaka.raidsim.UI.PartyList;

namespace dev.susybaka.raidsim.UI
{
    public class EnmitySlider : MonoBehaviour
    {
        private PartyListHelper partyHelper;
        private CanvasGroup group;

        [SerializeField] private CharacterState character;
        private EnmitySliderColor sliderColor;
        private Slider slider;
        private TextMeshProUGUI label;

        private List<EnmityInfo> enmityList = new List<EnmityInfo>();

        int id = 0;
        private int rateLimit = 0;

        private void Awake()
        {
            id = Random.Range(0, 10000);
            group = GetComponent<CanvasGroup>();
            partyHelper = transform.parent.GetComponentInParent<PartyListHelper>();
            slider = transform.GetComponentInChildren<Slider>();
            label = transform.GetComponentInChildren<TextMeshProUGUI>();
            sliderColor = slider.GetComponent<EnmitySliderColor>();
            rateLimit = partyHelper.updateEnmityList + 5;
        }

        private void Start()
        {
            if (character == null)
            {
                Utilities.FunctionTimer.Create(this, () => character = transform.parent.GetComponent<HudElement>().characterState, 1.1f, $"EnmitySlider_{id}_start_delay", true, false);
            }
        }

        private void Update()
        {
            if (character == null)
                return;

            if (Utilities.RateLimiter(rateLimit))
            {
                // Get the updated enmity list
                enmityList = partyHelper.GetCurrentPlayerTargetEnmityList();

                // Filter out dead players from the enmity list
                enmityList = enmityList.FindAll(info => !info.state.dead);

                // Check if we have any enmity values
                if (enmityList.Count <= 0 || character.dead)
                {
                    slider.minValue = 0;
                    slider.maxValue = 1;
                    slider.value = 0;
                    label.text = "8"; // Lowest rank
                    group.alpha = 0f;
                    return;
                }
                else
                {
                    group.alpha = 1f;
                }

                // Find the player's enmity in the list
                EnmityInfo? playerEnmityInfo = enmityList.Find(info => info.state == character);

                // If no enmity value for player, default to lowest rank
                if (playerEnmityInfo == null || !playerEnmityInfo.HasValue || playerEnmityInfo.Value.enmity <= 0)
                {
                    slider.minValue = 0;
                    slider.maxValue = 1;
                    slider.value = 0;
                    label.text = "8"; // Lowest rank
                    group.alpha = 0f;
                    return;
                }

                // Get the player's index in the list (lower index = higher enmity)
                int playerIndex = enmityList.IndexOf(playerEnmityInfo.Value);

                // Update the rank label (convert 1st place to 'A' and others to numbers)
                label.text = playerIndex == 0 ? "A" : (playerIndex + 1).ToString();

                // Update enmitySliderColor based on player's position
                sliderColor.useAlternativeColors = playerIndex == 1; // True if second in the list, false otherwise

                // Set the slider values based on the neighboring enmity values
                if (playerIndex == 0)
                {
                    // Top enmity, max out the slider
                    slider.minValue = 0;
                    slider.maxValue = playerEnmityInfo.Value.enmity;
                    slider.value = playerEnmityInfo.Value.enmity;
                }
                else if (playerIndex == enmityList.Count - 1)
                {
                    // Lowest enmity, min out the slider
                    slider.minValue = 0;
                    slider.maxValue = enmityList[playerIndex - 1].enmity <= 0 ? 1 : enmityList[playerIndex - 1].enmity; // Max is the enmity of the one above
                    slider.value = enmityList[playerIndex].enmity;
                }
                else
                {
                    // For everyone else, set min and max based on neighbors
                    int lowerEnmity = enmityList[playerIndex + 1].enmity;
                    int upperEnmity = enmityList[playerIndex - 1].enmity;

                    if (upperEnmity <= 0)
                        upperEnmity = 1;

                    slider.minValue = lowerEnmity;
                    slider.maxValue = upperEnmity;
                    slider.value = playerEnmityInfo.Value.enmity;
                }
            }
        }
    }
}