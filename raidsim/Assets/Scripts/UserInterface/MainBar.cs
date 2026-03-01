// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace dev.susybaka.raidsim.UI
{
    public class MainBar : MonoBehaviour
    {
        [SerializeField] private List<BarEntry> entries;
        [SerializeField] private ActionsMenu actionsMenu;
        [SerializeField] private MacroEditor macroEditor;
        [SerializeField] private ConfigMenu configMenu;
        [SerializeField] private TimelineConfigMenu timelineConfigMenu;
        [SerializeField] private PauseMenu pauseMenu;
        [SerializeField] private HudEditor hudEditor;

        private void Awake()
        {
            foreach (var entry in entries)
            {
                var contents = entry.contentsParent;
                entry.openButton.onClick.AddListener(() =>
                {
                    if (contents != null)
                        SetActive(contents, !contents.activeSelf);
                    CloseContentMenus(contents);
                });
                if (contents != null)
                    contents.SetActive(false);
            }
        }

        public void OpenActionsMenu()
        {
            actionsMenu.Window.OpenWindow();
            CloseContentMenus();
        }

        public void OpenMacroEditor()
        {
            macroEditor.Open(0);
            CloseContentMenus();
        }

        public void OpenConfigMenu()
        {
            configMenu.OpenWindow();
            CloseContentMenus();
        }

        public void OpenTimelineConfigMenu()
        {
            timelineConfigMenu.OpenWindow();
            CloseContentMenus();
        }

        public void OpenKeybinds()
        {
            configMenu.OpenWindow();
            configMenu.OpenKeybinds();
            CloseContentMenus();
        }

        public void OpenHudEditor()
        {
            hudEditor.ToggleHudEditor(true);
            CloseContentMenus();
        }

        public void QuitApplication()
        {
            pauseMenu.OpenWindow();
            CloseContentMenus();
        }

        private void CloseContentMenus(GameObject excluded = null)
        {
            foreach (var entry in entries)
            {
                var contents = entry.contentsParent;
                if (contents == null)
                    continue;
                if (excluded != null && contents == excluded)
                    continue;
                SetActive(contents, false);
            }
        }

        private void SetActive(GameObject target, bool state)
        {
            target.SetActive(state);
        }

        [System.Serializable]
        public struct BarEntry
        {
            public string name;
            public Button openButton;
            public GameObject contentsParent;
        }
    }
}