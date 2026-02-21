// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using UnityEngine;

namespace dev.susybaka.raidsim.UI
{
    public class Nameplate : MonoBehaviour
    {
        private Canvas nameplateCanvas;
        private Camera mainCamera;

        private void Awake()
        {
            nameplateCanvas = GetComponentInChildren<Canvas>(true);
            mainCamera = Camera.main;

            if (nameplateCanvas != null)
            {
                nameplateCanvas.worldCamera = mainCamera;
            }
        }
    }
}