// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using System;
using UnityEngine;

namespace dev.susybaka.raidsim.Actions
{
    public sealed class MacroLibrary : MonoBehaviour
    {
        public const int GridW = 10;
        public const int GridH = 10;
        public const int Count = GridW * GridH;

        [SerializeField] private MacroEntry[] entries = new MacroEntry[Count];

        public event Action<int> OnMacroChanged; // slot index
        public event Action OnAnyChanged;
        public event Action OnBulkChanged;

        public MacroEntry Get(int index) => entries[index];

        public void Set(int index, MacroEntry entry)
        {
            entries[index] = entry;
            OnMacroChanged?.Invoke(index);
            OnAnyChanged?.Invoke();
        }

        public void Clear(int index)
        {
            entries[index] = default;
            entries[index].isValid = false;
            entries[index].name = "";
            entries[index].body = "";
            entries[index].iconMode = MacroIconMode.Default;
            entries[index].customIconId = "";
            entries[index].miconType = MacroMiconType.None;
            entries[index].miconName = "";

            OnMacroChanged?.Invoke(index);
            OnAnyChanged?.Invoke();
        }

        public void ClearAll()
        {
            for (int i = 0; i < Count; i++)
            {
                entries[i] = default;
                entries[i].isValid = false;
                entries[i].name = "";
                entries[i].body = "";
                entries[i].iconMode = MacroIconMode.Default;
                entries[i].customIconId = "";
                entries[i].miconType = MacroMiconType.None;
                entries[i].miconName = "";
            }
        }

        public void NotifyBulkChanged()
        {
            OnBulkChanged?.Invoke();
        }

        public bool IsValid(int index) => entries[index].isValid;

        // Convenience: stable ID stored in hotbar binding
        public static string MacroIdFromIndex(int index) => index.ToString();
        public static bool TryParseMacroId(string id, out int index) => int.TryParse(id, out index);
    }
}