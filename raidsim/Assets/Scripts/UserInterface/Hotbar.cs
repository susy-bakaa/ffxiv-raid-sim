// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using dev.susybaka.raidsim.Core;
using dev.susybaka.raidsim.Characters;

namespace dev.susybaka.raidsim.UI 
{
    public class Hotbar : MonoBehaviour
    {
        [SerializeField] private bool usePlayer = true;
        [SerializeField] private CharacterState owner;
        [SerializeField] private string groupId;
        [SerializeField] private ConfigMenu configMenu;
        public HotbarController Controller => owner?.hotbarController;
        public CharacterState Owner => owner;
        public string GroupId => groupId;

        public UnityEvent onSlotsUpdated;

        private readonly HashSet<HotbarSlot> slots = new HashSet<HotbarSlot>();
        private bool awoken = false;

        private void Awake()
        {
            if (usePlayer)
                owner = FightTimeline.Instance.player;

            if (owner == null || owner.hotbarController == null)
                Debug.LogError("Hotbar missing reference to HotbarController!", gameObject);

            if (configMenu == null)
            {
                configMenu = FindObjectOfType<ConfigMenu>();
                if (configMenu != null)
                    configMenu.onChangeKeybinds.AddListener(RefreshSlots);
                awoken = true;
            }

            slots.Clear();
        }

        private void OnEnable()
        {
            if (owner == null || owner.hotbarController == null)
                Debug.LogError("Hotbar missing reference to HotbarController!", gameObject);

            owner.hotbarController.OnGroupChanged += HandleGroupChanged;
            owner.hotbarController.OnRefreshHotbars += RefreshSlotsStatic;
            RefreshSlots();

            if (!awoken)
                return;

            if (configMenu != null)
                configMenu.onChangeKeybinds.AddListener(RefreshSlots);
        }

        private void OnDisable()
        {
            owner.hotbarController.OnGroupChanged -= HandleGroupChanged;
            owner.hotbarController.OnRefreshHotbars -= RefreshSlotsStatic;
            
            if (!awoken)
                 return;

            if (configMenu != null)
                configMenu.onChangeKeybinds.RemoveListener(RefreshSlots);
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

        public void RefreshSlotsStatic()
        {
            foreach (HotbarSlot slot in slots)
            {
                slot.RefreshStatic();
            }
        }

        private void HandleGroupChanged(string changedGroupId)
        {
            if (changedGroupId == groupId)
                RefreshSlots();
        }
    }
}