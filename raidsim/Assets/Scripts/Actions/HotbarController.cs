// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using System.Collections.Generic;
using UnityEngine;
using dev.susybaka.raidsim.Actions;
using dev.susybaka.raidsim.Attributes;
using dev.susybaka.raidsim.Characters;
using dev.susybaka.raidsim.Core;
using dev.susybaka.raidsim.Inputs;
using dev.susybaka.Shared.Attributes;
using static dev.susybaka.raidsim.Core.GlobalData;

namespace dev.susybaka.raidsim.UI
{
    public class HotbarController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private UserInput input;
        [SerializeField] private CharacterState character;
        [SerializeField] private CharacterActionRegistry registry;
        [SerializeField] private ActionOverrideResolver overrideResolverBehaviour;
        [SerializeField] private MacroEditor macroEditor;
        [SerializeField] private MacroExecutor macroExecutor;
        public bool locked = false;

        public CharacterState Character => character;
        public CharacterActionRegistry Registry => registry;
        public ActionOverrideResolver OverrideResolver => overrideResolverBehaviour;
        public MacroEditor MacroEditor => macroEditor;
        public MacroExecutor MacroExecutor => macroExecutor;

        [Header("Layout")]
        [SerializeField] private HotbarGroupDefinition[] groupDefinitions;
        public HotbarGroupDefinition[] GroupDefinitions => groupDefinitions;

        [Header("Keybinds")]
        [SerializeField] private GroupKeybind[] groupBinds;

        public event System.Action<string> OnGroupChanged;
        public event System.Action OnRefreshHotbars;
        private readonly Dictionary<string, GroupState> groups = new();
        private IActionOverrideResolver overrideResolver;

        [Header("Behavior")]
        [SerializeField] private bool enableActionQueue = true;
        [SerializeField] private float actionQueueWindow = 0.5f;
        [SerializeField] private float actionQueueHoldMax = 2.0f;
        [SerializeField] private bool queueUseUnscaledTime = false;

        // two-slot queue
        private QueuedAction primaryQueued;
        private QueuedAction secondaryQueued;

        [Header("Audio")]
        [SerializeField, SoundName] private string onExecuteSound = "ui_execute_action";
        [SerializeField, Range(0f, 1f)] private float volume = 1f;

        private void Awake()
        {
            input = FightTimeline.Instance.input;
            if (overrideResolverBehaviour == null)
                overrideResolverBehaviour = GetComponentInChildren<ActionOverrideResolver>();
            overrideResolver = overrideResolverBehaviour as IActionOverrideResolver;
            if (macroEditor == null)
                macroEditor = FindFirstObjectByType<MacroEditor>();
            RebuildGroups();
        }

        private void OnEnable()
        {
            overrideResolverBehaviour.OnOverridesChanged += RefreshAllHotbars;
        }

        private void OnDisable()
        {
            overrideResolverBehaviour.OnOverridesChanged -= RefreshAllHotbars;
        }

