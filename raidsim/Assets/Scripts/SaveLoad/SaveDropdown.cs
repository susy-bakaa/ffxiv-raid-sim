using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

[RequireComponent(typeof(TMP_Dropdown))]
public class SaveDropdown : MonoBehaviour
{
    TMP_Dropdown dropdown;
    int savedValue = 0;
    
    public string group = "";
    public string key = "UnnamedDropdown";

    public UnityEvent<int> onStart;

    IniStorage ini;
    int id = 0;

    void Awake()
    {
        id = Random.Range(0, 10000);
        dropdown = GetComponent<TMP_Dropdown>();
        savedValue = 0;
        ini = new IniStorage(GlobalVariables.configPath);
    }

    void Start()
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

    public void SaveValue(int value)
    {
        ini.Load(GlobalVariables.configPath);

        savedValue = value;
        ini.Set(group, $"i{key}", savedValue);

        Utilities.FunctionTimer.Create(this, () => ini.Save(), 0.5f, $"{group}_{key}_dropdown_savevalue_delay", true, false);
    }
}