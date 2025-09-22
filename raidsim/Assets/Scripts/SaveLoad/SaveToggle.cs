// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using dev.illa4257;
using dev.susybaka.raidsim.Core;
using dev.susybaka.Shared;

namespace dev.susybaka.raidsim.SaveLoad
{
    [RequireComponent(typeof(Toggle))]
    public class SaveToggle : MonoBehaviour
    {
        Toggle toggle;
        int savedValue = 0;

        public string group = "";
        public string key = "UnnamedToggle";

        public UnityEvent<bool> onStart;

        IniStorage ini;
        int id = 0;
        float wait;

        private void Awake()
        {
            toggle = GetComponent<Toggle>();
            savedValue = 0;
            ini = new IniStorage(GlobalVariables.configPath);
            wait = UnityEngine.Random.Range(0.15f, 0.65f);
            id = Random.Range(0, 10000);
        }

        private void Start()
        {
            if (ini.Contains(group, $"i{key}"))
            {
                savedValue = ini.GetInt(group, $"i{key}");

                bool result = savedValue.ToBool();

                toggle.SetIsOnWithoutNotify(result);
                toggle.onValueChanged.Invoke(result);
                onStart.Invoke(result);
            }
        }

        public void SaveValue(bool value)
        {
            Utilities.FunctionTimer.Create(this, () => {
                ini.Load(GlobalVariables.configPath);

                savedValue = value.ToInt();
                ini.Set(group, $"i{key}", savedValue);

                ini.Save();
            }, wait, $"SaveToggle_{id}_{toggle.gameObject.name}_savevalue_delay", false, true);
        }
    }
}