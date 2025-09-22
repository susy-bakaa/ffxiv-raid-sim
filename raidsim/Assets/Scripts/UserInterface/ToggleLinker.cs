// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace dev.susybaka.raidsim.UI
{
    public class ToggleLinker : MonoBehaviour
    {
        public List<Toggle> toggles = new List<Toggle>();

        public UnityEvent<int> onTogglesUpdated;

        public void UpdateToggles(int toggle)
        {
            UpdateToggles(toggles[toggle]);
        }

        public void UpdateToggles(Toggle toggle)
        {
            foreach (Toggle t in toggles)
            {
                if (t != toggle)
                {
                    t.SetIsOnWithoutNotify(false);
                }
                else
                {
                    t.SetIsOnWithoutNotify(true);
                    onTogglesUpdated.Invoke(toggles.IndexOf(t));
                }
            }
        }

        public void SetToggles(int toggle)
        {
            SetToggles(toggles[toggle]);
        }

        public void SetToggles(Toggle toggle)
        {
            foreach (Toggle t in toggles)
            {
                if (t != toggle)
                {
                    t.SetIsOnWithoutNotify(false);
                }
                else
                {
                    t.isOn = true;
                }
            }
        }
    }
}