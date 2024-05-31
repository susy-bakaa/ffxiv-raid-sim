using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(TMP_Dropdown))]
public class SaveDropdown : MonoBehaviour
{
    TMP_Dropdown dropdown;
    int savedValue = 0;
    
    public string group = "";
    public string key = "UnnamedDropdown";

    public UnityEvent<int> onStart;

    IniStorage ini;

    void Awake()
    {
        dropdown = GetComponent<TMP_Dropdown>();
        savedValue = 0;
        ini = new IniStorage(GlobalVariables.configPath);
    }

    void Start()
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

    public void SaveValue(int value)
    {
        savedValue = value;
        ini.Set(group, $"i{key}", savedValue);
        ini.Save();
    }
}
