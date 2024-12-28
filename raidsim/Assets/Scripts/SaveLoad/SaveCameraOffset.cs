using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(ThirdPersonCamera))]
public class SaveCameraOffset : MonoBehaviour
{
    ThirdPersonCamera tpc;
    float savedValue = 2f;

    public string group = "";
    public string key = "UnnamedCameraOffset";

    public UnityEvent<float> onStart;

    IniStorage ini;
    int id = 0;
    float wait;

    void Awake()
    {
        tpc = GetComponent<ThirdPersonCamera>();
        savedValue = 2;
        ini = new IniStorage(GlobalVariables.configPath);
        wait = UnityEngine.Random.Range(0.15f, 0.65f);
        id = Random.Range(0, 10000);
    }

    void Start()
    {
        if (ini.Contains(group, $"f{key}"))
        {
            savedValue = ini.GetFloat(group, $"f{key}");

            if (tpc != null)
            {
                tpc.offsetFromTarget.y = savedValue;
            }
        }

        onStart.Invoke(savedValue);
    }

    public void SaveValue(float value)
    {
        string n = gameObject.name;
        Utilities.FunctionTimer.Create(this, () => {
            ini.Load(GlobalVariables.configPath);

            savedValue = value;
            ini.Set(group, $"f{key}", savedValue);

            ini.Save();
        }, wait, $"SaveCameraOffset_{id}_{n}_savevalue_delay", false, true);
    }
}
