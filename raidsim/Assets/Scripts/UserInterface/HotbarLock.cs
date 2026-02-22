// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using UnityEngine;
using UnityEngine.Events;

namespace dev.susybaka.raidsim.UI
{
    public class HotbarLock : MonoBehaviour
    {
        [SerializeField] private HotbarController controller;

        public UnityEvent<bool> onToggle;

        public void ToggleLock()
        {
            controller.locked = !controller.locked;
            onToggle.Invoke(controller.locked);
        }

        public void SetLocked(bool locked)
        {
            controller.locked = locked;
        }
    }
}