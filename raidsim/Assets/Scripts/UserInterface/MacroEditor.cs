// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using dev.susybaka.raidsim.Actions;
using dev.susybaka.raidsim.Characters;
using dev.susybaka.raidsim.Core;
using dev.susybaka.raidsim.Inputs;
using TMPro;
using UnityEngine;

namespace dev.susybaka.raidsim.UI
{
    [RequireComponent(typeof(HudWindow))]
    public sealed class MacroEditor : MonoBehaviour
    {
        private HudWindow hudWindow;

        [SerializeField] private UserInput input;
        [SerializeField] private CharacterState character;
        [SerializeField] private HotbarController hotbarController;
        [SerializeField] private MacroLibrary library;
        [SerializeField] private MacroIconResolver resolver;
        public MacroLibrary Library => library;
        public MacroIconResolver Resolver => resolver;

        [Header("UI")]
        [SerializeField] private TMP_InputField nameField;
        [SerializeField] private TMP_InputField bodyField;
        [SerializeField] private PresetHotbarItem iconField;
        [SerializeField] private TextMeshProUGUI roleIndicator;
        [SerializeField] private HudWindow macroIconPickerPopup;
        [SerializeField] private MacroSlot macroSlotPrefab;
        [SerializeField] private Transform macroSlotParent;


        // Optional icon UI: dropdown/picker for custom icons
        [SerializeField] private MacroIconMode defaultIconMode = MacroIconMode.Default;

        public System.Action<int> OnMacroSelected;
        private int currentIndex = -1;

        private void Awake()
        {
            hudWindow = GetComponent<HudWindow>();
            input = FightTimeline.Instance.input;

            for (int i = 0; i < MacroLibrary.Count; i++)
            {
                var slot = Instantiate(macroSlotPrefab, macroSlotParent);
                int index = i; // Capture loop variable
                slot.Initialize(this, hotbarController, index);
                slot.OnEditRequested += Open;
                OnMacroSelected += slot.HandleChanged;
            }

            if (roleIndicator != null)
            {
                roleIndicator.text = $"Lv{character.characterLevel} {GlobalVariables.roleNames[(int)character.role]}";
            }

            macroIconPickerPopup.CloseWindow();
            hudWindow.CloseWindow();
        }

        private void Update()
        {
            if (input.GetButtonDown("ToggleMacroEditorKey"))
            {
                ToggleEditor();
            }

            if (hudWindow.isOpen && Shared.Utilities.RateLimiter(30))
            {
                if (roleIndicator != null)
                {
                    roleIndicator.text = $"Lv{character.characterLevel} {GlobalVariables.roleNames[(int)character.role]}";
                }
            }
        }

        public void ToggleEditor()
        {
            if (hudWindow.isOpen)
            {
                Close();
            }
            else
            {
                Open(0); // Open first macro by default
            }
        }

        public void Open(int macroIndex)
        {
            currentIndex = macroIndex;
            var e = library.Get(macroIndex);

            nameField.text = e.name ?? "";
            bodyField.text = e.body ?? "";
            if (!hudWindow.isOpen)
                hudWindow.OpenWindow();
            OnMacroSelected?.Invoke(macroIndex);
        }

        public void Close()
        {
            if (hudWindow.isOpen)
                hudWindow.CloseWindow();
            currentIndex = -1;
        }

        public int GetCurrentIndex() => currentIndex;

        public void Save()
        {
            if (currentIndex < 0)
                return;

            var name = nameField.text ?? "";
            var body = MacroParsing.ClampTo15Lines(bodyField.text ?? "");

            bool valid = !string.IsNullOrWhiteSpace(name) || !string.IsNullOrWhiteSpace(body);

            var e = library.Get(currentIndex);
            e.isValid = valid;
            e.name = name;
            e.body = body;

            if (!valid)
            {
                e.iconMode = MacroIconMode.Default;
                e.customIconId = "";
                e.actionIconId = "";
                library.Set(currentIndex, e);
                return;
            }

            // Default icon mode if none set yet
            if (e.iconMode == 0)
                e.iconMode = defaultIconMode;

            // Auto-detect /micon or /macroicon
            if (MacroParsing.TryExtractMiconActionId(body, out var actionId))
            {
                e.iconMode = MacroIconMode.ActionIcon;
                e.actionIconId = actionId;
                // You can optionally clear customIconId here
            }

            library.Set(currentIndex, e);
        }

        // Optional: button to set custom icon
        public void SetCustomIcon(string iconId)
        {
            if (currentIndex < 0)
                return;
            var e = library.Get(currentIndex);
            e.isValid = true;
            e.iconMode = MacroIconMode.CustomSprite;
            e.customIconId = iconId ?? "";
            library.Set(currentIndex, e);
            Save();
        }

        public void ClearIconToDefault()
        {
            if (currentIndex < 0)
                return;
            var e = library.Get(currentIndex);
            e.iconMode = MacroIconMode.Default;
            e.customIconId = "";
            e.actionIconId = "";
            library.Set(currentIndex, e);
            Save();
        }

        public void UpdateIconPreview(PresetHotbarItem preset)
        {
            if (iconField == null)
                return;

            iconField.Copy(preset);
        }

        public void OpenIconPicker()
        {
            if (currentIndex < 0)
                return;

            macroIconPickerPopup.OpenWindow();
        }

        public void CloseIconPicker()
        {
            macroIconPickerPopup.CloseWindow(); 
        }
    }
}