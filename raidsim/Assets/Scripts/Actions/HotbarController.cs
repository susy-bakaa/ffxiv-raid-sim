// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using System.Collections.Generic;
using dev.susybaka.raidsim.Actions;
using dev.susybaka.raidsim.Characters;
using dev.susybaka.raidsim.Core;
using Unity.VisualScripting;
using UnityEngine;
using static dev.susybaka.raidsim.Core.GlobalData;

namespace dev.susybaka.raidsim.UI
{
    public class HotbarController : MonoBehaviour
    {
        [Header("Runtime refs")]
        [SerializeField] private CharacterState character;
        [SerializeField] private CharacterActionRegistry registry;
        [SerializeField] private MacroLibrary macroLibrary; // your library
        [SerializeField] private ChatHandler chat;      // your sender
        public bool locked = false;

        public CharacterState Character => character;
        public CharacterActionRegistry Registry => registry;
        public MacroLibrary MacroLibrary => macroLibrary;
        public ChatHandler Chat => chat;

        [Header("Layout")]
        [SerializeField] private HotbarGroupDefinition[] groupDefinitions;
        public HotbarGroupDefinition[] GroupDefinitions => groupDefinitions;

        //[Header("Temp keybinds (replace with your input system later)")]
        //[SerializeField] private KeyCode[] slotKeys; // length slotCount

        public event System.Action<string> OnGroupChanged;
        private readonly Dictionary<string, GroupState> groups = new();

        private void Awake()
        {
            RebuildGroups();
        }

        private void Update()
        {
            // Minimal keybind support; replace with your input layer later
            /*for (int i = 0; i < slotCount; i++)
            {
                var key = slotKeys[i];
                if (key != KeyCode.None && Input.GetKeyDown(key))
                    ExecuteSlot(i);
            }*/
        }

        private void RebuildGroups()
        {
            groups.Clear();

            foreach (var def in groupDefinitions)
            {
                var g = new GroupState
                {
                    groupId = def.groupId,
                    slotsPerPage = def.slotsPerPage,
                    pageCount = Mathf.Max(1, def.pageCount),
                    activePage = 0,
                };

                g.pages = new SlotBinding[g.pageCount][];
                for (int p = 0; p < g.pageCount; p++)
                    g.pages[p] = new SlotBinding[g.slotsPerPage];

                groups.Add(g.groupId, g);
            }
        }

        public int GetActivePage(string groupId) => groups[groupId].activePage;

        public void SetActivePage(string groupId, int page)
        {
            var g = groups[groupId];
            page = Mathf.Clamp(page, 0, g.pageCount - 1);
            if (g.activePage == page)
                return;
            g.activePage = page;
            NotifyGroupChanged(groupId);
        }

        public SlotBinding GetBinding(string groupId, int slotIndex)
        {
            var g = groups[groupId];
            return g.pages[g.activePage][slotIndex];
        }

        public SlotBinding GetBinding(string groupId, int pageIndex, int slotIndex)
        {
            var g = groups[groupId];
            return g.pages[pageIndex][slotIndex];
        }

        public void SetSlot(string groupId, int slotIndex, SlotBinding binding)
        {
            var g = groups[groupId];
            g.pages[g.activePage][slotIndex] = binding;
            NotifyGroupChanged(groupId);
        }

        public void SetSlot(string groupId, int pageIndex, int slotIndex, SlotBinding binding)
        {
            var g = groups[groupId];
            g.pages[pageIndex][slotIndex] = binding;
            NotifyGroupChanged(groupId);
        }

        public void ClearSlot(string groupId, int slotIndex)
        {
            if (locked)
                return;

            SetSlot(groupId, slotIndex, new SlotBinding { kind = SlotKind.Empty, id = "" });
        }

        public void SwapSlots(string gA, int pA, int sA, string gB, int pB, int sB)
        {
            if (locked)
                return;

            var a = GetBinding(gA, pA, sA);
            var b = GetBinding(gB, pB, sB);
            SetSlot(gA, pA, sA, b);
            SetSlot(gB, pB, sB, a);
            NotifyGroupChanged(gA);
            if (gB != gA)
                NotifyGroupChanged(gB);
        }

        public HotbarGroupSnapshot CreateSnapshot(string groupId)
        {
            if (!TryGetGroupState(groupId, out var g))
                return null;

            var snap = new HotbarGroupSnapshot
            {
                version = 1,
                groupId = g.groupId,
                slotsPerPage = g.slotsPerPage,
                pageCount = g.pageCount,
                slots = new SlotBinding[g.pageCount * g.slotsPerPage]
            };

            for (int p = 0; p < g.pageCount; p++)
                for (int s = 0; s < g.slotsPerPage; s++)
                    snap.slots[Idx(p, s, g.slotsPerPage)] = g.pages[p][s];

            return snap;
        }

