// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;
using dev.susybaka.raidsim.Actions;
using dev.susybaka.raidsim.Characters;
using dev.susybaka.raidsim.Core;
using dev.susybaka.raidsim.Inputs;
using dev.susybaka.raidsim.SaveLoad;
using static dev.susybaka.raidsim.Core.GlobalData;

namespace dev.susybaka.raidsim.UI
{
    [RequireComponent(typeof(HudWindow))]
    public class ActionsMenu : MonoBehaviour
    {
        private HudWindow window;
        private UserInput input;

        [SerializeField] private CharacterState character;
        [SerializeField] private ActionController actionController;
        [SerializeField] private HotbarController hotbarController;
        [SerializeField] private SaveHotbar mainHotbar;
        [SerializeField] private PresetHotbarItem itemPrefab;
        [SerializeField] private Transform standardActionParent;
        [SerializeField] private Transform roleActionParent;
        [SerializeField] private Transform generalActionParent;
        [SerializeField] private Transform blueActionParent;
        [SerializeField] private TextMeshProUGUI roleIndicator;

        public UnityEvent onActionsRegistered;

        private readonly List<PresetHotbarItem> items = new List<PresetHotbarItem>();

        private void Start()
        {
            window = GetComponent<HudWindow>();
            input = FightTimeline.Instance.input;

            window.CloseWindow();

            foreach (CharacterAction action in hotbarController.Registry.AllActions)
            {
                // Skip hidden actions, as they are not meant to be shown in the UI or assigned to hotbars.
                if (action.Kind == CharacterAction.ActionKind.Hidden)
                    continue;

                // Determine the parent transform under which the visuals will be spawned.
                Transform parent = action.Kind switch
                {
                    CharacterAction.ActionKind.Standard => standardActionParent,
                    CharacterAction.ActionKind.Role => roleActionParent,
                    CharacterAction.ActionKind.General => generalActionParent,
                    CharacterAction.ActionKind.BlueMage => blueActionParent,
                    _ => standardActionParent
                };

                PresetHotbarItem item = Instantiate(itemPrefab, parent);
                item.Initialize(hotbarController, null, new SlotBinding { kind = SlotKind.Action, id = action.ActionId });
                item.OnClick += OnClick;
                items.Add(item);
            }
            onActionsRegistered.Invoke();

            if (roleIndicator != null)
            {
                roleIndicator.text = $"Lv{character.characterLevel} {GlobalVariables.roleNames[(int)character.role]}";
            }
        }

        private void OnEnable()
        {
            hotbarController.OnRefreshHotbars += RefreshAll;
        }

        private void OnDisable()
        {
            hotbarController.OnRefreshHotbars -= RefreshAll;
        }

        private void Update()
        {
            if (input == null)
                input = FightTimeline.Instance.input;

            if (input.GetButtonDown("ToggleActionsMenu"))
            {
                if (window.isOpen)
                    window.CloseWindow();
                else
                    window.OpenWindow();
            }

            if (window.isOpen && Shared.Utilities.RateLimiter(30))
            {
                if (roleIndicator != null)
                {
                    roleIndicator.text = $"Lv{character.characterLevel} {GlobalVariables.roleNames[(int)character.role]}";
                }
            }
        }

        public void RefreshAll()
        {
            foreach (PresetHotbarItem item in items)
            {
                item.RefreshStaticVisuals();
            }
        }

        public void ResetHotbar()
        {
            mainHotbar.LoadDefaults();
        }

        private void OnClick(SlotBinding binding)
        {
            if (binding.kind != SlotKind.Action)
                return;

            if (actionController == null || hotbarController == null || hotbarController.Registry == null)
                return;

            actionController.PerformAction(hotbarController.GetResolvedAction(binding.id, ActionResolveMode.Execution));
        }
    }
}