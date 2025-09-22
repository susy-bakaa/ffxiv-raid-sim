// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using dev.susybaka.raidsim.Actions;
using dev.susybaka.raidsim.Core;
using dev.susybaka.raidsim.Targeting;

namespace dev.susybaka.raidsim.UI
{
    public class PauseMenu : HudWindow
    {
        [Header("Pause Menu Settings")]
        public HudEditor hudEditor;
        public TargetController playerTargeting;
        public ActionController playerActions;
        public string menuScene = "menu";
        public UnityEvent onPaused;
        public UnityEvent onUnpaused;

        protected override void Awake()
        {
            base.Awake();
            
            if (playerTargeting == null)
                playerTargeting = GameObject.Find("Player")?.GetComponent<TargetController>();
            if (playerActions == null)
                playerActions = GameObject.Find("Player")?.GetComponent<ActionController>();
            if (hudEditor == null)
                hudEditor = FindObjectOfType<HudEditor>();
            if (playerTargeting != null)
                playerTargeting.SetPauseMenu(this);
        }

        public void ExitToMainMenu()
        {
            if (FightTimeline.Instance != null)
                FightTimeline.Instance.ResetPauseState();
            SceneManager.LoadScene(menuScene);
        }

        public void TogglePauseMenu()
        {
            if (isOpen)
                ClosePauseMenu();
            else
                OpenPauseMenu();
        }

        public void ClosePauseMenu()
        {
            CloseWindow();
            FightTimeline.Instance.TogglePause(false, "pause_menu");
            onUnpaused.Invoke();
        }

        public void OpenPauseMenu()
        {
            if (playerTargeting.currentTarget != null)
                return;
            if (hudEditor != null && hudEditor.isEditorOpen)
                return;
            if (hudEditor != null && hudEditor.isMenuOpen)
                return;
            if (playerActions.isGroundTargeting)
                return;

            OpenWindow();
            FightTimeline.Instance.TogglePause(true, "pause_menu");
            onPaused.Invoke();
        }

        public void ClickButton(Button button)
        {
            if (button == null || button.interactable == false)
                return;
            button.onClick.Invoke();
        }
    }
}