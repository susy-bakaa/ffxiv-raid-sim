// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace dev.susybaka.raidsim.UI 
{
    public class Hotbar : MonoBehaviour
    {
        [SerializeField] private HotbarController controller;
        [SerializeField] private string groupId;
        public HotbarController Controller => controller;
        public string GroupId => groupId;

        public UnityEvent onSlotsUpdated;

        private readonly HashSet<HotbarSlot> slots = new HashSet<HotbarSlot>();

        private void Awake()
        {
            if (controller == null)
                Debug.LogError("Hotbar missing reference to HotbarController!", gameObject);

            slots.Clear();
        }

        private void OnEnable()
        {
            if (controller == null)
                Debug.LogError("Hotbar missing reference to HotbarController!", gameObject);

            controller.OnGroupChanged += HandleGroupChanged;
            RefreshSlots();
        }

        private void OnDisable()
        {
            controller.OnGroupChanged -= HandleGroupChanged;
        }

        public void RegisterSlot(HotbarSlot slot)
        {
            if (slot == null)
            {
                Debug.LogWarning("Attempted to register null slot to hotbar!", gameObject);
                return;
            }

            if (!slots.Contains(slot))
            {
                slot.SetGroup(groupId);
                slots.Add(slot);
            }
        }

        public void UnregisterSlot(HotbarSlot slot)
        {
            if (slot == null)
            {
                Debug.LogWarning("Attempted to unregister null slot from hotbar!", gameObject);
                return;
            }

            if (slots.Contains(slot))
                slots.Remove(slot);
        }

        public void RefreshSlots()
        {
            foreach (HotbarSlot slot in slots)
            {
                slot.Refresh();
            }
            onSlotsUpdated?.Invoke();
        }

        private void HandleGroupChanged(string changedGroupId)
        {
            if (changedGroupId == groupId)
                RefreshSlots();
        }
    }
}