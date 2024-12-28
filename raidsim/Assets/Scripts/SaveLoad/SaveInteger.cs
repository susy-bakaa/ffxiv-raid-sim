using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SaveInteger : MonoBehaviour
{
    int savedValue = 0;

    public string group = "";
    public string key = "UnnamedInteger";

    public UnityEvent<int> onStart;

    IniStorage ini;
    int id = 0;
    float wait;

    void Awake()
    {
        savedValue = 0;
        ini = new IniStorage(GlobalVariables.configPath);
        wait = UnityEngine.Random.Range(0.15f, 0.65f);
        id = Random.Range(0, 10000);
    }

    void Start()
    {
        if (ini.Contains(group, $"i{key}"))
        {
            savedValue = ini.GetInt(group, $"i{key}");
        }

        onStart.Invoke(savedValue);
    }

    public void SaveValue(int value)
    {
        string n = gameObject.name;
        Utilities.FunctionTimer.Create(this, () => {
            ini.Load(GlobalVariables.configPath);

            savedValue = value;
            ini.Set(group, $"i{key}", savedValue);

            ini.Save();
        }, wait, $"SaveInteger_{id}_{n}_savevalue_delay", false, true);
    }
}
