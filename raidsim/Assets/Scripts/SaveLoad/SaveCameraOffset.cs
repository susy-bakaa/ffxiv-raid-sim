// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using UnityEngine;
using UnityEngine.Events;
using dev.illa4257;
using dev.susybaka.raidsim.Core;
using dev.susybaka.raidsim.Inputs;
using dev.susybaka.Shared;

namespace dev.susybaka.raidsim.SaveLoad
{
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

        private void Awake()
        {
            tpc = GetComponent<ThirdPersonCamera>();
            savedValue = 2;
            ini = new IniStorage(GlobalVariables.configPath);
            wait = UnityEngine.Random.Range(0.15f, 0.65f);
            id = Random.Range(0, 10000);
        }

        private void Start()
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
}