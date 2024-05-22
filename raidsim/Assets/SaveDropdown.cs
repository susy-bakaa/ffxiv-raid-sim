using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Dropdown))]
public class SaveDropdown : MonoBehaviour
{
    TMP_Dropdown dropdown;
    int savedValue = 0;
    
    public string group = "";
    public string key = "UnnamedDropdown";

    IniStorage ini;

    void Awake()
    {
        dropdown = GetComponent<TMP_Dropdown>();
        savedValue = 0;
        ini = new IniStorage(GlobalVariables.configPath);
    }

    void Start()
    {
        if (ini.Contains(group, key))
            savedValue = ini.GetInt(group, key);

        dropdown.value = savedValue;
        dropdown.RefreshShownValue();
        dropdown.onValueChanged.Invoke(savedValue);
    }

    public void SaveValue(int value)
    {
        savedValue = value;
        ini.Set(group, key, savedValue);
        ini.Save();
    }
}
