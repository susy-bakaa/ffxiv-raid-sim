// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using dev.illa4257;
using dev.susybaka.raidsim.Core;
using dev.susybaka.raidsim.UI;
using dev.susybaka.Shared;

namespace dev.susybaka.raidsim.SaveLoad
{
    public class SaveHudWindowState : MonoBehaviour
    {
        HudWindow hudWindow;
        // For now tack on minimap visibility saving to this since it's also a HUD element,
        // but doesn't use the Hudwindow script and this way we don't need to add a separate component for the minimap.
        MinimapHandler minimapHandler;
        int savedValue = 0;

        public string group = "";
        public string key = "UnnamedHudWindow";
        public bool defaultValue = false;
        public bool runInAwake = false;
        public bool runOnReset = false;
        public bool useTimelineGroup = false;

        public UnityEvent<bool> onStart;

        IniStorage ini;
        int id = 0;
        float wait;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (useTimelineGroup)
                group = string.Empty;
        }
#endif

        private void Awake()
        {
            if (useTimelineGroup && FightTimeline.Instance != null)
            {
                group = FightTimeline.Instance.timelineAbbreviation;
            }

            hudWindow = GetComponent<HudWindow>();
            minimapHandler = GetComponent<MinimapHandler>();

            savedValue = 0;
            ini = new IniStorage(GlobalVariables.configPath);
            wait = UnityEngine.Random.Range(0.15f, 0.65f);
            id = Random.Range(0, 10000);

            if (runInAwake)
            {
                Begin();
            }
        }

        private void Start()
        {
            if (runOnReset && FightTimeline.Instance != null)
            {
                FightTimeline.Instance.onReset.AddListener(Begin);
            }

            if (!runInAwake)
            {
                Begin();
            }
        }

        private void OnEnable()
        {
            if (hudWindow != null)
            {
                hudWindow.onOpen.AddListener(SaveValueTrue);
                hudWindow.onClose.AddListener(SaveValueFalse);
            }
            // For now tack on minimap visibility saving to this since it's also a HUD element,
            // but doesn't use the Hudwindow script and this way we don't need to add a separate component for the minimap.
            if (minimapHandler != null)
            {
                minimapHandler.onToggleMinimapVisibility.AddListener(SaveValue);
            }
        }

        private void OnDisable()
        {
            if (hudWindow != null)
            {
                hudWindow.onOpen.RemoveListener(SaveValueTrue);
                hudWindow.onClose.RemoveListener(SaveValueFalse);
            }
            // For now tack on minimap visibility saving to this since it's also a HUD element,
            // but doesn't use the Hudwindow script and this way we don't need to add a separate component for the minimap.
            if (minimapHandler != null)
            {
                minimapHandler.onToggleMinimapVisibility.RemoveListener(SaveValue);
            }
        }

        private void OnDestroy()
        {
            if (FightTimeline.Instance != null && runOnReset)
            {
                FightTimeline.Instance.onReset.RemoveListener(Begin);
            }
        }

        private void Begin()
        {
            if (ini.Contains(group, $"i{key}"))
            {
                savedValue = ini.GetInt(group, $"i{key}");

                bool result = savedValue.ToBool();

                if (result)
                {
                    if (hudWindow != null)
                    {
                        hudWindow.OpenWindow();
                    }
                    // For now tack on minimap visibility saving to this since it's also a HUD element,
                    // but doesn't use the Hudwindow script and this way we don't need to add a separate component for the minimap.
                    if (minimapHandler != null)
                    {
                        minimapHandler.ToggleVisible(true);
                    }
                }
                else
                {
                    if (hudWindow != null)
                    {
                        hudWindow.CloseWindow();
                    }
                    // For now tack on minimap visibility saving to this since it's also a HUD element,
                    // but doesn't use the Hudwindow script and this way we don't need to add a separate component for the minimap.
                    if (minimapHandler != null)
                    {
                        minimapHandler.ToggleVisible(false);
                    }
                }

                onStart.Invoke(result);
            }
            else
            {
                onStart.Invoke(defaultValue);
            }
        }

        private void SaveValueTrue()
        {
            SaveValue(true);
        }

        private void SaveValueFalse()
        {
            SaveValue(false);
        }

        public void SaveValue(bool value)
        {
            string n = gameObject.name;
            if (hudWindow != null)
                n = hudWindow.gameObject.name;
            Utilities.FunctionTimer.Create(this, () => {
                ini.Load(GlobalVariables.configPath);

                savedValue = value.ToInt();
                ini.Set(group, $"i{key}", savedValue);

                ini.Save();
            }, wait, $"SaveHudWindowState_{id}_{n}_savevalue_delay", false, true);
        }
    }
}