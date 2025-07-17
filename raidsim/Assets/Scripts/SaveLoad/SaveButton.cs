using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using dev.illa4257;
using dev.susybaka.raidsim.Core;
using dev.susybaka.Shared;

namespace dev.susybaka.raidsim.SaveLoad
{
    public class SaveButton : MonoBehaviour
    {
        Button button;
        int savedValue = 0;

        public string group = "";
        public string key = "UnnamedButton";
        public bool defaultValue = false;
        public bool runInAwake = false;
        public bool runOnReset = false;

        public UnityEvent<bool> onStart;

        IniStorage ini;
        int id = 0;
        float wait;

        private void Awake()
        {
            button = GetComponent<Button>();
            savedValue = 0;
            ini = new IniStorage(GlobalVariables.configPath);
            wait = UnityEngine.Random.Range(0.15f, 0.65f);
            id = Random.Range(0, 10000);

            if (runInAwake)
            {
                Begin();
            }
        }

        private void Start()
        {
            if (runOnReset && FightTimeline.Instance != null)
            {
                FightTimeline.Instance.onReset.AddListener(Begin);
            }

            if (!runInAwake)
            {
                Begin();
            }
        }

        private void OnDestroy()
        {
            if (FightTimeline.Instance != null && runOnReset)
            {
                FightTimeline.Instance.onReset.RemoveListener(Begin);
            }
        }

        private void Begin()
        {
            if (ini.Contains(group, $"i{key}"))
            {
                savedValue = ini.GetInt(group, $"i{key}");

                bool result = savedValue.ToBool();

                if (result && button != null)
                {
                    button.onClick.Invoke();
                }
                onStart.Invoke(result);
            }
            else
            {
                onStart.Invoke(defaultValue);
            }
        }

        public void SaveValue(bool value)
        {
            string n = gameObject.name;
            if (button != null)
                n = button.gameObject.name;
            Utilities.FunctionTimer.Create(this, () => {
                ini.Load(GlobalVariables.configPath);

                savedValue = value.ToInt();
                ini.Set(group, $"i{key}", savedValue);

                ini.Save();
            }, wait, $"SaveButton_{id}_{n}_savevalue_delay", false, true);
        }
    }
}