// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using UnityEngine;
using dev.susybaka.raidsim.Inputs;

namespace dev.susybaka.raidsim.UI
{
    public class TimelineConfigMenu : HudWindow
    {
        UserInput userInput;

        [SerializeField] private HudWindow menuBar;

        protected override void Awake()
        {
            base.Awake();

            userInput = FindObjectOfType<UserInput>();

            CloseWindow();
            menuBar.EnableInteractions();
        }

        private void Update()
        {
            if (userInput != null && isOpen)
            {
                userInput.targetRaycastInputEnabled = false;
                userInput.zoomInputEnabled = false;
            }
        }

        public void ToggleTimelineConfig()
        {
            if (isOpen)
            {
                CloseTimelineConfig();
            }
            else
            {
                OpenTimelineConfig();
            }
        }

        public void OpenTimelineConfig()
        {
            OpenWindow();
            if (menuBar != null)
            {
                menuBar.DisableInteractions();
            }
        }

        public void CloseTimelineConfig()
        {
            CloseWindow();
            if (menuBar != null)
            {
                menuBar.EnableInteractions();
            }
        }
    }
}