        public void ApplySnapshot(HotbarGroupSnapshot snap, bool validate = true)
        {
            if (snap == null || string.IsNullOrWhiteSpace(snap.groupId))
                return;

            if (!TryGetGroupState(snap.groupId, out var g))
                return;

            // Clear the entire group first
            for (int p = 0; p < g.pageCount; p++)
                for (int s = 0; s < g.slotsPerPage; s++)
                    g.pages[p][s] = new SlotBinding { kind = SlotKind.Empty, id = "" };

            // Nothing to apply
            if (snap.slots == null || snap.slots.Length == 0)
            {
                NotifyGroupChanged(g.groupId);
                return;
            }

            // Apply only overlapping region (handles shape changes safely)
            int pagesToCopy = Mathf.Min(g.pageCount, snap.pageCount);
            int slotsToCopy = Mathf.Min(g.slotsPerPage, snap.slotsPerPage);

            for (int p = 0; p < pagesToCopy; p++)
                for (int s = 0; s < slotsToCopy; s++)
                {
                    int srcIndex = Idx(p, s, snap.slotsPerPage);
                    if (srcIndex < 0 || srcIndex >= snap.slots.Length)
                        continue;

                    var b = snap.slots[srcIndex];

                    if (validate)
                        b = ValidateBinding(b);

                    g.pages[p][s] = b;
                }

            NotifyGroupChanged(g.groupId);
        }

        public void ExecuteSlot(string groupId, int slotIndex)
        {
            Debug.Log($"Executing slot {slotIndex} in group {groupId} with binding {GetBinding(groupId, slotIndex).kind} ({GetBinding(groupId, slotIndex).id})");
            var binding = GetBinding(groupId, slotIndex);
            ExecuteBinding(binding);
        }

        public void ExecuteBinding(SlotBinding binding)
        {
            switch (binding.kind)
            {
                case SlotKind.Empty:
                    return;

                case SlotKind.Action:
                {
                    var action = registry.GetById(binding.id);
                    if (!action)
                        return;

                    // Call your existing execution entry point here:
                    // - action.Execute(...)
                    // - actionController.Execute(action)
                    // - executor.Execute(action)
                    ExecuteAction(action);
                    return;
                }

                case SlotKind.Macro:
                {
                    var macro = macroLibrary.Get(binding.id);
                    //if (macro != null)
                    //chat.Send(macro.body); // your existing macro handling
                    return;
                }
            }
        }

        public void UpdateSlots()
        {
            // TODO: make this update every hotbar script so all hotbars get updated when one updates.
        }

        public HotbarGroupDefinition GetGroupDefinition(string groupId)
        {
            foreach (var def in groupDefinitions)
                if (def.groupId == groupId)
                    return def;
            return null;
        }

        private void ExecuteAction(CharacterAction action)
        {
            // Keep this as a thin adapter so you don’t rewrite action logic.
            // Example:
            // action.Execute();

            character.actionController.PerformAction(action);
        }

        private void NotifyGroupChanged(string groupId)
        {
            OnGroupChanged?.Invoke(groupId);
        }

        private SlotBinding ValidateBinding(SlotBinding b)
        {
            // Normalize null-ish ids
            b.id ??= "";

            switch (b.kind)
            {
                case SlotKind.Empty:
                    b.id = "";
                    return b;

                case SlotKind.Action:
                    if (string.IsNullOrWhiteSpace(b.id) || registry.GetById(b.id) == null)
                    {
                        // If you prefer silent fail, remove the log
                        Debug.LogWarning($"Hotbar load: unknown ActionId '{b.id}', clearing slot.");
                        return new SlotBinding { kind = SlotKind.Empty, id = "" };
                    }
                    return b;

                case SlotKind.Macro:
                    if (string.IsNullOrWhiteSpace(b.id) || macroLibrary.Get(b.id) == null)
                    {
                        Debug.LogWarning($"Hotbar load: unknown MacroId '{b.id}', clearing slot.");
                        return new SlotBinding { kind = SlotKind.Empty, id = "" };
                    }
                    return b;

                default:
                    // Unknown kind -> clear
                    return new SlotBinding { kind = SlotKind.Empty, id = "" };
            }
        }

        private static int Idx(int page, int slot, int slotsPerPage) => page * slotsPerPage + slot;

        private bool TryGetGroupState(string groupId, out GroupState state) => groups.TryGetValue(groupId, out state);

        private sealed class GroupState
        {
            public string groupId;
            public int slotsPerPage;
            public int pageCount;
            public int activePage;

            public SlotBinding[][] pages; // [page][slot]
        }
    }
}