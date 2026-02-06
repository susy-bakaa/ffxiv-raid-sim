// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using UnityEngine;
using UnityEngine.Events;
using dev.illa4257;
using dev.susybaka.raidsim.Core;
using dev.susybaka.Shared;

namespace dev.susybaka.raidsim.SaveLoad
{
    public class SaveScale : MonoBehaviour
    {
        [SerializeField] private RectTransform target;
        float widthX = 0;
        float heightY = 0;
        float defaultWidth;
        float defaultHeight;

        public string group = "";
        public string key = "UnnamedScale";
        public string id = string.Empty;
        [SerializeField] private float randomDelay = 0.5f;

        private string keyWidthX { get { return $"{key}X"; } }
        private string keyHeightY { get { return $"{key}Y"; } }

        public UnityEvent<Vector2> onStart;

        IniStorage ini;

        private void Awake()
        {
            defaultWidth = target.sizeDelta.x;
            defaultHeight = target.sizeDelta.y;
            widthX = defaultWidth;
            heightY = defaultHeight;
            ini = new IniStorage(GlobalVariables.configPath);
        }

        private void Start()
        {
            randomDelay = Random.Range(randomDelay, randomDelay + 0.2f);
            Utilities.FunctionTimer.Create(this, () => OnStart(), Random.Range(1f, 1.25f), $"{group}_{key}{id}_savescale_onstart_delay", true, false);
        }

        private void OnStart()
        {
            if (ini.Contains(group, $"f{keyWidthX}{id}") && ini.Contains(group, $"f{keyHeightY}{id}"))
            {
                widthX = ini.GetFloat(group, $"f{keyWidthX}{id}");
                heightY = ini.GetFloat(group, $"f{keyHeightY}{id}");

                target.sizeDelta = new Vector2(widthX, heightY);
                onStart.Invoke(target.sizeDelta);
            }
            else
            {
                target.sizeDelta = new Vector2(defaultWidth, defaultHeight);
            }
        }

        public void SaveValue(float x, float y)
        {
            SaveValue(new Vector2(x, y));
        }

        public void SaveValue(Vector2 value)
        {
            Utilities.FunctionTimer.Create(this, () => {
                ini.Load(GlobalVariables.configPath);
                widthX = value.x;
                heightY = value.y;
                ini.Set(group, $"f{keyWidthX}{id}", widthX);
                ini.Set(group, $"f{keyHeightY}{id}", heightY);
                ini.Save();
            }, randomDelay, $"{group}_{key}{id}_savescale_savevalue_delay", true, false);
        }

        public void Reload()
        {
            Start();
        }
    }
}