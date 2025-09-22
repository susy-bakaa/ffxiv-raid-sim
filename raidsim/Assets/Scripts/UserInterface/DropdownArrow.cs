// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using UnityEngine;
using TMPro;
using dev.susybaka.raidsim.Inputs;
using dev.susybaka.Shared;

namespace dev.susybaka.raidsim.UI
{
    [RequireComponent(typeof(TMP_Dropdown))]
    public class DropdownArrow : MonoBehaviour
    {
        TMP_Dropdown dropdown;

        public GameObject standard;
        public GameObject expanded;
        public UserInput userInput;

        private bool inputSet = false;

        private void Awake()
        {
            dropdown = GetComponent<TMP_Dropdown>();
            if (userInput == null)
                userInput = FindObjectOfType<UserInput>();
        }

        private void Update()
        {
            if (Utilities.RateLimiter(27))
            {
                if (dropdown.IsExpanded)
                {
                    standard.SetActive(false);
                    expanded.SetActive(true);
                    if (userInput != null)
                    {
                        userInput.rotationInputEnabled = false;
                        userInput.zoomInputEnabled = false;
                    }
                    inputSet = false;
                }
                else
                {
                    expanded.SetActive(false);
                    standard.SetActive(true);
                    if (userInput != null && !inputSet)
                    {
                        userInput.rotationInputEnabled = true;
                        userInput.zoomInputEnabled = true;
                        inputSet = true;
                    }
                }
            }
        }
    }
}