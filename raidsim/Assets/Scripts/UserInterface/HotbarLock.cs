// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace dev.susybaka.raidsim.UI
{
    [RequireComponent(typeof(Image))]
    public class HotbarLock : MonoBehaviour
    {
        private Image icon;

        [SerializeField] private Hotbar hotbar;
        [SerializeField] private Sprite lockedIcon;
        [SerializeField] private Sprite unlockedIcon;

        public UnityEvent<bool> onToggle;

        private HotbarController controller;

        private void Start()
        {
            icon = GetComponent<Image>();
            controller = hotbar?.Controller;
            onToggle.AddListener(UpdateIcon);
        }

        public void ToggleLock()
        {
            controller.locked = !controller.locked;
            onToggle.Invoke(controller.locked);
        }

        public void SetLocked(bool locked)
        {
            controller.locked = locked;
        }

        private void UpdateIcon(bool locked)
        {
            if (icon == null || lockedIcon == null || unlockedIcon == null)
            {
                return;
            }

            if (locked)
            {
                icon.sprite = lockedIcon;
            }
            else
            {
                icon.sprite = unlockedIcon;
            }
        }
    }
}