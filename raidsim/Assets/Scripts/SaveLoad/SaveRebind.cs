using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Samples.RebindUI;

public class SaveRebind : MonoBehaviour
{
    public InputActionAsset actions;
    string savedValue = string.Empty;

    public string group = "";
    public string key = "UnnamedRebind";

    public UnityEvent<string> onEnable;

    IniStorage ini;
    int id = 0;
    float wait;
    private List<RebindActionUI> rebinds; 

    void Awake()
    {
        savedValue = string.Empty;
        ini = new IniStorage(GlobalVariables.configPath);
        wait = UnityEngine.Random.Range(0.15f, 0.65f);
        id = Random.Range(0, 10000);
        rebinds = new List<RebindActionUI>(GetComponentsInChildren<RebindActionUI>());
    }

    void OnEnable()
    {
        LoadValue();
    }

    public void LoadValue()
    {
        if (ini.Contains(group, $"s{key}"))
        {
            savedValue = ini.GetString(group, $"s{key}");
            if (!string.IsNullOrEmpty(savedValue))
                actions.LoadBindingOverridesFromJson(savedValue);
        }
        onEnable.Invoke(savedValue);
    }

    public void SaveValue()
    {
        SaveValue(string.Empty);
    }

    public void SaveValue(string value)
    {
        savedValue = actions.SaveBindingOverridesAsJson();

        if (!string.IsNullOrEmpty(value))
            savedValue = value;

        if (savedValue.Contains("reset_json123"))
        {
            savedValue = string.Empty;
            actions.RemoveAllBindingOverrides();
            if (rebinds != null && rebinds.Count > 0)
            {
                foreach (var rebind in rebinds)
                {
                    rebind.ResetToDefault();
                }
            }
        }

        string n = gameObject.name;
        Utilities.FunctionTimer.Create(this, () => {
            ini.Load(GlobalVariables.configPath);

            ini.Set(group, $"s{key}", savedValue);

            ini.Save();
        }, wait, $"SaveRebind_{id}_{n}_savevalue_delay", false, true);
    }
}
