// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using UnityEngine;
using dev.susybaka.raidsim.UI;

namespace dev.susybaka.raidsim.Actions
{
    public class OpenCommandPanel : MonoBehaviour
    {
        HudWindow commandPanel;

        private void Awake()
        {
            commandPanel = GameObject.Find("UserInterface").transform.Find("Canvas/CommandPanel").GetComponent<HudWindow>();
        }

        public void Toggle()
        {
            if (commandPanel != null)
            {
                if (commandPanel.isOpen)
                {
                    commandPanel.CloseWindow();
                }
                else
                {
                    commandPanel.OpenWindow();
                }
            }
        }
    }
}