// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using System;
using UnityEngine;

namespace dev.susybaka.raidsim.UI
{
    [CreateAssetMenu(fileName = "Macro Icon Database", menuName = "FFXIV/New Macro Icon Database")]
    public sealed class MacroIconData : ScriptableObject
    {
        [Serializable] public struct Entry { public string id; [NaughtyAttributes.ShowAssetPreview] public Sprite sprite; }
        [SerializeField] private Entry[] entries;
        public Entry[] Entries => entries;

        public Sprite Get(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return null;
            for (int i = 0; i < entries.Length; i++)
                if (entries[i].id == id)
                    return entries[i].sprite;
            return null;
        }
    }
}