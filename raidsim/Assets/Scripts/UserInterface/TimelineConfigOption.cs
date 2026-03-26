// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using dev.susybaka.raidsim.SaveLoad;
using dev.susybaka.Shared;

namespace dev.susybaka.raidsim.UI
{
    public class TimelineConfigOption : MonoBehaviour
    {
        [SerializeField] private string key = "New Timeline Config Option";
        [SerializeField] private TMP_Dropdown dropdown;
        [SerializeField] private SaveButton button;
        [SerializeField] private Toggle toggle;
        private SaveDropdown saveDropdown;
        private SaveToggle saveToggle;

        private void Awake()
        {
            if (dropdown != null)
            {
                saveDropdown = dropdown.GetComponent<SaveDropdown>();
            }
            if (toggle != null)
            {
                saveToggle = toggle.GetComponent<SaveToggle>();
            }
        }

        public string GetSelectedOption()
        {
            if (dropdown != null)
            {
                return $"{key}.d.{dropdown.value}";
            }
            else if (toggle != null)
            {
                return $"{key}.t.{toggle.isOn.ToInt()}";
            }
            else
            {
                return $"{key}.b.{button.GetValue().ToInt()}";
            }
        }

        public void SetSelectedOption(string value)
        {
            string[] parts = value.Split('.');
            
            if (parts.Length != 3 || parts[0] != key)
                return;
            
            if (dropdown != null && parts[1] == "d" && int.TryParse(parts[2], out int dropdownValue))
            {
                dropdown.value = dropdownValue;
                dropdown.RefreshShownValue();
                dropdown.onValueChanged.Invoke(dropdownValue);
                saveDropdown?.SaveValue(dropdownValue);
            }
            else if (toggle != null && parts[1] == "t" && int.TryParse(parts[2], out int toggleValue))
            {
                bool tValue = toggleValue.ToBool();
                toggle.isOn = tValue;
                toggle.onValueChanged.Invoke(tValue);
                saveToggle?.SaveValue(tValue);
            }
            else if (button != null && parts[1] == "b" && int.TryParse(parts[2], out int buttonValue))
            {
                bool bValue = buttonValue.ToBool();
                button.SetValue(bValue);
            }
        }
    }
}