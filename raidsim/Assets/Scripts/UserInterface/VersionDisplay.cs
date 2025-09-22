// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using UnityEngine;
using TMPro;

namespace dev.susybaka.raidsim.UI
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class VersionDisplay : MonoBehaviour
    {
        TextMeshProUGUI display;

        private void Awake()
        {
            display = GetComponent<TextMeshProUGUI>();
            display.text = $"Version: {Application.version}";
        }
    }
}