        private void Update()
        {
            if (input == null)
                return;

            if (TryProcessQueuedActions())
                return; // Only one action per frame

            foreach (var groupBind in groupBinds)
            {
                if (string.IsNullOrWhiteSpace(groupBind.groupId))
                    continue;

                if (!TryGetGroupState(groupBind.groupId, out var groupState))
                    continue;

                if (groupBind.binds == null || groupBind.binds.Length == 0)
                    continue;

                int maxSlots = Mathf.Min(groupBind.binds.Length, groupState.slotsPerPage);

                for (int i = 0; i < maxSlots; i++)
                {
                    string bind = groupBind.binds[i];
                    if (string.IsNullOrWhiteSpace(bind))
                        continue;

                    if (input.GetButtonDown(bind))
                    {
                        groupBind.onPress?.Invoke(i);
                        ExecuteSlot(groupBind.groupId, i, true);
                        return; // Only one action per frame
                    }
                }
            }
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

        public string GetGroupKeybind(string groupId, int slotIndex)
        {
            foreach (GroupKeybind groupBind in groupBinds)
            {
                if (groupBind.groupId == groupId)
                {
                    if (groupBind.binds != null && slotIndex >= 0 && slotIndex < groupBind.binds.Length)
                        return groupBind.binds[slotIndex];
                }
            }
            return null;
        }

        public void AttachToGroupKeybind(string groupId, System.Action<int> callback)
        {
            for (int i = 0; i < groupBinds.Length; i++)
            {
                if (groupBinds[i].groupId == groupId)
                {
                    var bind = groupBinds[i];
                    bind.onPress += callback;
                    groupBinds[i] = bind; // struct, need to re-assign
                    return;
                }
            }
        }

        public void DetachFromGroupKeybind(string groupId, System.Action<int> callback)
        {
            for (int i = 0; i < groupBinds.Length; i++)
            {
                if (groupBinds[i].groupId == groupId)
                {
                    var bind = groupBinds[i];
                    bind.onPress -= callback;
                    groupBinds[i] = bind; // struct, need to re-assign
                    return;
                }
            }
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

        public CharacterAction GetResolvedAction(string actionId, ActionResolveMode mode = ActionResolveMode.Execution)
        {
            // binding.id is the BASE action id stored in the slot
            var effectiveId = ResolveActionIdSafe(actionId, ActionResolveMode.Execution);
            var action = registry.GetById(effectiveId);

            if (action == null)
                action = registry.GetById(actionId); // fallback to un-resolved id if override fails

            return action;
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

        public void ExecuteSlot(string groupId, int slotIndex, bool playSound)
        {
            //Debug.Log($"Executing slot {slotIndex} in group {groupId} with binding {GetBinding(groupId, slotIndex).kind} ({GetBinding(groupId, slotIndex).id}) playSound {playSound} sound {onExecuteSound}");
            var binding = GetBinding(groupId, slotIndex);
            ExecuteBinding(binding);

            if (playSound && !string.IsNullOrEmpty(onExecuteSound) && Shared.Audio.AudioManager.Instance != null)
                Shared.Audio.AudioManager.Instance.Play(onExecuteSound, volume);
        }

        public void ExecuteBinding(SlotBinding binding)
        {
            switch (binding.kind)
            {
                case SlotKind.Empty:
                    return;

                case SlotKind.Action:
                {
                    var action = GetResolvedAction(binding.id);

                    if (!action)
                        return;

                    if (action.isDisabled || action.unavailable)
                        return;

                    if (action.isExecutable)
                    {
                        ExecuteAction(action);
                        ClearQueue(); // clear queued inputs on successful manual execution
                        return;
                    }

                    // Not executable now -> try to queue
                    TryQueueAction(binding.id, false);
                    return;
                }

                case SlotKind.Macro:
                {
                    if (MacroLibrary.TryParseMacroId(binding.id, out int idx))
                    {
                        var entry = macroEditor.Library.Get(idx);
                        macroExecutor.Execute(entry);
                        ClearQueue(); // clear any queued actions on macro execution since macros don't queue themselves but may execute actions, and we want to prevent queued actions from firing after a macro runs
                    }
                    return;
                }
            }
        }

        public void RefreshAllHotbars()
        {
            OnRefreshHotbars.Invoke();
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
            character.actionController.PerformAction(action);
        }

        private void NotifyGroupChanged(string groupId)
        {
            OnGroupChanged?.Invoke(groupId);
        }

        private string ResolveActionIdSafe(string baseId, ActionResolveMode mode)
        {
            if (overrideResolver == null || string.IsNullOrWhiteSpace(baseId))
                return baseId;

            // Follow override chain up to N steps (prevents infinite loops)
            const int maxHops = 4;
            string current = baseId;

            for (int i = 0; i < maxHops; i++)
            {
                string next = overrideResolver.ResolveActionId(current, mode);
                if (string.IsNullOrWhiteSpace(next) || next == current)
                    break;

                current = next;
            }

            return current;
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
                    if (string.IsNullOrWhiteSpace(b.id) || (MacroLibrary.TryParseMacroId(b.id, out int idx) && !macroEditor.Library.IsValid(idx)))
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

        private bool TryProcessQueuedActions()
        {
            if (!enableActionQueue)
                return false;

            // Expire old entries
            float now = QueueNow;
            if (primaryQueued.isValid && now > primaryQueued.expiresAt)
                ClearPrimary();
            if (secondaryQueued.isValid && now > secondaryQueued.expiresAt)
                ClearSecondary();

            // If primary exists and is ready, execute it immediately
            if (primaryQueued.isValid)
            {
                var action = GetResolvedAction(primaryQueued.baseActionId);
                if (action && !action.isDisabled && !action.unavailable && action.isExecutable)
                {
                    ExecuteAction(action);

                    if (primaryQueued.playSound && !string.IsNullOrEmpty(onExecuteSound) && Shared.Audio.AudioManager.Instance != null)
                        Shared.Audio.AudioManager.Instance.Play(onExecuteSound, volume);

                    // Promote secondary -> primary after primary fires
                    primaryQueued = secondaryQueued;
                    secondaryQueued = default;

                    return true;
                }

                // Primary not ready yet, keep it
                return false;
            }

            // No primary, but secondary exists:
            // Only promote to primary when it is within the queue window
            if (secondaryQueued.isValid)
            {
                var action = GetResolvedAction(secondaryQueued.baseActionId);
                if (action && !action.isDisabled && !action.unavailable && !action.isExecutable)
                {
                    float remaining = GetTimeUntilExecutable(action);
                    if (remaining <= actionQueueWindow)
                    {
                        primaryQueued = secondaryQueued;
                        secondaryQueued = default;
                    }
                }
                else if (action && action.isExecutable)
                {
                    // If it's already executable, you can just fire it now OR promote then fire next frame.
                    // To keep strict "one per frame", promote and let next Update fire it.
                    primaryQueued = secondaryQueued;
                    secondaryQueued = default;
                }
            }

            return false;
        }

        private bool TryQueueAction(string baseActionId, bool playSound)
        {
            if (!enableActionQueue || string.IsNullOrWhiteSpace(baseActionId))
                return false;

            var action = GetResolvedAction(baseActionId);
            if (!action)
                return false;

            // Don't queue disabled/unavailable actions
            if (action.isDisabled || action.unavailable)
                return false;

            // If it can execute now, do not queue
            if (action.isExecutable)
                return false;

            // Only queue if it's close enough to ready
            float remaining = GetTimeUntilExecutable(action);
            if (remaining > actionQueueWindow)
                return false;

            var entry = new QueuedAction
            {
                baseActionId = baseActionId,
                expiresAt = QueueNow + actionQueueHoldMax,
                playSound = playSound
            };

            // Primary is locked; never override it
            if (!primaryQueued.isValid)
                primaryQueued = entry;
            else
                secondaryQueued = entry; // secondary can be overwritten by new presses

            return true;
        }

        private float GetTimeUntilExecutable(CharacterAction action)
        {
            if (action == null)
                return float.PositiveInfinity;

            return Mathf.Max(0f, action.timeUntilExecutable);
        }

        private void ClearQueue()
        {
            ClearPrimary();
            ClearSecondary();
        }

        private float QueueNow => queueUseUnscaledTime ? Time.unscaledTime : Time.time;

        private void ClearPrimary() => primaryQueued = default;
        private void ClearSecondary() => secondaryQueued = default;

        private sealed class GroupState
        {
            public string groupId;
            public int slotsPerPage;
            public int pageCount;
            public int activePage;

            public SlotBinding[][] pages; // [page][slot]
        }

        [System.Serializable]
        public struct GroupKeybind
        {
            public string groupId;
            [KeybindName] public string[] binds;
            [System.NonSerialized] public System.Action<int> onPress;
        }

        private struct QueuedAction
        {
            public string baseActionId;
            public float expiresAt;
            public bool playSound;
            public bool isValid => !string.IsNullOrEmpty(baseActionId);

            public QueuedAction(string baseActionId, float expiresAt, bool playSound)
            {
                this.baseActionId = baseActionId;
                this.expiresAt = expiresAt;
                this.playSound = playSound;
            }
        }
    }
}