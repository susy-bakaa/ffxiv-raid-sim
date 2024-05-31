using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class SaveSlider : MonoBehaviour
{
    Slider slider;
    float savedValue = 0;

    public string group = "";
    public string key = "UnnamedSlider";

    public UnityEvent<float> onStart;

    IniStorage ini;

    void Awake()
    {
        slider = GetComponent<Slider>();
        savedValue = 0f;
        ini = new IniStorage(GlobalVariables.configPath);
    }

    void Start()
    {
        if (ini.Contains(group, $"f{key}"))
        {
            savedValue = ini.GetFloat(group, $"f{key}");

            slider.value = savedValue;
            slider.onValueChanged.Invoke(savedValue);
            onStart.Invoke(savedValue);
        }
    }

    public void SaveValue(float value)
    {
        savedValue = value;
        ini.Set(group, $"f{key}", savedValue);
        ini.Save();
    }
}
