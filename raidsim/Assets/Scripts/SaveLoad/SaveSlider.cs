using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using dev.illa4257;
using dev.susybaka.raidsim.Core;
using dev.susybaka.Shared;

namespace dev.susybaka.raidsim.SaveLoad
{
    [RequireComponent(typeof(Slider))]
    public class SaveSlider : MonoBehaviour
    {
        Slider slider;
        float savedValue = 100;

        public string group = "";
        public string key = "UnnamedSlider";

        public UnityEvent<float> onStart;

        IniStorage ini;

        private void Awake()
        {
            slider = GetComponent<Slider>();
            savedValue = 100f;
            ini = new IniStorage(GlobalVariables.configPath);
        }

        private void Start()
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
            ini.Load(GlobalVariables.configPath);

            savedValue = value;
            ini.Set(group, $"f{key}", savedValue);

            Utilities.FunctionTimer.Create(this, () => ini.Save(), 0.5f, $"{group}_{key}_slider_savevalue_delay", true, false);
        }
    }
}