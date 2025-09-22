// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using UnityEngine;
using UnityEngine.UI;

namespace dev.susybaka.raidsim.UI
{
    public class ToggleImage : MonoBehaviour
    {
        public Sprite off;
        public Sprite on;
        public Image target;
        public bool CurrentState => state;

        private bool state;

        public void Toggle(bool state)
        {
            this.state = state;
            if (state)
            {
                target.sprite = on;
            }
            else
            {
                target.sprite = off;
            }
        }
    }
}