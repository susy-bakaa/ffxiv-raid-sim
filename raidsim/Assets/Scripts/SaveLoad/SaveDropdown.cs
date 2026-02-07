// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using UnityEngine;
using UnityEngine.Events;
using TMPro;
using dev.illa4257;
using dev.susybaka.raidsim.Core;
using dev.susybaka.Shared;
using Random = UnityEngine.Random;

namespace dev.susybaka.raidsim.SaveLoad
{
    [RequireComponent(typeof(TMP_Dropdown))]
    public class SaveDropdown : MonoBehaviour
    {
        TMP_Dropdown dropdown;
        int savedValue = 0;

        public string group = "";
        public string key = "UnnamedDropdown";
        public bool useTimelineGroup = false;

        public UnityEvent<int> onStart;

        IniStorage ini;
        int id = -1;

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

            id = Random.Range(0, 10000);
            dropdown = GetComponent<TMP_Dropdown>();
            savedValue = 0;
            ini = new IniStorage(GlobalVariables.configPath);
        }

        private void Start()
        {
            Utilities.FunctionTimer.Create(this, () => OnStart(), Random.Range(1f, 1.25f), $"{group}_{key}_dropdown_{id}_onstart_delay", true, true);
        }

        private void OnStart()
        {
            if (ini.Contains(group, $"i{key}"))
            {
                savedValue = ini.GetInt(group, $"i{key}");
                dropdown.value = savedValue;
                dropdown.RefreshShownValue();
                dropdown.onValueChanged.Invoke(savedValue);
                onStart.Invoke(savedValue);
            }
        }

        public void LoadSavedValue()
        {
            if (id < 0)
                return;

            OnStart();
        }

        public void SaveValue(int value)
        {
            ini.Load(GlobalVariables.configPath);

            savedValue = value;
            ini.Set(group, $"i{key}", savedValue);

            Utilities.FunctionTimer.Create(this, () => ini.Save(), 0.5f, $"{group}_{key}_dropdown_savevalue_delay", true, false);
        }
    }
}