using UnityEngine;
using UnityEngine.Events;
using dev.illa4257;
using dev.susybaka.raidsim.Core;
using dev.susybaka.raidsim.UI;
using dev.susybaka.Shared;

namespace dev.susybaka.raidsim.SaveLoad
{
    [RequireComponent(typeof(SaveToggleLinker))]
    public class SaveToggleLinker : MonoBehaviour
    {
        ToggleLinker toggleLinker;
        int savedValue = 0;

        public string group = "";
        public string key = "UnnamedToggleLinker";

        public UnityEvent<int> onStart;

        IniStorage ini;

        private void Awake()
        {
            toggleLinker = GetComponent<ToggleLinker>();
            savedValue = 0;
            ini = new IniStorage(GlobalVariables.configPath);
        }

        private void Start()
        {
            if (ini.Contains(group, $"i{key}"))
            {
                savedValue = ini.GetInt(group, $"i{key}");

                for (int i = 0; i < toggleLinker.toggles.Count; i++)
                {
                    if (i == savedValue)
                    {
                        //Debug.Log($"ToggleSetTo num {i} true");
                        toggleLinker.toggles[i].SetIsOnWithoutNotify(true);
                    }
                    else
                    {
                        //Debug.Log($"ToggleSetTo num {i} false");
                        toggleLinker.toggles[i].SetIsOnWithoutNotify(false);
                    }
                }

                onStart.Invoke(savedValue);
            }
        }

        public void SaveValue(int value)
        {
            ini.Load(GlobalVariables.configPath);

            savedValue = value;
            ini.Set(group, $"i{key}", savedValue);

            Utilities.FunctionTimer.Create(this, () => ini.Save(), 0.5f, $"{group}_{key}_togglelinker_savevalue_delay", true, false);
        }
    }
}