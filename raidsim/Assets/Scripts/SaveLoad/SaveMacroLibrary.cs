// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using System;
using System.Text;
using dev.illa4257;
using dev.susybaka.raidsim.Actions;
using dev.susybaka.raidsim.Core;
using UnityEngine;
using static dev.susybaka.raidsim.Core.GlobalData;

namespace dev.susybaka.raidsim.SaveLoad
{
    public sealed class SaveMacroLibrary : MonoBehaviour
    {
        [SerializeField] private MacroLibrary library;
        [SerializeField] private string group = "Global";
        [SerializeField] private string key = "sMacroGrid";
        [SerializeField] private bool loadOnStart = true;

        private IniStorage ini;
        private bool dirty;

        private void Awake()
        {
            ini = new IniStorage(GlobalVariables.configPath);
        }

        private void OnEnable()
        {
            library.OnAnyChanged += MarkDirty;
        }

        private void OnDisable()
        {
            library.OnAnyChanged -= MarkDirty;
        }

        private void Start()
        {
            if (loadOnStart)
                LoadValue();
        }

        private void MarkDirty() => dirty = true;

        public void SaveValue()
        {
            if (!dirty)
                return;

            dirty = false;

            ini.Load(GlobalVariables.configPath);

            var list = new System.Collections.Generic.List<MacroSlotSave>();

            for (int i = 0; i < MacroLibrary.Count; i++)
            {
                var e = library.Get(i);
                if (!e.isValid)
                    continue;

                if (string.IsNullOrWhiteSpace(e.name) && string.IsNullOrWhiteSpace(e.body)) 
                    continue;

                list.Add(new MacroSlotSave { index = i, entry = e });
            }

            var snap = new MacroLibrarySnapshot { slots = list.ToArray() };
            var json = JsonUtility.ToJson(snap);

            //json = Convert.ToBase64String(Encoding.UTF8.GetBytes(json));

            ini.Set(group, key, json);
            ini.Save();
        }

        public void LoadValue()
        {
            ini.Load(GlobalVariables.configPath);

            var json = ini.GetString(group, key);
            if (string.IsNullOrWhiteSpace(json))
                return;

            var snap = JsonUtility.FromJson<MacroLibrarySnapshot>(json);
            if (snap?.slots == null)
                return;

            for (int i = 0; i < MacroLibrary.Count; i++)
                library.Clear(i);

            foreach (var s in snap.slots)
            {
                if (s.index < 0 || s.index >= MacroLibrary.Count)
                    continue;

                library.Set(s.index, s.entry);
            }
        }

        public void ClearValue()
        {
            ini.Load(GlobalVariables.configPath);

            ini.Remove(group, key);

            ini.Save();
        }
    }
}