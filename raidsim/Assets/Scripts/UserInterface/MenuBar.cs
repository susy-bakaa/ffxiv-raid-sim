// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using UnityEngine;
using UnityEngine.UI;
using dev.susybaka.raidsim.Core;
using dev.susybaka.raidsim.Inputs;

namespace dev.susybaka.raidsim.UI
{
    public class MenuBar : MonoBehaviour
    {
        [SerializeField] private UserInput input;
        [SerializeField] private Button reloadButton;
        [SerializeField] private Button resetButton;
        [SerializeField] private Button playButton;
        [SerializeField] private Button pauseButton;
        [SerializeField] private Button settingsButton;

        private void Awake()
        {
            input = FightTimeline.Instance.input;
        }

        private void Update()
        {
            if (input == null)
                return;

            if (input.GetButtonDown("ReloadKey"))
            {
                ClickButton(reloadButton);
            }
            if (input.GetButtonDown("PlayKey"))
            {
                ClickButton(playButton);
            }
            if (input.GetButtonDown("PauseKey"))
            {
                ClickButton(pauseButton);
            }
            if (input.GetButtonDown("OpenSettingsKey"))
            {
                OpenSettings();
            }
        }

        public void OpenSettings()
        {
            ClickButton(settingsButton);
        }

        public void ClickButton(Button button)
        {
            if (button == null || button.interactable == false)
                return;
            button.onClick.Invoke();
        }
    }
}