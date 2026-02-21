// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using UnityEngine;
using dev.susybaka.raidsim.Actions;
using dev.susybaka.raidsim.Characters;
using dev.susybaka.raidsim.Core;
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

        public CharacterState Character => character;
        public CharacterActionRegistry Registry => registry;
        public MacroLibrary MacroLibrary => macroLibrary;
        public ChatHandler Chat => chat;

        [Header("Layout")]
        [SerializeField] private int slotCount = 25;

        [Header("Temp keybinds (replace with your input system later)")]
        [SerializeField] private KeyCode[] slotKeys; // length slotCount

        private SlotBinding[] slots;

        public int SlotCount => slotCount;

        private void Awake()
        {
            chat = ChatHandler.Instance;

            slots = new SlotBinding[slotCount];
            if (slotKeys == null || slotKeys.Length != slotCount)
                slotKeys = new KeyCode[slotCount];
        }

        private void Update()
        {
            // Minimal keybind support; replace with your input layer later
            for (int i = 0; i < slotCount; i++)
            {
                var key = slotKeys[i];
                if (key != KeyCode.None && Input.GetKeyDown(key))
                    ExecuteSlot(i);
            }
        }

        public SlotBinding GetBinding(int index) => slots[index];

        public void SetSlot(int index, SlotBinding binding) => slots[index] = binding;

        public void ClearSlot(int index) => slots[index] = new SlotBinding { kind = SlotKind.Empty, id = "" };

        public void SwapSlots(int a, int b)
        {
            (slots[a], slots[b]) = (slots[b], slots[a]);
        }

        public void ExecuteSlot(int index)
        {
            var binding = slots[index];
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

        private void ExecuteAction(CharacterAction action)
        {
            // Keep this as a thin adapter so you don’t rewrite action logic.
            // Example:
            // action.Execute();

            character.actionController.PerformAction(action);
        }
    }
}